namespace Cobalt.Graphics
{
    public interface IPass
    {
        public enum PassType : uint
        {
            Compute,
            Graphics
        }

        public PassType GetType();
    }
}
