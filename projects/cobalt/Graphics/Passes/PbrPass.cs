using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    internal class PbrRenderPass : RenderPass
    {
        private Shader _pbrShader;
        private Shader _pbrTranslucentShader;
        private IRenderPass _pass;
        private IRenderPass _translucency;

        private static readonly uint MAX_TEX_COUNT = 500;

        public PbrRenderPass(IDevice device, IPipelineLayout layout) : base(device)
        {
            _pbrShader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_fragment.glsl"))
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder() 
                    .DepthCompareOp(ECompareOp.LessOrEqual)  
                    .DepthWriteEnabled(false)
                    .DepthTestEnabled(true)
                    .Build())
                .Build(), device, layout);

            IPipelineLayout transparencyLayout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                    new IDescriptorSetLayout.CreateInfo.Builder()
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(0)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("ObjectData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(1)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("Materials")
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(2)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("SceneData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(3)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("A-Buffer")
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(4)
                        .Count(1)
                        .DescriptorType(EDescriptorType.SampledImage)
                        .Name("A-Buffer-Head")
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(5)
                        .Count(1)
                        .DescriptorType(EDescriptorType.SampledImage)
                        .Name("A-Buffer-Counter")
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(6)
                        .Count((int)MAX_TEX_COUNT)
                        .DescriptorType(EDescriptorType.CombinedImageSampler)
                        .Name("Textures")
                        .AddAccessibleStage(EShaderType.Fragment)
                        .Build())
                    .Build()))
                .Build());

            _pbrTranslucentShader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr/oit_linkedlist_pbr_fragment_color.glsl"))
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                    .DepthCompareOp(ECompareOp.LessOrEqual)
                    .DepthWriteEnabled(false)
                    .DepthTestEnabled(true)
                    .Build())
                .ColorBlendInfo(new IGraphicsPipeline.ColorBlendCreateInfo.Builder()
                    .AddBlend(new IGraphicsPipeline.ColorAttachmentBlendCreateInfo.Builder()
                        .RedWritable(true)
                        .GreenWritable(true)
                        .BlueWritable(true)
                        .AlphaWritable(true)
                        .ColorBlendOp(EBlendOp.Add)
                        .SourceColorFactor(EBlendFactor.SrcAlpha)
                        .DestinationColorFactor(EBlendFactor.OneMinusSrcAlpha)
                        .AlphaBlendOp(EBlendOp.Add)
                        .SourceAlphaFactor(EBlendFactor.One)
                        .DestinationAlphaFactor(EBlendFactor.Zero)
                        .Build())
                    .Build())
                .Build(), device, transparencyLayout);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Clear)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB))
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.DepthAttachment)
                    .FinalLayout(EImageLayout.DepthAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.D32_SFLOAT)));

            _translucency = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB))
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.DepthAttachment)
                    .FinalLayout(EImageLayout.DepthAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.D32_SFLOAT)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            // Handle opaque objects
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1)), new ClearValue(1) },
                Width = 1280,
                Height = 720,
                FrameBuffer = info.frameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_pbrShader.Pipeline);
            buffer.Bind(_pbrShader.Layout, 0, draw.descriptorSets);
            Draw(buffer, draw, EMaterialType.Opaque);

            return true;
        }
    }
}
