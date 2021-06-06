namespace Cobalt.Entities.Components
{
    internal class DirtyComponent : BaseComponent
    {
        public bool IsDirty { get; internal set; } = true;
    }
}
