namespace Cobalt.Graphics.Passes
{
    public class GBufferResolvePass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}
