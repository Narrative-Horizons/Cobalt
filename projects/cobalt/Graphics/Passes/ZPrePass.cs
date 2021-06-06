using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Graphics.API;

namespace Cobalt.Graphics.Passes
{
    internal class ZPrePass : RenderPass
    {
        private int _framesInFlight;
        private Registry _registry;
        private Shader _shader;

        public ZPrePass(IDevice device, int framesInFlight, Registry registry) : base(device)
        {
            _framesInFlight = framesInFlight;
            _registry = registry;

            IPipelineLayout screenLayout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                new IDescriptorSetLayout.CreateInfo.Builder()
                .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                .AddAccessibleStage(EShaderType.Fragment).BindingIndex(0).DescriptorType(EDescriptorType.CombinedImageSampler).Count(1).Name("source_image").Build()).Build())).Build());

            string vsScreenSource = FileSystem.LoadFileToString("data/shaders/zpass/zpass_vertex.glsl");

            _shader = new Shader(new Shader.CreateInfo.Builder().VertexSource(vsScreenSource).Build(), device, screenLayout, false);
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
        }
    }
}
