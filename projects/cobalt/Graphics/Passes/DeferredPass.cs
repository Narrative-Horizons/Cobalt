namespace Cobalt.Graphics.Passes
{
    public class DeferredPass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}
