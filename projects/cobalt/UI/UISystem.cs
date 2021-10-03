using System;
using System.Collections.Generic;
using System.Text;
using Cobalt.Graphics.API;
using Cobalt.Math;
using ImGuiNET;

namespace Cobalt.UI
{
    public class UISystem
    {
        #region Private Data
        private bool _frameBegun;

        private IBuffer _vertexBuffer;
        private IBuffer _indexBuffer;
        private IVertexAttributeArray _vao;
        #endregion

        public struct UIDataBuffer
        {
            public Vector2 position;
            public Vector2 uv;
            public Vector4 color;
        }

        public UISystem(IDevice device)
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            var fonts = ImGui.GetIO().Fonts;
            ImGui.GetIO().Fonts.AddFontDefault();

            CreateDeviceResources(device);
        }

        private void CreateDeviceResources(IDevice device)
        {
            _vertexBuffer = device.CreateBuffer(new IBuffer.CreateInfo<UIDataBuffer>.Builder()
                    .AddUsage(EBufferUsage.ArrayBuffer).Size(10000),
                    new IBuffer.MemoryInfo.Builder()
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                        .AddRequiredProperty(EMemoryProperty.HostVisible)
                        .Usage(EMemoryUsage.CPUToGPU));

            _indexBuffer = device.CreateBuffer(new IBuffer.CreateInfo<uint>.Builder()
                .AddUsage(EBufferUsage.IndexBuffer).Size(2000),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .AddRequiredProperty(EMemoryProperty.HostVisible)
                    .Usage(EMemoryUsage.CPUToGPU));

            const int stride = 32;

            List<VertexAttribute> layout = new List<VertexAttribute>
            {
                new VertexAttribute.Builder().Location(0).Offset(0).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32_SFLOAT).Stride(stride).Binding(0),  // 2 floats
                new VertexAttribute.Builder().Location(1).Offset(8).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32_SFLOAT).Stride(stride).Binding(0),    // 2 floats
                new VertexAttribute.Builder().Location(2).Offset(16).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32B32A32_SFLOAT).Stride(stride).Binding(0), // 4 floats
            };

            _vao = device.CreateVertexAttributeArray(new List<IBuffer>() { _vertexBuffer }, _indexBuffer, layout);
        }
    }
}