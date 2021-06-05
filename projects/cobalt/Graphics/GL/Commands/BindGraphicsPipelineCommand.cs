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
            StateMachine.Enable(Bindings.GL.EEnableCap.DepthTest, Pipeline.Info.DepthStencilCreationInformation.DepthTestEnabled);
            StateMachine.SetDepthMask(Pipeline.Info.DepthStencilCreationInformation.DepthWriteEnabled);


            if (Pipeline.Info.RasterizerCreationInformation != null)
            {
                StateMachine.Enable(Bindings.GL.EEnableCap.DepthClamp, true);

                switch (Pipeline.Info.RasterizerCreationInformation.CullFaces)
                {
                    case API.EPolgyonFace.None:
                        StateMachine.Enable(Bindings.GL.EEnableCap.CullFace, false);
                        break;
                    case API.EPolgyonFace.Back:
                        StateMachine.Enable(Bindings.GL.EEnableCap.CullFace, true);
                        StateMachine.CullFace(Bindings.GL.ECullFaceMode.Back);
                        break;
                    case API.EPolgyonFace.Front:
                        StateMachine.Enable(Bindings.GL.EEnableCap.CullFace, true);
                        StateMachine.CullFace(Bindings.GL.ECullFaceMode.Front);
                        break;
                    case API.EPolgyonFace.FrontAndBack:
                        StateMachine.Enable(Bindings.GL.EEnableCap.CullFace, true);
                        StateMachine.CullFace(Bindings.GL.ECullFaceMode.FrontAndBack);
                        break;
                }

                switch (Pipeline.Info.RasterizerCreationInformation.WindingOrder)
                {
                    case API.EVertexWindingOrder.Clockwise:
                        StateMachine.FrontFace(Bindings.GL.EFrontFaceDirection.Cw);
                        break;
                    case API.EVertexWindingOrder.CounterClockwise:
                        StateMachine.FrontFace(Bindings.GL.EFrontFaceDirection.Ccw);
                        break;
                }

                switch (Pipeline.Info.RasterizerCreationInformation.PolygonMode)
                {
                    case API.EPolygonMode.Fill:
                        StateMachine.PolygonMode(Bindings.GL.EPolygonMode.Fill);
                        break;
                    case API.EPolygonMode.Point:
                        StateMachine.PolygonMode(Bindings.GL.EPolygonMode.Point);
                        break;
                    case API.EPolygonMode.Wireframe:
                        StateMachine.PolygonMode(Bindings.GL.EPolygonMode.Line);
                        break;
                }
            }
        }
    }
}
