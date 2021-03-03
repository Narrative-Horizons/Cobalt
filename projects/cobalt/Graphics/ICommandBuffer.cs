using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public interface ICommandBuffer : IDisposable
    {
        public class AllocateInfo
        {
            public sealed class Builder : AllocateInfo
            {
                public new Builder Level(ECommandBufferLevel level)
                {
                    base.Level = level;
                    return this;
                }

                public new Builder Count(int count)
                {
                    base.Count = count;
                    return this;
                }

                public AllocateInfo Build()
                {
                    return new AllocateInfo()
                    {
                        Count = base.Count,
                        Level = base.Level
                    };
                }
            }

            public ECommandBufferLevel Level { get; private set; }
            public int Count { get; private set; }
        }

        public class SecondaryInheritanceInfo
        {

        }

        public class RecordInfo
        {
            public SecondaryInheritanceInfo InheritanceInfo { get; private set; }
        }

        public class RenderPassBeginInfo
        {
            public IRenderPass RenderPass { get; set; }
            public IFrameBuffer FrameBuffer { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public List<Vector4> ClearValues { get; set; } = new List<Vector4>();
        }

        public class BufferCopyRegion
        {
            public sealed class Builder : BufferCopyRegion
            {
                public new Builder WriteOffset(int writeOffset)
                {
                    base.WriteOffset = writeOffset;
                    return this;
                }

                public new Builder ReadOffset(int readOffset)
                {
                    base.ReadOffset = readOffset;
                    return this;
                }

                public new Builder Length(int length)
                {
                    base.Length = length;
                    return this;
                }

                public BufferCopyRegion Build()
                {
                    return new BufferCopyRegion()
                    {
                        WriteOffset = base.WriteOffset,
                        ReadOffset = base.ReadOffset,
                        Length = base.Length
                    };
                }
            }

            public int WriteOffset { get; private set; }
            public int ReadOffset { get; private set; }
            public int Length { get; private set; }
        }
    }
}
