namespace Cobalt.Core
{
    public abstract class BaseApplication
    {
        public abstract void Setup();
        public abstract void Initialize();
        public abstract void Update();
        public abstract void Render();
        public abstract void Cleanup() ;
    }
}
