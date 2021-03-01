using OpenGL = Cobalt.Bindings.GL.GL;
using GLPropertyName = Cobalt.Bindings.GL.EPropertyName;

using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class GraphicsApplication : IGraphicsApplication
    {
        private List<IPhysicalDevice> _devices = new List<IPhysicalDevice>();
        
        public GraphicsApplication(IGraphicsApplication.CreateInfo info)
        {
            Info = info;

            var renderer = OpenGL.GetString(GLPropertyName.Renderer);
            var vendor = OpenGL.GetString(GLPropertyName.Vendor);

            var device = new PhysicalDevice(new IPhysicalDevice.CreateInfo.Builder()
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
