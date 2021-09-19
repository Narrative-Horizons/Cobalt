using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.Passes
{
    public class OpaquePbrUberPass : RenderPass
    {
        public OpaquePbrUberPass(IDevice device) : base(device)
        {
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            return true;
        }
    }
}
