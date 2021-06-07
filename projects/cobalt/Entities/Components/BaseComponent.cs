namespace Cobalt.Entities.Components
{
    public abstract class BaseComponent
    {
        public Registry Registry { get; internal set; }
        public Entity Owner { get; internal set; }
    }
}
