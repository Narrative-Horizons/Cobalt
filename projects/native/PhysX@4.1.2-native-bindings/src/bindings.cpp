#define _USE_MATH_DEFINES
#include <cmath>
#include <iostream>
#include <unordered_map>
#include <tuple>

#include <PxPhysicsAPI.h>

#define PHYSX_BINDING_EXPORT extern "C" __declspec(dllexport) inline

// Disc Serialization
/*
physx::PxDefaultMemoryOutputStream writeBuffer;
physx::PxTriangleMeshCookingResult::Enum result;
const bool status = _cooking->cookTriangleMesh(desc, writeBuffer, &result);
if (!status)
	return;

physx::PxDefaultMemoryInputData readBuffer(writeBuffer.getData(), writeBuffer.getSize());

physx::PxTriangleMesh* mesh = _physics->createTriangleMesh(readBuffer);
*/

struct PhysXVertexData
{
	float x, y, z;
};

struct PhysXMeshData
{
	PhysXVertexData* vertices;
	uint32_t vertexCount;

	uint32_t* indices;
	uint32_t indexCount;

	uint32_t uuid;
};

bool IsCcdActive(physx::PxFilterData& filterData)
{
	return true;
}

struct PhysicsTransform
{
	float x, y, z;
	float rx, ry, rz;
	uint32_t generation, identifier;

	PhysicsTransform(float x, float y, float z, float rx, float ry, float rz, uint32_t generation, uint32_t identifier)
		: x(x), y(y), z(z), rx(rx), ry(ry), rz(rz), generation(generation), identifier(identifier)
	{
	}
};

struct SimulationResults
{
	PhysicsTransform* data;
	uint32_t size;
};

static std::tuple<float, float, float> to_euler(const physx::PxQuat& rotation)
{
	// TODO: Trig LUTs?
	const float sinr_cosp = 2.0f * (rotation.w * rotation.x + rotation.y * rotation.z);
	const float cosr_cosp = 1.0f - 2.0 * (rotation.x * rotation.x + rotation.y * rotation.y);
	const float roll = std::atan2(sinr_cosp, cosr_cosp); // z

	const float sinp = 2.0f * (rotation.w * rotation.y - rotation.z * rotation.x);
	const float pitch = std::abs(sinp) >= 1.0 ? std::copysign(M_PI_2, sinp) : static_cast<float>(std::asin(sinp)); // x

	const float siny_cosp = 2.0f * (rotation.w * rotation.z + rotation.x * rotation.y);
	const float cosy_cosp = 1.0f - 2.0f * (rotation.y * rotation.y + rotation.z * rotation.z);
	const float yaw = std::atan2(siny_cosp, cosy_cosp); // y

	const float radtodeg = 180.0f / M_PI;

	return std::make_tuple(pitch * radtodeg, yaw * radtodeg, roll * radtodeg);
}

static physx::PxFoundation* foundation;
static physx::PxPvd* pvd;
static physx::PxPvdTransport* transport;
static physx::PxPhysics* physics;
static physx::PxCooking* cooking;
static physx::PxDefaultCpuDispatcher* cpuDispatcher;
static physx::PxSimulationFilterShader* simulationFilterShader;
static physx::PxScene* scene;
static physx::PxCudaContextManager* cudaContext;

static physx::PxDefaultErrorCallback defaultErrorCallback;
static physx::PxDefaultAllocator defaultAllocatorCallback;

static std::vector<PhysicsTransform> _transformScratchBuffer;
static bool _simulationComplete = true;

physx::PxFilterFlags gameCollisionFilterShader(
	const physx::PxFilterObjectAttributes aAttributes0, const physx::PxFilterData aFilterData0,
	const physx::PxFilterObjectAttributes aAttributes1, const physx::PxFilterData aFilterData1,
	physx::PxPairFlags& aPairFlags, const void* aConstantBlock, physx::PxU32 aConstantBlockSize)
{
	// let triggers through
	if (physx::PxFilterObjectIsTrigger(aAttributes0) || physx::PxFilterObjectIsTrigger(aAttributes1))
	{
		aPairFlags = physx::PxPairFlag::eTRIGGER_DEFAULT;
		return physx::PxFilterFlag::eDEFAULT;
	}

	// generate contacts for all that were not filtered above
	aPairFlags = physx::PxPairFlag::eCONTACT_DEFAULT;

	// trigger the contact callback for pairs (A,B) where
	// the filtermask of A contains the ID of B and vice versa.
	if ((aFilterData0.word0 & aFilterData1.word1) && (aFilterData1.word0 & aFilterData0.word1))
		aPairFlags |= physx::PxPairFlag::eNOTIFY_TOUCH_FOUND;

	return physx::PxFilterFlag::eDEFAULT;
}

