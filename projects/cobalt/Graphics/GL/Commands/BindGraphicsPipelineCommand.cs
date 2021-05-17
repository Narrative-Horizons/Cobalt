using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindGraphicsPipelineCommand : ICommand
    {
        public GraphicsPipeline Pipeline { get; private set; }

        public BindGraphicsPipelineCommand(GraphicsPipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            StateMachine.UseProgram(Pipeline);
            if(Pipeline.Info.DepthStencilCreationInformation.DepthTestEnabled)
            {
                OpenGL.Enable(Bindings.GL.EEnableCap.DepthTest);
            }
            else
            {
                OpenGL.Disable(Bindings.GL.EEnableCap.DepthTest);
            }

            if(Pipeline.Info.DepthStencilCreationInformation.DepthWriteEnabled)
            {
                StateMachine.SetDepthMask(true);
            }
            else
            {
                StateMachine.SetDepthMask(false);
            }

            if(Pipeline.Info.RasterizerCreationInformation != null)
            {
                switch (Pipeline.Info.RasterizerCreationInformation.PolygonMode)
                {
                    case API.EPolygonMode.Fill:
                        Bindings.GL.GL.PolygonMode(Bindings.GL.ECullFaceMode.FrontAndBack, Bindings.GL.EPolygonMode.Fill);
                        break;
                    case API.EPolygonMode.Point:
                        Bindings.GL.GL.PolygonMode(Bindings.GL.ECullFaceMode.FrontAndBack, Bindings.GL.EPolygonMode.Point);
                        break;
                    case API.EPolygonMode.Wireframe:
                        Bindings.GL.GL.PolygonMode(Bindings.GL.ECullFaceMode.FrontAndBack, Bindings.GL.EPolygonMode.Line);
                        break;
                }
            }
        }
    }
}
