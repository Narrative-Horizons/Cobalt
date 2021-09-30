﻿using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Text;
using static Cobalt.Graphics.RenderPass;

namespace Cobalt.Graphics
{
    public interface IRenderPipeline
    {
        public bool Register(string name, List<IImageView> views);
        public bool Register(string name, List<IFrameBuffer> buffers);
        public bool Register(string name, List<IBuffer> buffers);
        public IImageView GetImageView(string name, int frame);
        public IFrameBuffer GetFrameBuffer(string name, int frame);
        public IBuffer GetBuffer(string name, int frame);
        public void Render(FrameInfo frame, CameraComponent camera);
    }

    public abstract class RenderPipeline : IRenderPipeline
    {
        private readonly Dictionary<string, List<IImageView>> _imageViews = new Dictionary<string, List<IImageView>>();
        private readonly Dictionary<string, List<IFrameBuffer>> _frameBuffers = new Dictionary<string, List<IFrameBuffer>>();
        private readonly Dictionary<string, List<IBuffer>> _buffers = new Dictionary<string, List<IBuffer>>();
        protected IDevice Device { get; set; }

        public RenderPipeline(IDevice device)
        {
            Device = device;
        }

        public IBuffer GetBuffer(string name, int frame)
        {
            var buffers = _buffers.GetValueOrDefault(name, null);
            return buffers?[frame % buffers.Count];
        }

        public IFrameBuffer GetFrameBuffer(string name, int frame)
        {
            var buffers = _frameBuffers.GetValueOrDefault(name, null);
            return buffers?[frame % buffers.Count];
        }

        public IImageView GetImageView(string name, int frame)
        {
            var buffers = _imageViews.GetValueOrDefault(name, null);
            return buffers?[frame % buffers.Count];
        }

        public bool Register(string name, List<IImageView> views)
        {
            return _imageViews.TryAdd(name, views);
        }

        public bool Register(string name, List<IFrameBuffer> buffers)
        {
            return _frameBuffers.TryAdd(name, buffers);
        }

        public bool Register(string name, List<IBuffer> buffers)
        {
            return _buffers.TryAdd(name, buffers);
        }

        public abstract void Render(FrameInfo frame, CameraComponent camera);
        public abstract void OnFrameStart(FrameInfo frame);
    }
}
