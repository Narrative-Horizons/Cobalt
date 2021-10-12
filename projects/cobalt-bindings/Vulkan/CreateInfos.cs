using System.Runtime.InteropServices;

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

    [StructLayout(LayoutKind.Explicit)]
    public struct CommandBufferCreateInfo
    {
        [FieldOffset(0)] 
        public uint pool;
        [FieldOffset(4)] 
        public uint amount;
        [FieldOffset(8)] 
        public bool primary;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct BufferCreateInfo
    {
        [FieldOffset(0)] 
        public uint usage;
        [FieldOffset(4)] 
        public ulong size;
        [FieldOffset(12)]
        public uint sharingMode;
        [FieldOffset(16)]
        public uint indexCount;
        [FieldOffset(24)]
        public uint[] indices;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct BufferMemoryCreateInfo
    {
        [FieldOffset(0)] 
        public uint usage;
        [FieldOffset(4)]
        public uint preferredFlags;
        [FieldOffset(8)]
        public uint requiredFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderModuleCreateInfo
    {
        [FieldOffset(0)]
        public char[] code;
        [FieldOffset(8)]
        public ulong codeSize;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderLayoutBindingCreateInfo
    {
        [FieldOffset(0)]
        public uint bindingIndex;
        [FieldOffset(4)]
        public uint type;
        [FieldOffset(8)]
        public uint descriptorCount;
        [FieldOffset(12)]
        public uint stageFlags;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderLayoutSetCreateInfo
    {
        [FieldOffset(0)]
        public uint bindingCount;
        [FieldOffset(4)]
        public ShaderLayoutBindingCreateInfo[] bindingInfos;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderLayoutCreateInfo
    {
        [FieldOffset(0)]
        public uint setCount;
        [FieldOffset(4)]
        public ShaderLayoutSetCreateInfo[] setInfos;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderCreateInfo
    {
        [FieldOffset(0)]
        public char[] vertexModulePath;
        [FieldOffset(8)]
        public char[] fragmentModulePath;
        [FieldOffset(16)]
        public char[] geometryModulePath;
        [FieldOffset(24)]
        public char[] tesselationEvalModulePath;
        [FieldOffset(32)]
        public char[] tesselationControlModulePath;
        [FieldOffset(40)]
        public char[] computeModulePath;

        [FieldOffset(48)]
        public VK.RenderPass pass;
        [FieldOffset(56)]
        public uint subPassIndex;

        [FieldOffset(60)]
        public ShaderLayoutCreateInfo layoutInfo;
    };
}
