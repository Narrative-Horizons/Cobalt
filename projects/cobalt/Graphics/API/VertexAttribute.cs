namespace Cobalt.Graphics.API
{
    public class VertexAttribute
    {
        public sealed class Builder : VertexAttribute
        {
            public new Builder Binding(int binding)
            {
                base.Binding = binding;
                return this;
            }

            public new Builder Location(int location)
            {
                base.Location = location;
                return this;
            }

            public new Builder Format(EDataFormat format)
            {
                base.Format = format;
                return this;
            }

            public new Builder Offset(int offset)
            {
                base.Offset = offset;
                return this;
            }

            public new Builder Rate(EVertexInputRate rate)
            {
                base.Rate = rate;
                return this;
            }

            public new Builder Stride(int stride)
            {
                base.Stride = stride;
                return this;
            }
        }

        public int Binding { get; private set; }
        public int Location { get; private set; }
        public EDataFormat Format { get; private set; }
        public int Offset { get; private set; }
        public EVertexInputRate Rate { get; private set; }
        public int Stride { get; private set; }
    }
}
