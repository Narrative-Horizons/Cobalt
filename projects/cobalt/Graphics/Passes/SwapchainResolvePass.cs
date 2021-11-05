namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}