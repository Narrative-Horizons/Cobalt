using System;

namespace Cobalt.Graphics
{
    public interface IPhysicalDevice : IDisposable
    {
        internal class CreateInfo
        {
            internal sealed class Builder : CreateInfo
            {
                public new Builder Name(string name)
                {
                    base.Name = name;
                    return this;
                }

                public new Builder Debug(bool debug)
                {
                    base.Debug = debug;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Debug = base.Debug,
                        Name = base.Name
                    };
                    return info;
                }
            }

            public string Name { get; private set; }
            public bool Debug { get; private set; }
        }

        public string Name();

        public bool Debug();

        public IGraphicsApplication Owner();
    }
}
