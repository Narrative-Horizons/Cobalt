using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class PhysicalDevice : IPhysicalDevice
    {
        private readonly List<IQueue.CreateInfo> _queueInfos = new List<IQueue.CreateInfo>();

        public PhysicalDevice(IPhysicalDevice.CreateInfo createInfo)
        {
            Info = createInfo;
            IQueue.CreateInfo queueInfo = new IQueue.CreateInfo.Builder()
                .FamilyIndex(0)
                .QueueIndex(0)
                .Properties(new QueueProperties()
                {
                    Compute = true,
                    Graphics = true,
                    Present = true,
                    Transfer = true
                })
                .Build();
            _queueInfos.Add(queueInfo);
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

        public IDevice Create(IDevice.CreateInfo info)
        {
            IDevice device = new Device(info);
            _devices.Add(device);
            return device;
        }

        public List<IQueue.CreateInfo> QueueInfos()
        {
            return _queueInfos;
        }
    }
}
