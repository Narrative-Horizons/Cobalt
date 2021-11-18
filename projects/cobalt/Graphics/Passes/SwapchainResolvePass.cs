namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }

        public override void Start(CommandList commandList)
        {
            throw new System.NotImplementedException();
        }

        public override void Execute(CommandList commandList)
        {

        }

        public override void Dispose()
        {
            
        }
    }
}