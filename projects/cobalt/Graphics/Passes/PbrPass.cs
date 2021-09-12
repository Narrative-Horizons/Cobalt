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

            _pbrTranslucentShader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_fragment.glsl"))
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
                .Build(), device, layout);

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

            // Handle translucent objects            
            buffer.Bind(_pbrTranslucentShader.Pipeline);
            buffer.Bind(_pbrTranslucentShader.Layout, 0, draw.descriptorSets);
            Draw(buffer, draw, EMaterialType.Translucent);

            return true;
        }
    }
}
