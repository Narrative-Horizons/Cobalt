using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.Vulkan
{
    public struct ApiVersion
    {
        public uint major;
        public uint minor;
        public uint patch;
    };
    public struct InstanceCreateInfo
    {
        public ApiVersion appVersion;
        public string appName;
        public ApiVersion engineVersion;
        public string engineName;
        public ApiVersion requiredVersion;
        public ApiVersion desiredVersion;
        public ulong enabledLayerCount;
        public string[] enabledLayers;
        public ulong enabledExtensionCount;
        public string[] enabledExtensions;
        public bool requireValidationLayers;
        public bool useDefaultDebugger;
        public GLFW.GLFWWindow window;
        // TODO: custom debugger and layers
    };

    public struct SwapchainCreateInfo
    {

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AttachmentDescription
    {
        [FieldOffset(0)]
        public uint flags;
        [FieldOffset(4)]
        public uint format;
        [FieldOffset(8)]
        public uint samples;
        [FieldOffset(12)]
        public uint loadOp;
        [FieldOffset(16)]
        public uint storeOp;
        [FieldOffset(20)]
        public uint stencilLoadOp;
        [FieldOffset(24)]
        public uint stencilStoreOp;
        [FieldOffset(28)]
        public uint initialLayout;
        [FieldOffset(32)]
        public uint finalLayout;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AttachmentReference
    {
        [FieldOffset(0)]
        public uint attachment;
        [FieldOffset(4)]
        public uint layout;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SubpassDescription
    {
        [FieldOffset(0)]
        public uint flags;
        [FieldOffset(4)]
        public uint pipelineBindPoint;
        [FieldOffset(8)]
        public uint inputAttachmentCount;
        [FieldOffset(12)]
        public AttachmentReference[] attachments;
        [FieldOffset(20)]
        public uint colorAttachmentCount;
        [FieldOffset(24)]
        public AttachmentReference[] colorAttachments;
        [FieldOffset(32)]
        public AttachmentReference[] resolveAttachments;
        [FieldOffset(40)]
        public AttachmentReference[] depthStencilAttachments;
        [FieldOffset(48)]
        public uint preserveAttachmentCount;
        [FieldOffset(52)]
        public uint[] preseveAttachments;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SubpassDependency
    {
        [FieldOffset(0)]
        public uint srcSubpass;
        [FieldOffset(4)]
        public uint dstSubpass;
        [FieldOffset(8)]
        public uint srcStageMask;
        [FieldOffset(12)]
        public uint dstStageMask;
        [FieldOffset(16)]
        public uint srcAccessMask;
        [FieldOffset(20)]
        public uint dstAccessMask;
        [FieldOffset(24)]
        public uint dependencyFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RenderPassCreateInfo
    {
        [FieldOffset(0)] 
        public uint attachmentCount;
        [FieldOffset(4)] 
        public AttachmentDescription[] attachments;
        [FieldOffset(12)] 
        public uint subpassCount;
        [FieldOffset(16)] 
        public SubpassDescription[] subpasses;
        [FieldOffset(24)] 
        public uint dependencyCount;
        [FieldOffset(28)] 
        public SubpassDependency[] dependencies;
    }
}
