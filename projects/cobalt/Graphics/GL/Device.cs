using Cobalt.Bindings.Utils;
using Cobalt.Core;
using Cobalt.Graphics.API;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenGL = Cobalt.Bindings.GL.GL;
using Phonon = Cobalt.Bindings.Phonon.Phonon;

namespace Cobalt.Graphics.GL
{
    internal class Device : IDevice
    {
        public bool Debug { get; private set; }

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly Dictionary<Window, RenderSurface> _surfaces = new Dictionary<Window, RenderSurface>();

        public List<GLRenderPass> Passes { get; private set; } = new List<GLRenderPass>();
        public List<FrameBuffer> FrameBuffers { get; private set; } = new List<FrameBuffer>();
        public List<IBuffer> Buffers { get; private set; } = new List<IBuffer>();
        public List<Image> Images { get; private set; } = new List<Image>();
        public List<ShaderModule> Modules { get; private set; } = new List<ShaderModule>();
        public List<DescriptorPool> DescriptorPools { get; private set; } = new List<DescriptorPool>();
        public List<DescriptorSetLayout> DescSetLayouts { get; private set; } = new List<DescriptorSetLayout>();
        public List<PipelineLayout> Layouts { get; private set; } = new List<PipelineLayout>();
        public List<GraphicsPipeline> GraphicsPipelines { get; private set; } = new List<GraphicsPipeline>();
        public List<CommandPool> CommandPools { get; private set; } = new List<CommandPool>();
        public List<Sampler> Samplers { get; private set; } = new List<Sampler>();
        public List<Fence> Fences { get; private set; } = new List<Fence>();

        private static OpenGL.DebugCallback debugCallback;
        private GCHandle debugHandle;

        public Device(IDevice.CreateInfo info)
        {
            info.QueueInformation.ForEach(info =>
            {
                _queues.Add(new Queue(info));
            });

            Debug = info.Debug;
            debugCallback = DebugCallback;

            debugHandle = GCHandle.Alloc(debugCallback);

            OpenGL.DebugMessageCallback(debugCallback, IntPtr.Zero);

            // TESTING CODE BELOW
            /*Phonon.CreateContext(null, null, null, out IntPtr phononContext);

            Phonon.RenderingSettings rSettings = new Phonon.RenderingSettings
            {
                samplingRate = 44100,
                frameSize = 1024
            };

            Phonon.HrtfParams hrtfParams = new Phonon.HrtfParams
            {
                type = Phonon.HrtfDatabaseType.Default,
                hrtfData = IntPtr.Zero,
                sofaFileName = IntPtr.Zero
            };

            Phonon.CreateBinauralRenderer(phononContext, rSettings, hrtfParams, out IntPtr audioRenderer);

            Phonon.AudioFormat stereo = new Phonon.AudioFormat
            {
                channelLayoutType = Phonon.ChannelLayoutType.Speakers,
                channelLayout = Phonon.ChannelLayout.Stereo,
                channelOrder = Phonon.ChannelOrder.Interleaved
            };

            Phonon.AudioFormat mono = new Phonon.AudioFormat
            {
                channelLayoutType = Phonon.ChannelLayoutType.Speakers,
                channelLayout = Phonon.ChannelLayout.Mono,
                channelOrder = Phonon.ChannelOrder.Interleaved
            };

            Phonon.CreateBinauralEffect(audioRenderer, mono, stereo, out IntPtr audioEffect);

            var audioFile = new AudioFileReader("data/sound.wav");
            MemoryStream audioStream = new MemoryStream();
            audioFile.CopyTo(audioStream);

            byte[] audioData = audioStream.ToArray();

            GCHandle audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);

            Phonon.AudioBuffer inBuffer = new Phonon.AudioBuffer
            {
                format = mono,
                numSamples = 1024,
                interleavedBuffer = audioHandle.AddrOfPinnedObject()
            };

            //GCHandle h = GCHandle.FromIntPtr(inBuffer.interleavedBuffer);
            //byte[] inBufferData = h.Target as byte[];

            WaveFileReader wr = new WaveFileReader("data/sound.wav");
            var x = wr.WaveFormat;

            var soundStream = audioStream;
            var rs = new RawSourceWaveStream(audioStream, x);

            
            using (var wo = new WaveOutEvent())
            {
                wo.Init(rs);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            }

            int jonathan = 0;

            Phonon.DestroyBinauralEffect(ref audioEffect);
            Phonon.DestroyBinauralRenderer(ref audioRenderer);
            Phonon.DestroyContext(ref phononContext);*/
        }

        private static void DebugCallback(Bindings.GL.EDebugSource source, Bindings.GL.EDebugType type, uint id, Bindings.GL.EDebugSeverity severity, int length, IntPtr messagePtr, IntPtr userParam)
        {
#if !RELEASE
            string debugMessage = source.ToString() + " (" + type.ToString() + "): " + Util.PtrToStringUTF8(messagePtr);

            switch (severity)
            {
                case Bindings.GL.EDebugSeverity.High:
                    Logger.Log.Fatal(debugMessage);
                    break;
                case Bindings.GL.EDebugSeverity.Medium:
                    Logger.Log.Error(debugMessage);
                    break;
                case Bindings.GL.EDebugSeverity.Low:
                    Logger.Log.Debug(debugMessage);
                    break;
                case Bindings.GL.EDebugSeverity.Notification:
                    Logger.Log.Info(debugMessage);
                    break;
            }
#endif
        }

