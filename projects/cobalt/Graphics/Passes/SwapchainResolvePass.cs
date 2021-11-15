namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }
    }
}