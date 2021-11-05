namespace Cobalt.Graphics.Passes
{
    public class ColorPass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}
