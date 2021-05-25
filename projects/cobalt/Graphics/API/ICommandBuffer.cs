using Cobalt.Math;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
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

                public new Builder Count(uint count)
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
            public uint Count { get; private set; }
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
            public List<ClearValue> ClearValues { get; set; } = new List<ClearValue>();
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

        public class BufferImageCopyRegion
        {
            public sealed class Builder : BufferImageCopyRegion
            {
                public new Builder BufferOffset(int bufferOffset)
                {
                    base.BufferOffset = bufferOffset;
                    return this;
                }

                public new Builder ColorAspect(bool colorAspect)
                {
                    base.ColorAspect = colorAspect;
                    return this;
                }

                public new Builder DepthAspect(bool depthAspect)
                {
                    base.DepthAspect = depthAspect;
                    return this;
                }

                public new Builder ArrayLayer(int arrayLayer)
                {
                    base.ArrayLayer = arrayLayer;
                    return this;
                }

                public new Builder MipLevel(int mipLevel)
                {
                    base.MipLevel = mipLevel;
                    return this;
                }

                public new Builder X(int x)
                {
                    base.X = x;
                    return this;
                }

                public new Builder Y(int y)
                {
                    base.Y = y;
                    return this;
                }

                public new Builder Z(int z)
                {
                    base.Z = z;
                    return this;
                }

                public new Builder Width(int width)
                {
                    base.Width = width;
                    return this;
                }

                public new Builder Height(int height)
                {
                    base.Height = height;
                    return this;
                }

                public new Builder Depth(int depth)
                {
                    base.Depth = depth;
                    return this;
                }

                public BufferImageCopyRegion Build()
                {
                    return new BufferImageCopyRegion()
                    {
                        BufferOffset = base.BufferOffset,
                        ColorAspect = base.ColorAspect,
                        DepthAspect = base.DepthAspect,
                        ArrayLayer = base.ArrayLayer,
                        MipLevel = base.MipLevel,
                        X = base.X,
                        Y = base.Y,
                        Z = base.Z,
                        Width = base.Width,
                        Height = base.Height,
                        Depth = base.Depth
                    };
                }
            }

            public int BufferOffset { get; private set; }
            public bool ColorAspect { get; private set; }
            public bool DepthAspect { get; private set; }
            public int ArrayLayer { get; private set; }
            public int MipLevel { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Z { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Depth { get; private set; }
        }

        void Reset();

        void Record(RecordInfo info);

        void End();

        void BeginRenderPass(RenderPassBeginInfo info);

        void Bind(IGraphicsPipeline pipeline);

        void Bind(IVertexAttributeArray vao);
        void Bind(EBufferUsage usage, IBuffer buffer);

        void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets);

        void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets, List<uint> offsets);

        void Draw(int baseVertex, int vertexCount, int baseInstance, int instanceCount);

        void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount);

        void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount, long indexOffset);

        void DrawElementsMultiIndirect(DrawElementsIndirectCommand indirect, int offset, IBuffer indirectBuffer);

        void Copy(IBuffer source, IBuffer destination, List<BufferCopyRegion> regions);
        void Copy(byte[] source, IImage destination, List<BufferImageCopyRegion> regions);
    }
}
