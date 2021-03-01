using OpenGL = Cobalt.Bindings.GL.GL;

using System.Collections.Generic;
using Cobalt.Bindings.GL;

namespace Cobalt.Graphics.GL
{
    internal class GraphicsApplication : IGraphicsApplication
    {
        private List<IPhysicalDevice> _devices = new List<IPhysicalDevice>();
        
        public GraphicsApplication(IGraphicsApplication.CreateInfo info)
        {
            Info = info;
            string renderer = OpenGL.GetString(EPropertyName.Renderer);
            string vendor = OpenGL.GetString(EPropertyName.Vendor);

            PhysicalDevice device = new PhysicalDevice(new IPhysicalDevice.CreateInfo.Builder()
                .Debug(Info.Debug)
                .Name(vendor + " - " + renderer)
                .Build(),
                this);
            _devices.Add(device);
        }

        public IGraphicsApplication.CreateInfo Info { get; }

        public void Dispose()
        {
            _devices.ForEach(device => device.Dispose());
        }

        public List<IPhysicalDevice> GetPhysicalDevices()
        {
            return _devices;
        }
    }
}
