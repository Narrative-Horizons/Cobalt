namespace Cobalt.Graphics.Passes
{
    public class TransparancyPass : IPass
    {
        public new IPass.PassType GetType()
        {
            return IPass.PassType.Graphics;
        }
    }
}
