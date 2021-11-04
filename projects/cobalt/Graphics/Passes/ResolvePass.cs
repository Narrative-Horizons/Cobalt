namespace Cobalt.Graphics.Passes
{
    public class ResolvePass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}