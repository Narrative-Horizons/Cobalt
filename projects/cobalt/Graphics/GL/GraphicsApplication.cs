using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class GraphicsApplication : IGraphicsApplication
    {
        private List<IPhysicalDevice> _devices = new List<IPhysicalDevice>();
        
        public GraphicsApplication(IGraphicsApplication.CreateInfo info)
        {
            Info = info;
            string renderer = OpenGL.GetString(EStringName.Renderer);
            string vendor = OpenGL.GetString(EStringName.Vendor);

            PhysicalDevice device = new PhysicalDevice(new IPhysicalDevice.CreateInfo.Builder()
                .Debug(Info.Debug)
                .Name(vendor + " - " + renderer)
                .Build());
            _devices.Add(device);
        }

        public IGraphicsApplication.CreateInfo Info { get; }

        public void Dispose()
        {
            _devices.ForEach(device => device.Dispose());
            _devices.Clear();
        }

        public List<IPhysicalDevice> GetPhysicalDevices()
        {
            return _devices;
        }
    }
}
