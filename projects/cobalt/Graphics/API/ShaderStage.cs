namespace Cobalt.Graphics.API
{
    public class ShaderStageCreateInfo
    {
        public sealed class Builder : ShaderStageCreateInfo
        {
            public new Builder Module(IShaderModule module)
            {
                base.Module = module;
                return this;
            }

            public new Builder EntryPoint(string entryPoint)
            {
                base.EntryPoint = entryPoint;
                return this;
            }

            public ShaderStageCreateInfo Build()
            {
                return new ShaderStageCreateInfo()
                {
                    Module = base.Module,
                    EntryPoint = base.EntryPoint
                };
            }
        }

        public IShaderModule Module { get; private set; }
        public string EntryPoint { get; private set; }
    }
}
