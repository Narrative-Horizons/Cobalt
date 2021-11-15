namespace Cobalt.Graphics.Passes
{
    public class DeferredPass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }
    }
}
