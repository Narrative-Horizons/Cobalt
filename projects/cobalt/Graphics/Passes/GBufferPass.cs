namespace Cobalt.Graphics.Passes
{
    public class GBufferPass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }
    }
}
