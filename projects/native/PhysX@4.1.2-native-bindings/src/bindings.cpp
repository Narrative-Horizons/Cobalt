#define _USE_MATH_DEFINES
#include <cmath>
#include <iostream>
#include <unordered_map>
#include <tuple>

#include <PxPhysicsAPI.h>

#define PHYSX_BINDING_EXPORT extern "C" __declspec(dllexport) inline

bool IsCcdActive(physx::PxFilterData& filterData)
{
	return true;
	//return filterData.word3 & CCD_FLAG ? true : false;
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
	const float pitch = std::abs(sinp) >= 1.0 ? std::copysign(M_PI_2, sinp) : (float)std::asin(sinp); // x

	const float siny_cosp = 2.0f * (rotation.w * rotation.z + rotation.x * rotation.y);
	const float cosy_cosp = 1.0f - 2.0f * (rotation.y * rotation.y + rotation.z * rotation.z);
	const float yaw = std::atan2(siny_cosp, cosy_cosp); // y

	return std::make_tuple(pitch, yaw, roll);
}

static physx::PxFoundation* _foundation;
static physx::PxPvd* _pvd;
static physx::PxPvdTransport* _transport;
static physx::PxPhysics* _physics;
static physx::PxCooking* _cooking;
static physx::PxDefaultCpuDispatcher* _cpuDispatcher;
static physx::PxSimulationFilterShader* _simulationFilterShader;
static physx::PxScene* _scene;
static physx::PxCudaContextManager* _cudaContext;

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

	/*PxFilterFlags filterFlags = PxDefaultSimulationFilterShader(aAttributes0,
		filterData0, aAttributes1, filterData1, pairFlags, constantBlock, constantBlockSize);

	if (isCCDActive(filterData0) && isCCDActive(filterData1))
	{
		pairFlags |= PxPairFlag::eCCD_LINEAR;
	}

	return filterFlags;*/
}

PHYSX_BINDING_EXPORT void init()
{
	using namespace physx;

	_foundation = PxCreateFoundation(PX_PHYSICS_VERSION, defaultAllocatorCallback,
		defaultErrorCallback);

	if (_foundation == nullptr)
	{
		printf("PxCreateFoundation failed!\n");
		return;
	}

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	const PxCudaContextManagerDesc cudaDesc;

	_cudaContext = PxCreateCudaContextManager(*_foundation, cudaDesc);

	if (_cudaContext == nullptr)
	{
		std::cerr << "PxCreateCudaContextManager failed!" << std::endl;
	}

	if (_cudaContext != nullptr && _cudaContext->contextIsValid() == false)
	{
		std::cerr << "Cuda Context is invalid!" << std::endl;
	}
#endif

	const PxTolerancesScale scale;

	_pvd = PxCreatePvd(*_foundation);
	_transport = PxDefaultPvdSocketTransportCreate("127.0.0.1", 5425, 10);
	_pvd->connect(*_transport, PxPvdInstrumentationFlag::eALL);

	_physics = PxCreatePhysics(PX_PHYSICS_VERSION, *_foundation, scale, true, nullptr);
	if (_physics == nullptr)
	{
		std::cerr << "PxCreatePhysics failed!" << std::endl;
		return;
	}

	PxCookingParams params(scale);
	params.meshWeldTolerance = 0.001f;
	params.meshPreprocessParams = PxMeshPreprocessingFlags(PxMeshPreprocessingFlag::eWELD_VERTICES);
	_cooking = PxCreateCooking(PX_PHYSICS_VERSION, *_foundation, params);
	if (_cooking == nullptr)
	{
		std::cerr << "PxCreateCooking failed!" << std::endl;
		return;
	}

	if (!PxInitExtensions(*_physics, _pvd))
	{
		std::cerr << "PxInitExtensions failed!" << std::endl;
		return;
	}

	PxCookingParams cookingParams(scale);
	// disable mesh cleaning - perform mesh validation on development configurations
	cookingParams.meshPreprocessParams |= PxMeshPreprocessingFlag::eDISABLE_CLEAN_MESH;
	// disable edge precompute, edges are set for each triangle, slows contact generation
	cookingParams.meshPreprocessParams |= PxMeshPreprocessingFlag::eDISABLE_ACTIVE_EDGES_PRECOMPUTE;
	_cooking->setParams(params);

	// Create the scene
	PxSceneDesc sceneDesc(scale);
	sceneDesc.gravity = PxVec3(0, -9.81f, 0);

	if (!sceneDesc.cpuDispatcher)
	{
		_cpuDispatcher = PxDefaultCpuDispatcherCreate(1);
		if (_cpuDispatcher == nullptr)
		{
			std::cerr << "PxDefaultCpuDispatcherCreate failed!" << std::endl;
			return;
		}
		sceneDesc.cpuDispatcher = _cpuDispatcher;
	}

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	// Set GPU dispatcher
#endif

	sceneDesc.flags |= PxSceneFlag::eENABLE_CCD;
	sceneDesc.filterShader = gameCollisionFilterShader;

	_scene = _physics->createScene(sceneDesc);
	//_scene->setSimulationEventCallback() TODO
	_scene->simulate(0.02f); // Prewarm
}

