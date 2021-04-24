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
        }
    }
}
