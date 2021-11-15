namespace Cobalt.Graphics.Passes
{
    public class GBufferResolvePass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }
    }
}
