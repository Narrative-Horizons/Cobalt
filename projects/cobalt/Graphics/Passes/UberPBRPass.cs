using System.Runtime.InteropServices;
using Cobalt.Bindings.Vulkan;
using Cobalt.Core;
using Cobalt.Graphics.Enums;
using Cobalt.Math;

namespace Cobalt.Graphics.Passes
{
    public class UberPBRPass : Pass
    {
        private readonly Shader _opaqueShader;
        private readonly VK.RenderPass _renderPass;

        private readonly uint _framesInFlight;

        private readonly Framebuffer[] _framebuffers;
        private readonly ImageView[] _frameBufferColorImageViews;
        private readonly ImageView[] _frameBufferDepthImageViews;

        private readonly Buffer _vertexBuffer;
        private readonly Buffer _indexBuffer;
        private readonly Buffer _indirectBuffer;

        private readonly Buffer[] _sceneDataBuffer;
        private readonly Buffer[] _objectDataBuffer;
        private readonly Buffer[] _materialDataBuffer;

        //private ImageView[] _textures;
        //private VK.Sampler[] _textureSamplers;

        private readonly Descriptor[] _sceneDescriptor;
        private readonly Descriptor[] _objectDescriptor;
        private readonly Descriptor[] _materialDescriptor;
        private readonly Descriptor[] _textureDescriptor;

        public struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
            public Vector3 normal;
            public Vector3 tangent;
            public Vector3 bitangent;
            public Vector4 color;
        }

        public struct SceneData
        {
            public Matrix4 view;
            public Matrix4 projection;
            public Matrix4 viewProjection;

            public Vector3 cameraPosition;
            public Vector3 cameraDirection;

            public DirectionalLightComponent directionalLighting;
        }

        public struct ObjectData
        {
            public Matrix4 transform;
            public uint materialID;
            public uint identifier;
            public uint generation;
            public uint padding;
        }

        public struct MaterialData
        {
            public uint albedo;
            public uint normal;
            public uint emission;
            public uint orm;
        }

        public UberPBRPass(Device device, uint framesInFlight)
        {
            _framesInFlight = framesInFlight;

            AttachmentDescription colorDesc = new AttachmentDescription
            {
                initialLayout = (uint) ImageLayout.Undefined,
                finalLayout = (uint) ImageLayout.ColorAttachmentOptimal,
                format = (uint) Format.B8G8R8A8Srgb,
                samples = (uint) SampleCountFlagBits.Count1Bit,
                loadOp = (uint) AttachmentLoadOp.Clear,
                storeOp = (uint) AttachmentStoreOp.Store,
                flags = 0
            };

            AttachmentDescription depthDesc = new AttachmentDescription
            {
                initialLayout = (uint) ImageLayout.Undefined,
                finalLayout = (uint) ImageLayout.DepthStencilAttachmentOptimal,
                format = (uint) Format.D24UnormS8Uint,
                samples = (uint) SampleCountFlagBits.Count1Bit,
                loadOp = (uint) AttachmentLoadOp.Clear,
                storeOp = (uint) AttachmentStoreOp.Store,
                flags = 0
            };

            AttachmentReference colorBuffer = new AttachmentReference
            {
                attachment = 0,
                layout = (uint) ImageLayout.ColorAttachmentOptimal
            };

            AttachmentReference depthBuffer = new AttachmentReference
            {
                attachment = 1,
                layout = (uint) ImageLayout.DepthStencilAttachmentOptimal
            };

            SubpassDescription opaquePass = new SubpassDescription
            {
                colorAttachments = new[] {colorBuffer},
                colorAttachmentCount = 1,
                depthStencilAttachments = new[] {depthBuffer},
                pipelineBindPoint = (uint) PipelineBindPoint.Graphics,
                flags = 0,
            };

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo
            {
                attachments = new[] {colorDesc, depthDesc},
                attachmentCount = 2,
                subpasses = new[] {opaquePass},
                subpassCount = 1
            };

            _renderPass = device.CreateRenderPass(renderPassInfo);

            _framebuffers = new Framebuffer[_framesInFlight];
            _frameBufferColorImageViews = new ImageView[_framesInFlight];
            _frameBufferDepthImageViews = new ImageView[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                ImageCreateInfo colorInfo = new ImageCreateInfo
                {
                    format = (uint) Format.B8G8R8A8Srgb,
                    width = 1280,
                    height = 720,
                    depth = 1,
                    initialLayout = (uint) ImageLayout.Undefined,
                    arrayLayers = 1,
                    samples = (uint) SampleCountFlagBits.Count1Bit,
                    imageType = (uint) ImageType.Type2D,
                    mipLevels = 1,
                    sharingMode = (uint) SharingMode.Exclusive,
                    tiling = (uint) ImageTiling.Optimal,
                    usage = (uint) ImageUsageFlagBits.ColorAttachmentBit
                };

                ImageMemoryCreateInfo colorMemory = new ImageMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.GpuOnly, 
                    requiredFlags = (uint) MemoryPropertyFlagBits.DeviceLocalBit
                };

                Image colorImage = device.CreateImage(colorInfo, colorMemory, "color", i);
                _frameBufferColorImageViews[i] = colorImage.CreateImageView((Format) colorInfo.format, ImageAspectFlagBits.ColorBit);

                ImageCreateInfo depthInfo = new ImageCreateInfo
                {
                    format = (uint) Format.D24UnormS8Uint,
                    width = 1280,
                    height = 720,
                    depth = 1,
                    initialLayout = (uint) ImageLayout.Undefined,
                    arrayLayers = 1,
                    samples = (uint) SampleCountFlagBits.Count1Bit,
                    imageType = (uint) ImageType.Type2D,
                    mipLevels = 1,
                    sharingMode = (uint) SharingMode.Exclusive,
                    tiling = (uint) ImageTiling.Optimal,
                    usage = (uint) ImageUsageFlagBits.DepthStencilAttachmentBit
                };

                ImageMemoryCreateInfo depthMemory = new ImageMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.GpuOnly, 
                    requiredFlags = (uint) MemoryPropertyFlagBits.DeviceLocalBit
                };

