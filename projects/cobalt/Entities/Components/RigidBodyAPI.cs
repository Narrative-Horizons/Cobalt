using Cobalt.Math;

namespace Cobalt.Entities.Components
{
    public partial class RigidBodyComponent
    {
        public enum ForceMode
        {
            Start,
            Impulse,
            Acceleration,
            Force,
            VelocityChange
        }

        public void AddForce(Vector3 force, ForceMode mode)
        {

        }
    }
}
