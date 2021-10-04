using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class GLRenderPass : IRenderPass
    {
        public IRenderPass.CreateInfo Info { get; private set; }

        public GLRenderPass(IRenderPass.CreateInfo info)
        {
            Info = info;
        }

        public void Dispose()
        {
            // Do nothing
        }

        public List<IRenderPass.AttachmentDescription> GetAttachments()
        {
            return Info.Attachments;
        }
    }
}
