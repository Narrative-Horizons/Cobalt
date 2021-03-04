using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public interface ISwapchain : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Format(EDataFormat format)
                {
                    base.Format = format;
                    return this;
                }

                public new Builder ColorSpace(EColorSpace space)
                {
                    base.ColorSpace = space;
                    return this;
                }

                public new Builder PresentMode(EPresentMode mode)
                {
                    base.PresentMode = mode;
                    return this;
                }

                public new Builder ImageCount(uint count)
                {
                    base.ImageCount = count;
                    return this;
                }

                public new Builder Width(uint width)
                {
                    base.Width = width;
                    return this;
                }

                public new Builder Height(uint height)
                {
                    base.Height = height;
                    return this;
                }

                public new Builder Layers(uint layers)
                {
                    base.Layers = layers;
                    return this;
                }

                public Builder AddQueue(IQueue queue)
                {
                    base.Queues.Add(queue);
                    return this;
                }

                public new Builder Queues(List<IQueue> queues)
                {
                    base.Queues.Clear();
                    base.Queues.AddRange(queues);
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Format = base.Format,
                        ColorSpace = base.ColorSpace,
                        PresentMode = base.PresentMode,
                        ImageCount = base.ImageCount,
                        Width = base.Width,
                        Layers = base.Layers,
                        Queues = new List<IQueue>(base.Queues)
                    };

                    return info;
                }
            }

            public EDataFormat Format { get; private set; }
            public EColorSpace ColorSpace { get; private set; }
            public EPresentMode PresentMode { get; private set; }
            public uint ImageCount { get; private set; }
            public uint Width { get; private set; }
            public uint Height { get; private set; }
            public uint Layers { get; private set; }
            public List<IQueue> Queues { get; private set; } = new List<IQueue>();
        }

        public class PresentInfo
        {
            public sealed class Builder : PresentInfo
            {

            }
        }

        void Present(PresentInfo info);

        IFrameBuffer GetFrameBuffer(int frame);

        uint GetImageCount();
    }
}
