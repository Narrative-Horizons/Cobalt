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

        private readonly List<IDevice> _devices = new List<IDevice>();

        public bool Debug()
        {
            return Info.Debug;
        }

        public void Dispose()
        {
            _devices.ForEach(device => device.Dispose());
        }

        public string Name()
        {
            return Info.Name;
        }

        public IGraphicsApplication Owner()
        {
            return GraphicsApplication;
        }

        public IDevice Create(IDevice.CreateInfo info)
        {
            IDevice device = new Device(info);
            _devices.Add(device);
            return device;
        }
    }
}
