using System.Runtime.InteropServices;
using Cobalt.Bindings.Vulkan;
using Cobalt.Core;
using Cobalt.Graphics.Enums;
using Cobalt.Math;
using CobaltConverter.Core;

namespace Cobalt.Graphics.Passes
{
    public class UberPBRPass : Pass
    {
        private Shader _opaqueShader;
        private VK.RenderPass _renderPass;
        private Device _device;

        private uint _framesInFlight;

        private Framebuffer[] _framebuffers;
        private ImageView[] _frameBufferColorImageViews;
        private ImageView[] _frameBufferDepthImageViews;

        private Buffer _vertexBuffer;
        private Buffer _indexBuffer;
        private Buffer _indirectBuffer;

        private Buffer[] _sceneDataBuffer;
        private Buffer[] _objectDataBuffer;
        private Buffer[] _materialDataBuffer;

        private ImageView[] _textures;
        private VK.Sampler[] _textureSamplers;

        private Descriptor[] _sceneDescriptor;
        private Descriptor[] _objectDescriptor;
        private Descriptor[] _materialDescriptor;
        private Descriptor[] _textureDescriptor;

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
            public uint _padding;
        }

        public struct MaterialData
        {
            public uint albedo;
            public uint normal;
            public uint emission;
            public uint ORM;
        }

