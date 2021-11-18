using System;
using System.Collections.Generic;
using System.Text;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Buffer
    {
        internal readonly VK.Buffer handle;

        internal Buffer(VK.Buffer buffer)
        {
            handle = buffer;
        }
    }
}
