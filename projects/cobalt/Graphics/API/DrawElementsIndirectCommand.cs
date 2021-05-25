using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics.API
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DrawElementsIndirectCommandPayload
    {
        public uint Count { get; set; }
        public uint InstanceCount { get; set; }
        public uint FirstIndex { get; set; }
        public uint BaseVertex { get; set; }
        public uint BaseInstance { get; set; }
    }

    public class DrawElementsIndirectCommand
    {
        public List<DrawElementsIndirectCommandPayload> Data { get; internal set; } = new List<DrawElementsIndirectCommandPayload>();

        public DrawElementsIndirectCommand Add(DrawElementsIndirectCommandPayload payload)
        {
            Data.Add(payload);

            return this;
        }
    }

    public class DrawElementsIndirectCommand<T>
    {
        public DrawElementsIndirectCommand<T> Add(DrawElementsIndirectCommandPayload payload, T objectData)
        {

            return this;
        }
    }
}
