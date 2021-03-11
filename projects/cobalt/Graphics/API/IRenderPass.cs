using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IRenderPass : IDisposable
    {
        public class AttachmentDescription
        {
            public sealed class Builder : AttachmentDescription
            {
                public new Builder Format(EDataFormat format)
                {
                    base.Format = format;
                    return this;
                }

                public new Builder Samples(ESampleCount samples)
                {
                    base.Samples = samples;
                    return this;
                }

                public new Builder LoadOp(EAttachmentLoad loadOp)
                {
                    base.LoadOp = loadOp;
                    return this;
                }

                public new Builder StoreOp(EAttachmentStore storeOp)
                {
                    base.StoreOp = storeOp;
                    return this;
                }

                public new Builder StencilLoadOp(EAttachmentLoad loadOp)
                {
                    base.StencilLoadOp = loadOp;
                    return this;
                }

                public new Builder StencilStoreOp(EAttachmentStore storeOp)
                {
                    base.StencilStoreOp = storeOp;
                    return this;
                }

                public new Builder InitialLayout(EImageLayout layout)
                {
                    base.InitialLayout = layout;
                    return this;
                }

                public new Builder FinalLayout(EImageLayout layout)
                {
                    base.FinalLayout = layout;
                    return this;
                }
            }

            public EDataFormat Format { get; private set; }
            public ESampleCount Samples { get; private set; }
            public EAttachmentLoad LoadOp { get; private set; }
            public EAttachmentStore StoreOp { get; private set; }
            public EAttachmentLoad StencilLoadOp { get; private set; }
            public EAttachmentStore StencilStoreOp { get; private set; }
            public EImageLayout InitialLayout { get; private set; }
            public EImageLayout FinalLayout { get; private set; }

        }

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public Builder AddAttachment(AttachmentDescription attachment)
                {
                    Attachments.Add(attachment);
                    return this;
                }

                public Builder AddAttachment(AttachmentDescription.Builder attachment)
                {
                    Attachments.Add(attachment);
                    return this;
                }
            }

            public List<AttachmentDescription> Attachments { get; private set; } = new List<AttachmentDescription>();
        }

        public List<AttachmentDescription> GetAttachments();
    }
}