PHYSX_BINDING_EXPORT void init()
{
	using namespace physx;

	foundation = PxCreateFoundation(PX_PHYSICS_VERSION, defaultAllocatorCallback,
	                                defaultErrorCallback);

	if (foundation == nullptr)
	{
		std::cerr << "PxCreateFoundation failed!" << std::endl;
		return;
	}

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	const PxCudaContextManagerDesc cudaDesc;

	cudaContext = PxCreateCudaContextManager(*foundation, cudaDesc);

	if (cudaContext == nullptr)
	{
		std::cerr << "PxCreateCudaContextManager failed!" << std::endl;
	}

	if (cudaContext != nullptr && cudaContext->contextIsValid() == false)
	{
		std::cerr << "Cuda Context is invalid!" << std::endl;
	}
#endif

	const PxTolerancesScale scale;

	pvd = PxCreatePvd(*foundation);
	transport = PxDefaultPvdSocketTransportCreate("127.0.0.1", 5425, 10);
	pvd->connect(*transport, PxPvdInstrumentationFlag::eALL);

	physics = PxCreatePhysics(PX_PHYSICS_VERSION, *foundation, scale, true, nullptr);
	if (physics == nullptr)
	{
		std::cerr << "PxCreatePhysics failed!" << std::endl;
		return;
	}

	PxCookingParams params(scale);
	params.meshWeldTolerance = 0.001f;
	params.meshPreprocessParams = PxMeshPreprocessingFlags(PxMeshPreprocessingFlag::eWELD_VERTICES);
	cooking = PxCreateCooking(PX_PHYSICS_VERSION, *foundation, params);
	if (cooking == nullptr)
	{
		std::cerr << "PxCreateCooking failed!" << std::endl;
		return;
	}

	if (!PxInitExtensions(*physics, pvd))
	{
		std::cerr << "PxInitExtensions failed!" << std::endl;
		return;
	}

	PxCookingParams cookingParams(scale);
	// disable mesh cleaning - perform mesh validation on development configurations
	cookingParams.meshPreprocessParams |= PxMeshPreprocessingFlag::eDISABLE_CLEAN_MESH;
	// disable edge precompute, edges are set for each triangle, slows contact generation
	cookingParams.meshPreprocessParams |= PxMeshPreprocessingFlag::eDISABLE_ACTIVE_EDGES_PRECOMPUTE;
	cooking->setParams(params);

	// Create the scene
	PxSceneDesc sceneDesc(scale);
	sceneDesc.gravity = PxVec3(0, -9.81f, 0);

	if (!sceneDesc.cpuDispatcher)
	{
		cpuDispatcher = PxDefaultCpuDispatcherCreate(1);
		if (cpuDispatcher == nullptr)
		{
			std::cerr << "PxDefaultCpuDispatcherCreate failed!" << std::endl;
			return;
		}
		sceneDesc.cpuDispatcher = cpuDispatcher;
	}

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	// Set GPU dispatcher
#endif

	sceneDesc.flags |= PxSceneFlag::eENABLE_CCD;
	sceneDesc.filterShader = gameCollisionFilterShader;

	scene = physics->createScene(sceneDesc);
	//_scene->setSimulationEventCallback() TODO
	scene->simulate(0.02f); // Prewarm
}

PHYSX_BINDING_EXPORT void destroy()
{
	PxCloseExtensions();

	scene->fetchResults();
	scene->release();
	scene = nullptr;

	cpuDispatcher->release();
	cpuDispatcher = nullptr;

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	if (cudaContext != nullptr)
	{
		cudaContext->release();
		cudaContext = nullptr;
	}
#endif

	physics->release();
	physics = nullptr;

	simulationFilterShader = nullptr;

	cooking->release();
	cooking = nullptr;

	pvd->release();
	pvd = nullptr;

	transport->release();
	transport = nullptr;

	foundation->release();
	foundation = nullptr;
}