        public void Dispose()
        {
            debugHandle.Free();

            foreach (var pair in _surfaces)
            {
                pair.Value.Dispose();
            }
            
            _queues.ForEach(queue => queue.Dispose());
            Passes.ForEach(pass => pass.Dispose());
            FrameBuffers.ForEach(fb => fb.Dispose());
            Buffers.ForEach(buf => buf.Dispose());
            Images.ForEach(img => img.Dispose());
            Modules.ForEach(mod => mod.Dispose());
            DescSetLayouts.ForEach(set => set.Dispose());
            Layouts.ForEach(layout => layout.Dispose());
            GraphicsPipelines.ForEach(pipe => pipe.Dispose());
            CommandPools.ForEach(cp => cp.Dispose());
            DescriptorPools.ForEach(dp => dp.Dispose());
            Samplers.ForEach(samp => samp.Dispose());
            Fences.ForEach(fence => fence.Dispose());

            _surfaces.Clear();
            Passes.Clear();
            FrameBuffers.Clear();
            Buffers.Clear();
            Images.Clear();
            Modules.Clear(); ;
            DescSetLayouts.Clear();
            Layouts.Clear();
            GraphicsPipelines.Clear();
            CommandPools.Clear();
            DescriptorPools.Clear();
            Samplers.Clear();
            Fences.Clear();
        }

        public List<IQueue> Queues()
        {
            return _queues;
        }

        public IRenderSurface GetSurface(Window window)
        {
            if (!_surfaces.ContainsKey(window))
            {
                _surfaces[window] = new RenderSurface(window);
            }
            return _surfaces[window];
        }

        public IRenderPass CreateRenderPass(IRenderPass.CreateInfo info)
        {
            GLRenderPass pass = new GLRenderPass(info);
            Passes.Add(pass);

            return pass;
        }

        public IFrameBuffer CreateFrameBuffer(IFrameBuffer.CreateInfo info)
        {
            FrameBuffer fb = new FrameBuffer(info);
            FrameBuffers.Add(fb);

            return fb;
        }

        public IBuffer CreateBuffer<T>(IBuffer.CreateInfo<T> info, IBuffer.MemoryInfo memory) where T : unmanaged
        {
            Buffer<T> buffer = new Buffer<T>(memory, info);
            Buffers.Add(buffer);

            return buffer;
        }

        public IImage CreateImage(IImage.CreateInfo info, IImage.MemoryInfo memory)
        {
            Image image = new Image(memory, info);
            Images.Add(image);

            return image;
        }

        public IShaderModule CreateShaderModule(IShaderModule.CreateInfo info)
        {
            ShaderModule module = new ShaderModule(info);
            Modules.Add(module);

            return module;
        }

        public IDescriptorPool CreateDescriptorPool(IDescriptorPool.CreateInfo info)
        {
            DescriptorPool pool = new DescriptorPool(info);
            DescriptorPools.Add(pool);

            return pool;
        }

        public IDescriptorSetLayout CreateDescriptorSetLayout(IDescriptorSetLayout.CreateInfo info)
        {
            DescriptorSetLayout layout = new DescriptorSetLayout(info);
            DescSetLayouts.Add(layout);

            return layout;
        }

        public IPipelineLayout CreatePipelineLayout(IPipelineLayout.CreateInfo info)
        {
            PipelineLayout layout = new PipelineLayout(info);
            Layouts.Add(layout);

            return layout;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(IGraphicsPipeline.CreateInfo info)
        {
            GraphicsPipeline pipeline = new GraphicsPipeline(info);
            GraphicsPipelines.Add(pipeline);

            return pipeline;
        }

        public ICommandPool CreateCommandPool(ICommandPool.CreateInfo info)
        {
            CommandPool pool = new CommandPool(info);
            CommandPools.Add(pool);

            return pool;
        }

        public ISampler CreateSampler(ISampler.CreateInfo info)
        {
            Sampler sampler = new Sampler(info);
            Samplers.Add(sampler);

            return sampler;
        }

        public Shader CreateShader(Shader.CreateInfo info, IPipelineLayout layout)
        {
            Shader shader = new Shader(info, this, layout);

            return shader;
        }

        public ComputeShader CreateComputeShader(string computeSource)
        {
            ComputeShader shader = new ComputeShader(this, computeSource);

            return shader;
        }

        public void UpdateDescriptorSets(List<DescriptorWriteInfo> writeInformation)
        {
            writeInformation.ForEach(writeInfo =>
            {
                DescriptorSet s = writeInfo.DescriptorSet as DescriptorSet;

                s.Write(writeInformation);
            });
        }

        public void UpdateDescriptorSets(List<DescriptorCopyInfo> copyInformation)
        {
            copyInformation.ForEach(copyInfo =>
            {
                DescriptorSet src = copyInfo.src as DescriptorSet;
                src.Copy(copyInfo);
            });
        }

        public IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer indexBuffer, List<VertexAttribute> layout)
        {
            VertexAttributeArray vao = new VertexAttributeArray(new IGraphicsPipeline.VertexAttributeCreateInfo.Builder()
                .Attributes(layout).Build(), vertexBuffers, indexBuffer);

            return vao;
        }

        public IFence CreateFence(IFence.CreateInfo info)
        {
            Fence fence = new Fence(info);
            Fences.Add(fence);
            return fence;
        }

        public IComputePipeline CreateComputePipeline(IComputePipeline.CreateInfo info)
        {
            ComputePipeline pipeline = new ComputePipeline(info);
            return pipeline;
        }
    }
}
