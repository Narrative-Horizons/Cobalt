using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class PhysicalDevice : IPhysicalDevice
    {
        public PhysicalDevice(IPhysicalDevice.CreateInfo createInfo, GraphicsApplication graphicsApplication)
        {
            Info = createInfo;
            GraphicsApplication = graphicsApplication;
        }

        public IPhysicalDevice.CreateInfo Info { get; }
        public GraphicsApplication GraphicsApplication { get; }

        public bool Debug()
        {
            return Info.Debug;
        }

        public void Dispose()
        {
            // TODO: Write me
        }

        public string Name()
        {
            return Info.Name;
        }

        public IGraphicsApplication Owner()
        {
            return GraphicsApplication;
        }
    }
}
