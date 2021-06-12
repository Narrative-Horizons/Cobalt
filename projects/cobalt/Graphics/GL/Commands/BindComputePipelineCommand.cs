namespace Cobalt.Graphics.GL.Commands
{
    internal class BindComputePipelineCommand : ICommand
    {
        public ComputePipeline Pipeline { get; private set; }

        public BindComputePipelineCommand(ComputePipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.UseProgram(Pipeline);
        }
    }
}
