using Cobalt.Bindings.GLSL_Parser;
using Cobalt.Core;
using Cobalt.Graphics.API;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Graphics
{

    // TODO: Dynamic sizing of target
    public class Shader : IDisposable
    {
        public IGraphicsPipeline Pipeline { get; private set; }
        public IPipelineLayout Layout { get; private set; }

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder VertexSource(string vertexSource)
                {
                    base.VertexSource = vertexSource;
                    return this;
                }

                public new Builder FragmentSource(string fragmentSource)
                {
                    base.FragmentSource = fragmentSource;
                    return this;
                }

                public new Builder GeometrySource(string geometrySource)
                {
                    base.GeometrySource = geometrySource;
                    return this;
                }

                public new Builder TessControlSource(string tessControlSource)
                {
                    base.TessControlSource = tessControlSource;
                    return this;
                }

                public new Builder TessEvalSource(string tessEvalSource)
                {
                    base.TessEvalSource = tessEvalSource;
                    return this;
                }

                public new Builder ComputeSource(string computeSource)
                {
                    base.ComputeSource = computeSource;
                    return this;
                }

                public CreateInfo Build()
                {
                    return new CreateInfo()
                    {
                        VertexSource = base.VertexSource,
                        FragmentSource = base.FragmentSource,
                        GeometrySource = base.GeometrySource,
                        TessControlSource = base.TessControlSource,
                        TessEvalSource = base.TessEvalSource,
                        ComputeSource = base.ComputeSource
                    };
                }
            }

            public string VertexSource { get; private set; } = null;
            public string FragmentSource { get; private set; } = null;
            public string GeometrySource { get; private set; } = null;
            public string TessControlSource { get; private set; } = null;
            public string TessEvalSource { get; private set; } = null;
            public string ComputeSource { get; private set; } = null;
        }

        static bool first = true;
        public Shader(CreateInfo createInfo, IDevice device, IPipelineLayout layout, bool test)
        {
            if(createInfo.ComputeSource != null)
            {
                // This is a compute shader
            }
            else
            {
                if(createInfo.VertexSource == null)
                {
                    // Not a valid shader creation
                    Logger.Log.Error("Shader needs at least a compute or a vertex shader source attached.");
                    return;
                }

                IShaderModule vsShaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Vertex).ResourceStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(createInfo.VertexSource))));

                string source = "";
                string[] lines = createInfo.VertexSource.Split('\n');

                foreach(string l in lines)
                {
                    string line = l.TrimStart();
                    if (line.StartsWith("#") || line.Length == 0)
                        continue;

                    source += l + "\n";
                }

                // GLSLParser.ParseSource(source, "filename.glsl", 1);

                IShaderModule fsShaderModule = null;
                IShaderModule gsShaderModule = null;
                IShaderModule tcShaderModule = null;
                IShaderModule teShaderModule = null;

                if(createInfo.FragmentSource != null)
                {
                    fsShaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Fragment).ResourceStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(createInfo.FragmentSource))));
                }

                if (createInfo.GeometrySource != null)
                {
                    gsShaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Geometry).ResourceStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(createInfo.GeometrySource))));
                }

                if (createInfo.TessControlSource != null)
                {
                    tcShaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.TessellationControl).ResourceStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(createInfo.TessControlSource))));
                }

                if (createInfo.TessEvalSource != null)
                {
                    teShaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.TessellationEvaluation).ResourceStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(createInfo.TessEvalSource))));
                }

                IGraphicsPipeline.CreateInfo.Builder pipelineCreateInfo = new IGraphicsPipeline.CreateInfo.Builder()
                .AddStageCreationInformation(
                    new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                    .Module(vsShaderModule)
                    .EntryPoint("main").Build());

                if(fsShaderModule != null)
                {
                    pipelineCreateInfo.AddStageCreationInformation(
                        new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                        .Module(fsShaderModule)
                        .EntryPoint("main").Build());
                }

                if (gsShaderModule != null)
                {
                    pipelineCreateInfo.AddStageCreationInformation(
                        new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                        .Module(gsShaderModule)
                        .EntryPoint("main").Build());
                }

                if (tcShaderModule != null)
                {
                    pipelineCreateInfo.AddStageCreationInformation(
                        new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                        .Module(tcShaderModule)
                        .EntryPoint("main").Build());
                }

                if (teShaderModule != null)
                {
                    pipelineCreateInfo.AddStageCreationInformation(
                        new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                        .Module(teShaderModule)
                        .EntryPoint("main").Build());
                }

                int sizeOfLayout = 5 * 4;

                pipelineCreateInfo.VertexAttributeCreationInformation(
                    new IGraphicsPipeline.VertexAttributeCreateInfo.Builder()
                    .AddAttribute(
                        new VertexAttribute.Builder()
                            .Binding(0)
                            .Format(EDataFormat.R32G32B32_SFLOAT)
                            .Location(0)
                            .Offset(0)
                            .Rate(EVertexInputRate.PerVertex)
                            .Stride(sizeOfLayout))
                    .AddAttribute(
                        new VertexAttribute.Builder()
                            .Binding(0)
                            .Format(EDataFormat.R32G32_SFLOAT)
                            .Location(1)
                            .Offset(sizeof(float) * 3)
                            .Rate(EVertexInputRate.PerVertex)
                            .Stride(sizeOfLayout)).Build())
                .InputAssemblyCreationInformation(
                    new IGraphicsPipeline.InputAssemblyCreateInfo.Builder()
                        .RestartEnabled(false)
                        .Topology(ETopology.TriangleList))
                .ViewportCreationInformation(
                    new IGraphicsPipeline.ViewportCreateInfo.Builder()
                        .Viewport(new Viewport()
                        {
                            LeftX = 0,
                            UpperY = 0,
                            Width = 1280,
                            Height = 720,
                            MinDepth = 0,
                            MaxDepth = 1
                        })
                        .ScissorRegion(new Scissor()
                        {
                            ExtentX = 1280,
                            ExtentY = 720,
                            OffsetX = 0,
                            OffsetY = 0
                        }))
                .RasterizerCreationInformation(
                    new IGraphicsPipeline.RasterizerCreateInfo.Builder()
                    .DepthClampEnabled(false)
                    .PolygonMode(first ? EPolygonMode.Fill : EPolygonMode.Fill)
                    .WindingOrder(EVertexWindingOrder.Clockwise)
                    .CullFaces(EPolgyonFace.Back)
                    .RasterizerDiscardEnabled(true).Build())
                .DepthStencilCreationInformation(
                    new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                    .DepthTestEnabled(test)
                    .DepthWriteEnabled(test)
                    .DepthCompareOp(ECompareOp.Less).Build())
                .MultisamplingCreationInformation(
                    new IGraphicsPipeline.MultisampleCreateInfo.Builder()
                    .AlphaToOneEnabled(false)
                    .AlphaToCoverageEnabled(false)
                    .Samples(ESampleCount.Samples1).Build())
                .PipelineLayout(layout).Build();

                Pipeline = device.CreateGraphicsPipeline(pipelineCreateInfo);
                Layout = layout;

                first = !first;
            }
        }

        public void Dispose()
        {
            Pipeline.Dispose();
        }
    }
}
