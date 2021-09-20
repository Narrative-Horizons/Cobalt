namespace Cobalt.Entities.Components
{
    public partial class RigidBodyComponent : BaseComponent
    {
        // Mass in KG
        public float Mass { get; set; }
        public bool Kinematic { get; set; }
        public bool UseGravity { get; set; }
        public float SleepThreshold { get; set; }
    }
}