PHYSX_BINDING_EXPORT void simulate()
{
	if (_simulationComplete)
	{
		scene->simulate(1.0f / 60.0f);
	}
}

static std::unordered_map<uint32_t, physx::PxShape*> shapes;
PHYSX_BINDING_EXPORT void create_mesh_shape(PhysXMeshData meshData)
{
	if(shapes.find(meshData.uuid) != shapes.cend())
	{
		// TODO: Shape already exists
		return;
	}
	
	physx::PxTriangleMeshDesc desc;
	desc.points.count = meshData.vertexCount;
	desc.points.stride = sizeof(PhysXVertexData);
	desc.points.data = meshData.vertices;

	desc.triangles.count = meshData.indexCount / 3;
	desc.triangles.stride = sizeof(uint32_t) * 3;
	desc.triangles.data = meshData.indices;

#if defined(_DEBUG)
	//const bool res = cooking->validateTriangleMesh(desc);
	//PX_ASSERT(res);
#endif

	//physx::PxTriangleMesh* mesh = cooking->createTriangleMesh(desc, physics->getPhysicsInsertionCallback());

	//const physx::PxTriangleMeshGeometry geom = physx::PxTriangleMeshGeometry(mesh);
	physx::PxMaterial* mat = physics->createMaterial(1, 1, 1);

	physx::PxBoxGeometry* boxGeom = new physx::PxBoxGeometry(1, 1, 1);
	
	physx::PxShape* shape = physics->createShape(*boxGeom, *mat, true);

	shapes[meshData.uuid] = shape;
}

static std::unordered_map<uint64_t, physx::PxRigidBody*> actors;
PHYSX_BINDING_EXPORT void create_mesh_collider(const uint64_t id, const uint32_t shapeId, const float x, const float y, const float z)
{
	if(actors.find(id) != actors.cend())
	{
		// TODO: Already exists
		return;
	}
	
	if (shapes.find(shapeId) == shapes.cend())
	{
		// TODO: Shape doesn't exist
		return;
	}
	
	physx::PxRigidDynamic* dyn = physx::PxCreateDynamic(*physics, { x, y, z }, *shapes[shapeId], 10.0f);
	dyn->userData = reinterpret_cast<void*>(id);
	dyn->setLinearVelocity({ 1, 0, 0 });
	dyn->setAngularVelocity({ 0, 0, 5 });
	scene->addActor(*dyn);
	
	actors[id] = dyn;
}

PHYSX_BINDING_EXPORT SimulationResults fetch_results()
{
	_simulationComplete = scene->fetchResults();

	if (_simulationComplete)
	{
		_transformScratchBuffer.clear();
		_transformScratchBuffer.reserve(actors.size());

		// populate the results buffer
		for (auto& [id, actor] : actors)
		{
			uint32_t generation = id >> 32;
			uint32_t identifier = (id & std::numeric_limits<uint32_t>::max());

			const auto transform = actor->getGlobalPose();
			const auto [rx, ry, rz] = to_euler(transform.q);

			_transformScratchBuffer.emplace_back(transform.p.x, transform.p.y, transform.p.z, rx, ry, rz, generation, identifier);
		}
	}

	SimulationResults x =
	{
		_simulationComplete ? _transformScratchBuffer.data() : nullptr,
		_simulationComplete ? static_cast<uint32_t>(_transformScratchBuffer.size()) : 0u
	};

	return x;
}

PHYSX_BINDING_EXPORT SimulationResults get_results()
{
	if (_simulationComplete)
	{
		_transformScratchBuffer.reserve(actors.size());
	}
	
	SimulationResults x =
	{
		_simulationComplete ? _transformScratchBuffer.data() : nullptr,
		_simulationComplete ? static_cast<uint32_t>(_transformScratchBuffer.size()) : 0u
	};

	return x;
}

PHYSX_BINDING_EXPORT void sync()
{
	if (_simulationComplete)
	{
		for (PhysicsTransform trans : _transformScratchBuffer)
		{
			uint64_t id = static_cast<uint64_t>(trans.generation) << 32 | trans.identifier;

			actors[id]->setGlobalPose({ trans.x, trans.y, trans.z, {0, 0, 0, 1} });
		}
	}
}