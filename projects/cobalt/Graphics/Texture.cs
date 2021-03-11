using Cobalt.Graphics.API;

namespace Cobalt.Graphics
{
    public class Texture
    {
        public string name { get; set; }
        public IImageView Image { get; set; }
        public ISampler Sampler { get; set; }
    }
}
