namespace Cobalt.Graphics.Passes
{
    public class GBufferPass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}
