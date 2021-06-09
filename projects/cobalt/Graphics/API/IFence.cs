using System;

namespace Cobalt.Graphics.API
{
    public interface IFence : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Signaled(bool signaled)
                {
                    base.Signaled = signaled;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo()
                    {
                        Signaled = base.Signaled
                    };

                    return info;
                }
            }

            public bool Signaled { get; private set; }
        }

        void Wait();
        void Reset();
    }
}
