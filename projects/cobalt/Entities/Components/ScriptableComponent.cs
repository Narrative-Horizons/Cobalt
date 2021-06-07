namespace Cobalt.Entities.Components
{
    public abstract class ScriptableComponent : BaseComponent
    {
        public T GetComponent<T>() where T : BaseComponent
        {
            return Registry.TryGet<T>(Owner);
        }

        public virtual void OnInit()
        {

        }

        public virtual void OnUpdate()
        {

        }
    }
}
