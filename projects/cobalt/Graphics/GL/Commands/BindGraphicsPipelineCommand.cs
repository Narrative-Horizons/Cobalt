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
                if(Pipeline.Info.RasterizerCreationInformation.DepthClampEnabled)
                {
                    OpenGL.Enable(Bindings.GL.EEnableCap.DepthClamp);
                }
                else
                {
                    OpenGL.Disable(Bindings.GL.EEnableCap.DepthClamp);
                }

                switch (Pipeline.Info.RasterizerCreationInformation.CullFaces)
                {
                    case API.EPolgyonFace.None:
                        OpenGL.Disable(Bindings.GL.EEnableCap.CullFace);
                        break;
                    case API.EPolgyonFace.Back:
                        OpenGL.Enable(Bindings.GL.EEnableCap.CullFace);
                        OpenGL.CullFace(Bindings.GL.ECullFaceMode.Back);
                        break;
                    case API.EPolgyonFace.Front:
                        OpenGL.Enable(Bindings.GL.EEnableCap.CullFace);
                        OpenGL.CullFace(Bindings.GL.ECullFaceMode.Front);
                        break;
                    case API.EPolgyonFace.FrontAndBack:
                        OpenGL.Enable(Bindings.GL.EEnableCap.CullFace);
                        OpenGL.CullFace(Bindings.GL.ECullFaceMode.FrontAndBack);
                        break;
                }

                switch (Pipeline.Info.RasterizerCreationInformation.WindingOrder)
                {
                    case API.EVertexWindingOrder.Clockwise:
                        OpenGL.FrontFace(Bindings.GL.EFrontFaceDirection.Cw);
                        break;
                    case API.EVertexWindingOrder.CounterClockwise:
                        OpenGL.FrontFace(Bindings.GL.EFrontFaceDirection.Ccw);
                        break;
                }

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
