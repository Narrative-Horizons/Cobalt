namespace Cobalt.Graphics.Passes
{
    public class TransparancyPass : Pass
    {
        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }
    }
}