PHYSX_BINDING_EXPORT void destroy()
{
	PxCloseExtensions();

	_scene->fetchResults();
	_scene->release();
	_scene = nullptr;

	_cpuDispatcher->release();
	_cpuDispatcher = nullptr;

#if defined(PX_SUPPORT_GPU_PHYSX) && defined(_WIN32)
	if (_cudaContext != nullptr)
	{
		_cudaContext->release();
		_cudaContext = nullptr;
	}
#endif

	_physics->release();
	_physics = nullptr;

	_simulationFilterShader = nullptr;

	_cooking->release();
	_cooking = nullptr;

	_pvd->release();
	_pvd = nullptr;

	_transport->release();
	_transport = nullptr;

	_foundation->release();
	_foundation = nullptr;
}

PHYSX_BINDING_EXPORT void simulate()
{
	if (_simulationComplete)
	{
		_scene->simulate(1.0f / 60.0f);
	}
}

struct VertexData
{
	float x, y, z;
};

struct MeshData
{
	VertexData* vertices;
	uint32_t vertexCount;
	
	uint32_t* indices;
	uint32_t indexCount;

	uint32_t uuid;
};

static std::unordered_map<uint32_t, physx::PxShape*> _meshShapes;
PHYSX_BINDING_EXPORT void create_mesh_shape(MeshData* meshData)
{
	if(_meshShapes.find(meshData->uuid) != _meshShapes.cend())
	{
		// TODO: Shape already exists
		return;
	}
	
	physx::PxTriangleMeshDesc desc;
	desc.points.count = meshData->vertexCount;
	desc.points.stride = sizeof(VertexData);
	desc.points.data = meshData->vertices;

	desc.triangles.count = meshData->indexCount / 3;
	desc.triangles.stride = sizeof(uint32_t) * 3;
	desc.triangles.data = meshData->indices;

#if defined(_DEBUG)
	const bool res = _cooking->validateTriangleMesh(desc);
	PX_ASSERT(res);
#endif

	physx::PxTriangleMesh* mesh = _cooking->createTriangleMesh(desc, _physics->getPhysicsInsertionCallback());

	physx::PxTriangleMeshGeometry geom = physx::PxTriangleMeshGeometry(mesh);
	physx::PxMaterial* mat = _physics->createMaterial(1, 1, 1);
	physx::PxShape* shape = _physics->createShape(geom, *mat, false);
	
	_meshShapes[meshData->uuid] = shape;
	// Disc Serialization
	/*
	physx::PxDefaultMemoryOutputStream writeBuffer;
	physx::PxTriangleMeshCookingResult::Enum result;
	const bool status = _cooking->cookTriangleMesh(desc, writeBuffer, &result);
	if (!status)
		return;

	physx::PxDefaultMemoryInputData readBuffer(writeBuffer.getData(), writeBuffer.getSize());

	physx::PxTriangleMesh* mesh = _physics->createTriangleMesh(readBuffer);*/
}

static std::unordered_map<uint64_t, physx::PxRigidBody*> _actors;
PHYSX_BINDING_EXPORT void create_mesh_collider(uint64_t id, uint32_t shapeId, float x, float y, float z)
{
	if(_actors.find(id) != _actors.cend())
	{
		// TODO: Already exists
		return;
	}
	
	if (_meshShapes.find(shapeId) == _meshShapes.cend())
	{
		// TODO: Shape doesn't exist
		return;
	}
	
	physx::PxRigidDynamic* dyn = _physics->createRigidDynamic({ x, y, z });
	dyn->attachShape(*_meshShapes[shapeId]);
	dyn->userData = reinterpret_cast<void*>(id);

	_actors[id] = dyn;
}

PHYSX_BINDING_EXPORT SimulationResults fetch_results()
{
	_simulationComplete = _scene->fetchResults();

	if (_simulationComplete)
	{
		_transformScratchBuffer.clear();
		_transformScratchBuffer.reserve(_actors.size());

		// populate the results buffer
		for (auto& [id, actor] : _actors)
		{
			uint32_t generation = id >> 32;
			uint32_t identifier = (id & std::numeric_limits<uint32_t>::max());

			const auto transform = actor->getGlobalPose();
			const auto [rx, ry, rz] = to_euler(transform.q);

			_transformScratchBuffer.emplace_back(transform.p.x, transform.p.y, transform.p.z, rx, ry, rz, generation, identifier);
		}
	}

	return
	{
		_simulationComplete ? _transformScratchBuffer.data() : nullptr,
		_simulationComplete ? static_cast<uint32_t>(_transformScratchBuffer.size()) : 0u
	};
}