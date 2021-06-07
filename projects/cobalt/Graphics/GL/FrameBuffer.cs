using OpenGL = Cobalt.Bindings.GL.GL;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class FrameBuffer : IFrameBuffer
    {
        public uint Handle { get; private set; }

        public FrameBuffer()
        {
            Handle = 0;
        }

        public FrameBuffer(IFrameBuffer.CreateInfo info) : base()
        {
            Handle = OpenGL.CreateFramebuffers();
            Dictionary<Bindings.GL.EFramebufferAttachment, int> attachmentCount = new Dictionary<Bindings.GL.EFramebufferAttachment, int>();

            foreach (IFrameBuffer.CreateInfo.Attachment attachment in info.Attachments)
            {
                ImageView view = (ImageView)attachment.ImageView;
                Image image = view.Image;

                Bindings.GL.EFramebufferAttachment type = ToInternalType(attachment.Usage);

                if (attachmentCount.ContainsKey(type))
                {
                    attachmentCount[type] += 1;
                }
                else
                {
                    attachmentCount[type] = 0;
                }

                var attachmentPoint = (Bindings.GL.EFramebufferAttachment)((int)type + attachmentCount[type]);

                OpenGL.NamedFramebufferTexture(Handle, attachmentPoint, image.Handle, 0);
            }

            uint status = OpenGL.CheckNamedFramebufferStatus(Handle, Bindings.GL.EFramebufferTarget.Framebuffer);
        }

        private static Bindings.GL.EFramebufferAttachment ToInternalType(EImageUsage usage)
        {
            switch (usage)
            {
                case EImageUsage.TransferSource:
                    break;
                case EImageUsage.TransferDestination:
                    break;
                case EImageUsage.Sampled:
                    break;
                case EImageUsage.Storage:
                    break;
                case EImageUsage.ColorAttachment:
                    return Bindings.GL.EFramebufferAttachment.ColorAttachment0;
                case EImageUsage.DepthAttachment:
                    return Bindings.GL.EFramebufferAttachment.DepthAttachment;
                case EImageUsage.DepthStencilAttachment:
                    return Bindings.GL.EFramebufferAttachment.DepthStencilAttachment;
                case EImageUsage.InputAttachment:
                    break;
            }

            throw new InvalidOperationException("Unsupported data type.");
        }

        public void Dispose()
        {
            OpenGL.DeleteFramebuffers(Handle);
        }
    }
}
