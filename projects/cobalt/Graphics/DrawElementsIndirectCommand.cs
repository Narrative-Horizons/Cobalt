using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DrawElementsIndirectCommandPayload
    {
        public uint IndexCount { get; set; }
        public uint InstanceCount { get; set; }
        public uint FirstIndex { get; set; }
        public uint BaseVertex { get; set; }
        public uint FirstInstance { get; set; }
    }

    public class DrawElementsIndirectCommand
    {
        public DrawElementsIndirectCommand Add(DrawElementsIndirectCommandPayload payload)
        {

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