                Image depthImage = device.CreateImage(depthInfo, depthMemory, "depth", i);
                _frameBufferDepthImageViews[i] = depthImage.CreateImageView((Format)depthInfo.format, ImageAspectFlagBits.DepthBit);

                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo
                {
                    attachmentCount = 2,
                    attachments =
                        new[] {_frameBufferColorImageViews[i].handle, _frameBufferDepthImageViews[i].handle},
                    height = 720,
                    width = 1280,
                    layers = 1,
                    pass = _renderPass
                };

                _framebuffers[i] = device.CreateFramebuffer(framebufferInfo);
            }

            ShaderCreateInfo opaqueShaderInfo = new ShaderCreateInfo
            {
                pass = _renderPass,
                subPassIndex = 0,
                vertexModulePath = "data/shaders/pbr/opaque_vert.spv",
                fragmentModulePath = "data/shaders/pbr/opaque_frag.spv"
            };

            ShaderLayoutCreateInfo opaqueLayoutInfo = new ShaderLayoutCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetSceneInfo = new ShaderLayoutSetCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetObjectInfo = new ShaderLayoutSetCreateInfo();

            ShaderLayoutBindingCreateInfo opaqueBindingObjectDataInfo = new ShaderLayoutBindingCreateInfo
            {
                bindingIndex = 0,
                type = (uint) DescriptorType.StorageBuffer,
                stageFlags = (uint) ShaderStageFlagBits.VertexBit | (uint) ShaderStageFlagBits.FragmentBit,
                descriptorCount = 1
            };

            ShaderLayoutBindingCreateInfo opaqueBindingMaterialDataInfo = new ShaderLayoutBindingCreateInfo
            {
                bindingIndex = 1,
                type = (uint) DescriptorType.StorageBuffer,
                stageFlags = (uint) ShaderStageFlagBits.FragmentBit,
                descriptorCount = 1
            };

            ShaderLayoutBindingCreateInfo opaqueBindingSceneDataInfo = new ShaderLayoutBindingCreateInfo
            {
                bindingIndex = 0,
                type = (uint) DescriptorType.UniformBuffer,
                stageFlags = (uint) ShaderStageFlagBits.VertexBit | (uint) ShaderStageFlagBits.FragmentBit,
                descriptorCount = 1
            };

            ShaderLayoutBindingCreateInfo opaqueBindingTextureDataInfo = new ShaderLayoutBindingCreateInfo
            {
                bindingIndex = 2,
                type = (uint) DescriptorType.CombinedImageSampler,
                stageFlags = (uint) ShaderStageFlagBits.FragmentBit,
                descriptorCount = 500
            };

            opaqueSetObjectInfo.bindingCount = 3;
            opaqueSetObjectInfo.bindingInfos = new[] {opaqueBindingObjectDataInfo, opaqueBindingMaterialDataInfo, opaqueBindingTextureDataInfo};

            opaqueSetSceneInfo.bindingCount = 1;
            opaqueSetSceneInfo.bindingInfos = new[] {opaqueBindingSceneDataInfo};

            opaqueLayoutInfo.setCount = 2;
            opaqueLayoutInfo.setInfos = new[] {opaqueSetSceneInfo, opaqueSetObjectInfo};

            opaqueShaderInfo.layoutInfo = opaqueLayoutInfo;

            _opaqueShader = device.CreateShader(opaqueShaderInfo);

            BufferCreateInfo vertexBufferCreateInfo = new BufferCreateInfo
            {
                size = (ulong) Marshal.SizeOf(typeof(Vertex)) * 3,
                usage = (uint) BufferUsageFlagBits.VertexBufferBit,
                sharingMode = (uint) SharingMode.Exclusive
            };

            BufferMemoryCreateInfo vertexBufferMemoryCreateInfo =
                new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

            _vertexBuffer = device.CreateBuffer(vertexBufferCreateInfo, vertexBufferMemoryCreateInfo);

            NativeBuffer<Vertex> nativeVertexBuffer =
                new NativeBuffer<Vertex>(VK.MapBuffer(device.handle, _vertexBuffer.handle));
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(0.0f, -0.5f, 0.0f) }, 0);
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(0.5f, 0.5f, 0.0f) }, 1);
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(-0.5f, 0.5f, 0.0f) }, 2);
            VK.UnmapBuffer(device.handle, _vertexBuffer.handle);

            BufferCreateInfo indexBufferCreateInfo = new BufferCreateInfo
            {
                size = sizeof(uint) * 3,
                usage = (uint) BufferUsageFlagBits.IndexBufferBit,
                sharingMode = (uint) SharingMode.Exclusive
            };

            BufferMemoryCreateInfo indexBufferMemoryCreateInfo =
                new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

            _indexBuffer = device.CreateBuffer(indexBufferCreateInfo, indexBufferMemoryCreateInfo);

            NativeBuffer<uint> nativeIndexBuffer =
                new NativeBuffer<uint>(VK.MapBuffer(device.handle, _indexBuffer.handle));
            nativeIndexBuffer.Set(0, 0);
            nativeIndexBuffer.Set(1, 1);
            nativeIndexBuffer.Set(2, 2);
            VK.UnmapBuffer(device.handle, _indexBuffer.handle);

            _sceneDataBuffer = new Buffer[_framesInFlight];
            _objectDataBuffer = new Buffer[_framesInFlight];
            _materialDataBuffer = new Buffer[_framesInFlight];
            for (uint i = 0; i < _framesInFlight; i++)
            {
                BufferCreateInfo sceneBufferCreateInfo = new BufferCreateInfo
                {
                    size = (ulong) Marshal.SizeOf(typeof(SceneData)),
                    usage = (uint) BufferUsageFlagBits.UniformBufferBit
                };
                vertexBufferCreateInfo.sharingMode = (uint) SharingMode.Exclusive;

                BufferMemoryCreateInfo sceneBufferMemoryCreateInfo = new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

                _sceneDataBuffer[i] = device.CreateBuffer(sceneBufferCreateInfo, sceneBufferMemoryCreateInfo);

                NativeBuffer<SceneData> nativeSceneBuffer =
                    new NativeBuffer<SceneData>(VK.MapBuffer(device.handle, _sceneDataBuffer[i].handle));
                nativeSceneBuffer.Set(new SceneData(), 0);
                VK.UnmapBuffer(device.handle, _sceneDataBuffer[i].handle);

                BufferCreateInfo objectBufferCreateInfo = new BufferCreateInfo
                {
                    size = (ulong) Marshal.SizeOf(typeof(ObjectData)),
                    usage = (uint) BufferUsageFlagBits.StorageBufferBit,
                    sharingMode = (uint) SharingMode.Exclusive
                };

                BufferMemoryCreateInfo objectBufferMemoryCreateInfo = new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

                _objectDataBuffer[i] = device.CreateBuffer(objectBufferCreateInfo, objectBufferMemoryCreateInfo);

                NativeBuffer<ObjectData> nativeObjectBuffer =
                    new NativeBuffer<ObjectData>(VK.MapBuffer(device.handle, _objectDataBuffer[i].handle));
                nativeObjectBuffer.Set(new ObjectData(), 0);
                VK.UnmapBuffer(device.handle, _objectDataBuffer[i].handle);

                BufferCreateInfo materialBufferCreateInfo = new BufferCreateInfo
                {
                    size = (ulong) Marshal.SizeOf(typeof(MaterialData)),
                    usage = (uint) BufferUsageFlagBits.StorageBufferBit,
                    sharingMode = (uint) SharingMode.Exclusive
                };

                BufferMemoryCreateInfo materialBufferMemoryCreateInfo = new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

                _materialDataBuffer[i] = device.CreateBuffer(materialBufferCreateInfo, materialBufferMemoryCreateInfo);

                NativeBuffer<MaterialData> nativeMaterialBuffer =
                    new NativeBuffer<MaterialData>(VK.MapBuffer(device.handle, _materialDataBuffer[i].handle));
                nativeMaterialBuffer.Set(new MaterialData(), 0);
                VK.UnmapBuffer(device.handle, _materialDataBuffer[i].handle);
            }

            BufferCreateInfo indirectInfo = new BufferCreateInfo
            {
                usage = (uint) (BufferUsageFlagBits.StorageBufferBit | BufferUsageFlagBits.IndirectBufferBit),
                sharingMode = (uint) SharingMode.Exclusive,
                size = 20
            };

            BufferMemoryCreateInfo indirectMemoryInfo = new BufferMemoryCreateInfo
            {
                requiredFlags =
                    (uint)(MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                preferredFlags = 0,
                usage = (uint)MemoryUsage.CpuToGpu
            };

            _indirectBuffer = device.CreateBuffer(indirectInfo, indirectMemoryInfo);

            DrawIndirectCommand drawCommand = new DrawIndirectCommand()
            {
                indexCount = 3,
                instanceCount = 1,
                firstIndex = 0,
                vertexOffset = 0,
                firstInstance = 0
            };

            NativeBuffer<DrawIndirectCommand> nativeBuffer = new NativeBuffer<DrawIndirectCommand>(VK.MapBuffer(device.handle, _indirectBuffer.handle));
            nativeBuffer.Set(drawCommand);
            VK.UnmapBuffer(device.handle, _indirectBuffer.handle);

            _sceneDescriptor = new Descriptor[_framesInFlight];
            _objectDescriptor = new Descriptor[_framesInFlight];
            _materialDescriptor = new Descriptor[_framesInFlight];
            _textureDescriptor = new Descriptor[_framesInFlight];
            for (uint i = 0; i < _framesInFlight; i++)
            {
                VK.DescriptorSet set = VK.AllocateDescriptors(_opaqueShader.handle);

                _sceneDescriptor[i] = new Descriptor(set, 0, 0);
                _objectDescriptor[i] = new Descriptor(set, 1, 0);
                _materialDescriptor[i] = new Descriptor(set, 1, 1);
                _textureDescriptor[i] = new Descriptor(set, 1, 2);

                DescriptorWriteInfo sceneWriteInfo = new DescriptorWriteInfo
                {
                    sets = set,
                    set = 0,
                    binding = 0,
                    count = 1,
                    element = 0,
                    type = (uint) DescriptorType.UniformBuffer
                };

                BufferWriteInfo sceneUniformBufferInfo = new BufferWriteInfo
                {
                    buffer = _sceneDataBuffer[i].handle,
                    offset = 0,
                    range = (ulong) Marshal.SizeOf(typeof(SceneData))
                };

                TypedWriteInfo sceneTypedInfo = new TypedWriteInfo
                {
                    buffers = sceneUniformBufferInfo
                };

                sceneWriteInfo.infos = new[] { sceneTypedInfo };

                DescriptorWriteInfo objectWriteInfo = new DescriptorWriteInfo
                {
                    sets = set,
                    set = 1,
                    binding = 0,
                    count = 1,
                    element = 0,
                    type = (uint) DescriptorType.StorageBuffer
                };

                BufferWriteInfo objectStorageBufferInfo = new BufferWriteInfo
                {
                    buffer = _objectDataBuffer[i].handle,
                    offset = 0,
                    range = (ulong) Marshal.SizeOf(typeof(ObjectData))
                };

                TypedWriteInfo objectTypedInfo = new TypedWriteInfo
                {
                    buffers = objectStorageBufferInfo
                };

                objectWriteInfo.infos = new[] { objectTypedInfo };

                DescriptorWriteInfo materialWriteInfo = new DescriptorWriteInfo
                {
                    sets = set,
                    set = 1,
                    binding = 1,
                    count = 1,
                    element = 0,
                    type = (uint) DescriptorType.StorageBuffer
                };

                BufferWriteInfo materialStorageBufferInfo =
                    new BufferWriteInfo
                    {
                        buffer = _materialDataBuffer[i].handle,
                        offset = 0,
                        range = (ulong) Marshal.SizeOf(typeof(MaterialData))
                    };

                TypedWriteInfo materialTypedInfo = new TypedWriteInfo
                {
                    buffers = materialStorageBufferInfo
                };

                materialWriteInfo.infos = new[] { materialTypedInfo };

                /*DescriptorWriteInfo textureWriteInfo = new DescriptorWriteInfo();
                textureWriteInfo.sets = set;
                textureWriteInfo.set = 1;
                textureWriteInfo.binding = 2;
                textureWriteInfo.count = 1;
                textureWriteInfo.element = 0;
                textureWriteInfo.type = (uint)DescriptorType.CombinedImageSampler;

                ImageWriteInfo textureSamplerInfo = new ImageWriteInfo();
                textureSamplerInfo.view = _textures[0].handle;
                textureSamplerInfo.sampler = _textureSamplers[0];
                textureSamplerInfo.layout = (uint) ImageLayout.ShaderReadOnlyOptimal;

                TypedWriteInfo textureTypedInfo = new TypedWriteInfo();
                textureTypedInfo.images = textureSamplerInfo;

                textureWriteInfo.infos = new[] { textureTypedInfo };*/

                VK.WriteDescriptors(device.handle, 1, new[] { sceneWriteInfo, objectWriteInfo, materialWriteInfo });
            }

            // subpasses
            //      zprepass (eventually)
            //      opaque
            //      translucent (eventually)

            // attachments
            //      depthBuffer
            //      colorBuffer
        }

        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }

        public ImageView[] GetOutputImages()
        {
            return _frameBufferColorImageViews;
        }

        public override void Start(CommandList commandList)
        {

        }

        public override void Execute(CommandList commandList, uint frameInFlight)
        {
            commandList.BeginRenderPass(_renderPass, _framebuffers[frameInFlight]);

            commandList.Bind(0, new [] {_vertexBuffer}, new []{0UL});
            commandList.Bind(_indexBuffer, 0, IndexType.Uint32);
            commandList.Bind(_opaqueShader, 0, new [] {_sceneDescriptor[frameInFlight], _objectDescriptor[frameInFlight], _materialDescriptor[frameInFlight]}, new []{0U, 0U, 0U});
            commandList.Bind(_opaqueShader);

            commandList.DrawIndexedIndirect(_indirectBuffer, 0, 1, (uint) Marshal.SizeOf(typeof(DrawIndirectCommand)));

            commandList.EndRenderPass();
        }

        public override void Dispose()
        {
            
        }
    }
}