        public UberPBRPass(Device device, uint framesInFlight)
        {
            _framesInFlight = framesInFlight;
            _device = device;

            AttachmentDescription colorDesc = new AttachmentDescription();
            colorDesc.initialLayout = (uint)ImageLayout.Undefined;
            colorDesc.finalLayout = (uint)ImageLayout.ColorAttachmentOptimal;
            colorDesc.format = (uint) Format.B8G8R8A8Srgb;
            colorDesc.samples = (uint) SampleCountFlagBits.Count1Bit;
            colorDesc.loadOp = (uint) AttachmentLoadOp.Clear;
            colorDesc.storeOp = (uint) AttachmentStoreOp.Store;
            colorDesc.flags = 0;

            AttachmentDescription depthDesc = new AttachmentDescription();
            depthDesc.initialLayout = (uint)ImageLayout.Undefined;
            depthDesc.finalLayout = (uint)ImageLayout.DepthStencilAttachmentOptimal;
            depthDesc.format = (uint)Format.D24UnormS8Uint;
            depthDesc.samples = (uint)SampleCountFlagBits.Count1Bit;
            depthDesc.loadOp = (uint)AttachmentLoadOp.Clear;
            depthDesc.storeOp = (uint)AttachmentStoreOp.Store;
            depthDesc.flags = 0;

            AttachmentReference colorBuffer = new AttachmentReference();
            colorBuffer.layout = (uint) ImageLayout.ColorAttachmentOptimal;
            colorBuffer.attachment = 0;

            AttachmentReference depthBuffer = new AttachmentReference();
            depthBuffer.layout = (uint)ImageLayout.DepthStencilAttachmentOptimal;
            depthBuffer.attachment = 1;

            SubpassDescription opaquePass = new SubpassDescription();
            opaquePass.colorAttachments = new[] {colorBuffer};
            opaquePass.colorAttachmentCount = 1;
            opaquePass.depthStencilAttachments = new[] {depthBuffer};
            opaquePass.pipelineBindPoint = (uint) PipelineBindPoint.Graphics;
            opaquePass.flags = 0;

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo();
            renderPassInfo.attachments = new[] {colorDesc, depthDesc};
            renderPassInfo.attachmentCount = 2;
            renderPassInfo.subpasses = new[] {opaquePass};
            renderPassInfo.subpassCount = 1;

            _renderPass = device.CreateRenderPass(renderPassInfo);

            _framebuffers = new Framebuffer[_framesInFlight];
            _frameBufferColorImageViews = new ImageView[_framesInFlight];
            _frameBufferDepthImageViews = new ImageView[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                ImageCreateInfo colorInfo = new ImageCreateInfo();
                colorInfo.format = (uint) Format.B8G8R8A8Srgb;
                colorInfo.width = 1280;
                colorInfo.height = 720;
                colorInfo.depth = 1;
                colorInfo.initialLayout = (uint) ImageLayout.Undefined;
                colorInfo.arrayLayers = 1;
                colorInfo.samples = (uint) SampleCountFlagBits.Count1Bit;
                colorInfo.imageType = (uint) ImageType.Type2D;
                colorInfo.mipLevels = 1;
                colorInfo.sharingMode = (uint) SharingMode.Exclusive;
                colorInfo.tiling = (uint) ImageTiling.Optimal;
                colorInfo.usage = (uint) ImageUsageFlagBits.ColorAttachmentBit;

                ImageMemoryCreateInfo colorMemory = new ImageMemoryCreateInfo();
                colorMemory.usage = (uint) MemoryUsage.GpuOnly;
                colorMemory.requiredFlags = (uint) MemoryPropertyFlagBits.DeviceLocalBit;

                Image colorImage = device.CreateImage(colorInfo, colorMemory, "color", i);
                _frameBufferColorImageViews[i] = colorImage.CreateImageView((Format) colorInfo.format, ImageAspectFlagBits.ColorBit);

                ImageCreateInfo depthInfo = new ImageCreateInfo();
                depthInfo.format = (uint)Format.D24UnormS8Uint;
                depthInfo.width = 1280;
                depthInfo.height = 720;
                depthInfo.depth = 1;
                depthInfo.initialLayout = (uint)ImageLayout.Undefined;
                depthInfo.arrayLayers = 1;
                depthInfo.samples = (uint)SampleCountFlagBits.Count1Bit;
                depthInfo.imageType = (uint)ImageType.Type2D;
                depthInfo.mipLevels = 1;
                depthInfo.sharingMode = (uint)SharingMode.Exclusive;
                depthInfo.tiling = (uint)ImageTiling.Optimal;
                depthInfo.usage = (uint)ImageUsageFlagBits.DepthStencilAttachmentBit;

                ImageMemoryCreateInfo depthMemory = new ImageMemoryCreateInfo();
                depthMemory.usage = (uint)MemoryUsage.GpuOnly;
                depthMemory.requiredFlags = (uint)MemoryPropertyFlagBits.DeviceLocalBit;

                Image depthImage = device.CreateImage(depthInfo, depthMemory, "depth", i);
                _frameBufferDepthImageViews[i] = depthImage.CreateImageView((Format)depthInfo.format, ImageAspectFlagBits.DepthBit);

                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo();
                framebufferInfo.attachmentCount = 2;
                framebufferInfo.attachments = new[] {_frameBufferColorImageViews[i].handle, _frameBufferDepthImageViews[i].handle};
                framebufferInfo.height = 720;
                framebufferInfo.width = 1280;
                framebufferInfo.layers = 1;
                framebufferInfo.pass = _renderPass;

                _framebuffers[i] = device.CreateFramebuffer(framebufferInfo);
            }

            ShaderCreateInfo opaqueShaderInfo = new ShaderCreateInfo();
            opaqueShaderInfo.pass = _renderPass;
            opaqueShaderInfo.subPassIndex = 0;
            opaqueShaderInfo.vertexModulePath = "data/shaders/pbr/opaque_vert.spv";
            opaqueShaderInfo.fragmentModulePath = "data/shaders/pbr/opaque_frag.spv";

            ShaderLayoutCreateInfo opaqueLayoutInfo = new ShaderLayoutCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetSceneInfo = new ShaderLayoutSetCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetObjectInfo = new ShaderLayoutSetCreateInfo();

            ShaderLayoutBindingCreateInfo opaqueBindingObjectDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingObjectDataInfo.bindingIndex = 0;
            opaqueBindingObjectDataInfo.type = (uint) DescriptorType.StorageBuffer;
            opaqueBindingObjectDataInfo.stageFlags = (uint) ShaderStageFlagBits.VertexBit | (uint) ShaderStageFlagBits.FragmentBit;
            opaqueBindingObjectDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingMaterialDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingMaterialDataInfo.bindingIndex = 1;
            opaqueBindingMaterialDataInfo.type = (uint)DescriptorType.StorageBuffer;
            opaqueBindingMaterialDataInfo.stageFlags = (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingMaterialDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingSceneDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingSceneDataInfo.bindingIndex = 0;
            opaqueBindingSceneDataInfo.type = (uint)DescriptorType.UniformBuffer;
            opaqueBindingSceneDataInfo.stageFlags = (uint)ShaderStageFlagBits.VertexBit | (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingSceneDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingTextureDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingTextureDataInfo.bindingIndex = 2;
            opaqueBindingTextureDataInfo.type = (uint)DescriptorType.CombinedImageSampler;
            opaqueBindingTextureDataInfo.stageFlags = (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingTextureDataInfo.descriptorCount = 500;

            opaqueSetObjectInfo.bindingCount = 3;
            opaqueSetObjectInfo.bindingInfos = new[] {opaqueBindingObjectDataInfo, opaqueBindingMaterialDataInfo, opaqueBindingTextureDataInfo};

            opaqueSetSceneInfo.bindingCount = 1;
            opaqueSetSceneInfo.bindingInfos = new[] {opaqueBindingSceneDataInfo};

            opaqueLayoutInfo.setCount = 2;
            opaqueLayoutInfo.setInfos = new[] {opaqueSetSceneInfo, opaqueSetObjectInfo};

            opaqueShaderInfo.layoutInfo = opaqueLayoutInfo;

            _opaqueShader = device.CreateShader(opaqueShaderInfo);

            BufferCreateInfo vertexBufferCreateInfo = new BufferCreateInfo();
            vertexBufferCreateInfo.size = (ulong) Marshal.SizeOf(typeof(Vertex)) * 3;
            vertexBufferCreateInfo.usage = (uint)BufferUsageFlagBits.VertexBufferBit;
            vertexBufferCreateInfo.sharingMode = (uint)SharingMode.Exclusive;

            BufferMemoryCreateInfo vertexBufferMemoryCreateInfo = new BufferMemoryCreateInfo();
            vertexBufferMemoryCreateInfo.usage = (uint)MemoryUsage.CpuToGpu;
            vertexBufferMemoryCreateInfo.requiredFlags =
                (uint)(MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
            vertexBufferMemoryCreateInfo.preferredFlags = 0;

            _vertexBuffer = device.CreateBuffer(vertexBufferCreateInfo, vertexBufferMemoryCreateInfo);

            NativeBuffer<Vertex> nativeVertexBuffer =
                new NativeBuffer<Vertex>(Bindings.Vulkan.VK.MapBuffer(device.handle, _vertexBuffer.handle));
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(0.0f, -0.5f, 0.0f) }, 0);
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(0.5f, 0.5f, 0.0f) }, 1);
            nativeVertexBuffer.Set(new Vertex() { position = new Vector3(-0.5f, 0.5f, 0.0f) }, 2);
            Bindings.Vulkan.VK.UnmapBuffer(device.handle, _vertexBuffer.handle);

            BufferCreateInfo indexBufferCreateInfo = new BufferCreateInfo();
            indexBufferCreateInfo.size = sizeof(uint) * 3;
            indexBufferCreateInfo.usage = (uint)BufferUsageFlagBits.IndexBufferBit;
            indexBufferCreateInfo.sharingMode = (uint)SharingMode.Exclusive;

            BufferMemoryCreateInfo indexBufferMemoryCreateInfo = new BufferMemoryCreateInfo();
            indexBufferMemoryCreateInfo.usage = (uint)MemoryUsage.CpuToGpu;
            indexBufferMemoryCreateInfo.requiredFlags =
                (uint)(MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
            indexBufferMemoryCreateInfo.preferredFlags = 0;

            _indexBuffer = device.CreateBuffer(indexBufferCreateInfo, indexBufferMemoryCreateInfo);

            NativeBuffer<uint> nativeIndexBuffer =
                new NativeBuffer<uint>(Bindings.Vulkan.VK.MapBuffer(device.handle, _indexBuffer.handle));
            nativeIndexBuffer.Set(0, 0);
            nativeIndexBuffer.Set(1, 1);
            nativeIndexBuffer.Set(2, 2);
            Bindings.Vulkan.VK.UnmapBuffer(device.handle, _indexBuffer.handle);

            _sceneDataBuffer = new Buffer[_framesInFlight];
            _objectDataBuffer = new Buffer[_framesInFlight];
            _materialDataBuffer = new Buffer[_framesInFlight];
            for (uint i = 0; i < _framesInFlight; i++)
            {
                BufferCreateInfo sceneBufferCreateInfo = new BufferCreateInfo();
                sceneBufferCreateInfo.size = (ulong) Marshal.SizeOf(typeof(SceneData));
                sceneBufferCreateInfo.usage = (uint) BufferUsageFlagBits.UniformBufferBit;
                vertexBufferCreateInfo.sharingMode = (uint) SharingMode.Exclusive;

                BufferMemoryCreateInfo sceneBufferMemoryCreateInfo = new BufferMemoryCreateInfo();
                sceneBufferMemoryCreateInfo.usage = (uint) MemoryUsage.CpuToGpu;
                sceneBufferMemoryCreateInfo.requiredFlags =
                    (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
                sceneBufferMemoryCreateInfo.preferredFlags = 0;

                _sceneDataBuffer[i] = device.CreateBuffer(sceneBufferCreateInfo, sceneBufferMemoryCreateInfo);

                NativeBuffer<SceneData> nativeSceneBuffer =
                    new NativeBuffer<SceneData>(Bindings.Vulkan.VK.MapBuffer(device.handle, _sceneDataBuffer[i].handle));
                nativeSceneBuffer.Set(new SceneData(), 0);
                Bindings.Vulkan.VK.UnmapBuffer(device.handle, _sceneDataBuffer[i].handle);

                BufferCreateInfo objectBufferCreateInfo = new BufferCreateInfo();
                objectBufferCreateInfo.size = (ulong) Marshal.SizeOf(typeof(ObjectData));
                objectBufferCreateInfo.usage = (uint) BufferUsageFlagBits.StorageBufferBit;
                objectBufferCreateInfo.sharingMode = (uint) SharingMode.Exclusive;

                BufferMemoryCreateInfo objectBufferMemoryCreateInfo = new BufferMemoryCreateInfo();
                objectBufferMemoryCreateInfo.usage = (uint) MemoryUsage.CpuToGpu;
                objectBufferMemoryCreateInfo.requiredFlags =
                    (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
                objectBufferMemoryCreateInfo.preferredFlags = 0;

                _objectDataBuffer[i] = device.CreateBuffer(objectBufferCreateInfo, objectBufferMemoryCreateInfo);

                NativeBuffer<ObjectData> nativeObjectBuffer =
                    new NativeBuffer<ObjectData>(Bindings.Vulkan.VK.MapBuffer(device.handle, _objectDataBuffer[i].handle));
                nativeObjectBuffer.Set(new ObjectData(), 0);
                Bindings.Vulkan.VK.UnmapBuffer(device.handle, _objectDataBuffer[i].handle);

                BufferCreateInfo materialBufferCreateInfo = new BufferCreateInfo();
                materialBufferCreateInfo.size = (ulong)Marshal.SizeOf(typeof(MaterialData));
                materialBufferCreateInfo.usage = (uint)BufferUsageFlagBits.StorageBufferBit;
                materialBufferCreateInfo.sharingMode = (uint)SharingMode.Exclusive;

                BufferMemoryCreateInfo materialBufferMemoryCreateInfo = new BufferMemoryCreateInfo();
                materialBufferMemoryCreateInfo.usage = (uint)MemoryUsage.CpuToGpu;
                materialBufferMemoryCreateInfo.requiredFlags =
                    (uint)(MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
                materialBufferMemoryCreateInfo.preferredFlags = 0;

                _materialDataBuffer[i] = device.CreateBuffer(materialBufferCreateInfo, materialBufferMemoryCreateInfo);

                NativeBuffer<MaterialData> nativeMaterialBuffer =
                    new NativeBuffer<MaterialData>(Bindings.Vulkan.VK.MapBuffer(device.handle, _materialDataBuffer[i].handle));
                nativeMaterialBuffer.Set(new MaterialData(), 0);
                Bindings.Vulkan.VK.UnmapBuffer(device.handle, _materialDataBuffer[i].handle);
            }

            BufferCreateInfo indirectInfo = new BufferCreateInfo();
            indirectInfo.usage = (uint)(BufferUsageFlagBits.StorageBufferBit | BufferUsageFlagBits.IndirectBufferBit);
            indirectInfo.sharingMode = (uint)SharingMode.Exclusive;
            indirectInfo.size = 20;

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

            NativeBuffer<DrawIndirectCommand> nativeBuffer = new NativeBuffer<DrawIndirectCommand>(Bindings.Vulkan.VK.MapBuffer(device.handle, _indirectBuffer.handle));
            nativeBuffer.Set(drawCommand);
            Bindings.Vulkan.VK.UnmapBuffer(device.handle, _indirectBuffer.handle);

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

                DescriptorWriteInfo sceneWriteInfo = new DescriptorWriteInfo();
                sceneWriteInfo.sets = set;
                sceneWriteInfo.set = 0;
                sceneWriteInfo.binding = 0;
                sceneWriteInfo.count = 1;
                sceneWriteInfo.element = 0;
                sceneWriteInfo.type = (uint)DescriptorType.UniformBuffer;

                BufferWriteInfo sceneUniformBufferInfo = new BufferWriteInfo();
                sceneUniformBufferInfo.buffer = _sceneDataBuffer[i].handle;
                sceneUniformBufferInfo.offset = 0;
                sceneUniformBufferInfo.range = (ulong) Marshal.SizeOf(typeof(SceneData));

                TypedWriteInfo sceneTypedInfo = new TypedWriteInfo();
                sceneTypedInfo.buffers = sceneUniformBufferInfo;

                sceneWriteInfo.infos = new[] { sceneTypedInfo };

                DescriptorWriteInfo objectWriteInfo = new DescriptorWriteInfo();
                objectWriteInfo.sets = set;
                objectWriteInfo.set = 1;
                objectWriteInfo.binding = 0;
                objectWriteInfo.count = 1;
                objectWriteInfo.element = 0;
                objectWriteInfo.type = (uint)DescriptorType.StorageBuffer;

                BufferWriteInfo objectStorageBufferInfo = new BufferWriteInfo();
                objectStorageBufferInfo.buffer = _objectDataBuffer[i].handle;
                objectStorageBufferInfo.offset = 0;
                objectStorageBufferInfo.range = (ulong)Marshal.SizeOf(typeof(ObjectData));

                TypedWriteInfo objectTypedInfo = new TypedWriteInfo();
                objectTypedInfo.buffers = objectStorageBufferInfo;

                objectWriteInfo.infos = new[] { objectTypedInfo };

                DescriptorWriteInfo materialWriteInfo = new DescriptorWriteInfo();
                materialWriteInfo.sets = set;
                materialWriteInfo.set = 1;
                materialWriteInfo.binding = 1;
                materialWriteInfo.count = 1;
                materialWriteInfo.element = 0;
                materialWriteInfo.type = (uint)DescriptorType.StorageBuffer;

                BufferWriteInfo materialStorageBufferInfo = new BufferWriteInfo();
                materialStorageBufferInfo.buffer = _materialDataBuffer[i].handle;
                materialStorageBufferInfo.offset = 0;
                materialStorageBufferInfo.range = (ulong)Marshal.SizeOf(typeof(MaterialData));

                TypedWriteInfo materialTypedInfo = new TypedWriteInfo();
                materialTypedInfo.buffers = materialStorageBufferInfo;

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

                Bindings.Vulkan.VK.WriteDescriptors(device.handle, 1, new[] { sceneWriteInfo, objectWriteInfo, materialWriteInfo });
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
