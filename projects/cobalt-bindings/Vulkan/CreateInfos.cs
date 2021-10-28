﻿using System;
using System.Numerics;
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
        [FieldOffset(16)]
        public AttachmentReference[] attachments;
        [FieldOffset(24)]
        public uint colorAttachmentCount;
        [FieldOffset(32)]
        public AttachmentReference[] colorAttachments;
        [FieldOffset(40)]
        public AttachmentReference[] resolveAttachments;
        [FieldOffset(48)]
        public AttachmentReference[] depthStencilAttachments;
        [FieldOffset(56)]
        public uint preserveAttachmentCount;
        [FieldOffset(64)]
        public uint[] preserveAttachments;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct SubpassDescriptionImpl
    {
        [FieldOffset(0)]
        public uint flags;
        [FieldOffset(4)]
        public uint pipelineBindPoint;
        [FieldOffset(8)]
        public uint inputAttachmentCount;
        [FieldOffset(16)]
        public AttachmentReference* attachments;
        [FieldOffset(24)]
        public uint colorAttachmentCount;
        [FieldOffset(32)]
        public AttachmentReference* colorAttachments;
        [FieldOffset(40)]
        public AttachmentReference* resolveAttachments;
        [FieldOffset(48)]
        public AttachmentReference* depthStencilAttachments;
        [FieldOffset(56)]
        public uint preserveAttachmentCount;
        [FieldOffset(64)]
        public uint* preserveAttachments;
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
        [FieldOffset(8)] 
        public AttachmentDescription[] attachments;
        [FieldOffset(16)] 
        public uint subpassCount;
        [FieldOffset(24)] 
        public SubpassDescription[] subpasses;
        [FieldOffset(32)] 
        public uint dependencyCount;
        [FieldOffset(40)] 
        public SubpassDependency[] dependencies;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct RenderPassCreateInfoImpl
    {
        [FieldOffset(0)]
        public uint attachmentCount;
        [FieldOffset(8)]
        public AttachmentDescription* attachments;
        [FieldOffset(16)]
        public uint subpassCount;
        [FieldOffset(24)]
        public SubpassDescriptionImpl* subpasses;
        [FieldOffset(32)]
        public uint dependencyCount;
        [FieldOffset(40)]
        public SubpassDependency* dependencies;
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
        [FieldOffset(8)]
        public ShaderLayoutBindingCreateInfo[] bindingInfos;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderLayoutCreateInfo
    {
        [FieldOffset(0)]
        public uint setCount;
        [FieldOffset(8)]
        public ShaderLayoutSetCreateInfo[] setInfos;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ShaderCreateInfo
    {
        [FieldOffset(0)]
        public string vertexModulePath;
        [FieldOffset(8)]
        public string fragmentModulePath;
        [FieldOffset(16)]
        public string geometryModulePath;
        [FieldOffset(24)]
        public string tesselationEvalModulePath;
        [FieldOffset(32)]
        public string tesselationControlModulePath;
        [FieldOffset(40)]
        public string computeModulePath;

        [FieldOffset(48)]
        public VK.RenderPass pass;
        [FieldOffset(56)]
        public uint subPassIndex;

        [FieldOffset(64)]
        public ShaderLayoutCreateInfo layoutInfo;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ImageCreateInfo
    {
        [FieldOffset(0)] 
        public uint imageType;
        [FieldOffset(4)] 
        public uint format;
        [FieldOffset(8)] 
        public uint width;
        [FieldOffset(12)] 
        public uint height;
        [FieldOffset(16)] 
        public uint depth;
        [FieldOffset(20)] 
        public uint mipLevels;
        [FieldOffset(24)] 
        public uint arrayLayers;
        [FieldOffset(28)] 
        public uint samples;
        [FieldOffset(32)] 
        public uint tiling;
        [FieldOffset(36)] 
        public uint usage;
        [FieldOffset(40)] 
        public uint sharingMode;
        [FieldOffset(44)] 
        public uint queueFamilyIndexCount;
        [FieldOffset(48)] 
        public uint[] queueFamilyIndices;
        [FieldOffset(56)] 
        public uint initialLayout;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ImageViewCreateInfo
    {
        [FieldOffset(0)] 
        public VK.Image image;
        [FieldOffset(8)] 
        public uint viewType;
        [FieldOffset(12)]
        public uint format;
        [FieldOffset(16)]
        public uint aspectMask;
        [FieldOffset(20)]
        public uint baseMipLevel;
        [FieldOffset(24)]
        public uint levelCount;
        [FieldOffset(28)]
        public uint baseArrayLayer;
        [FieldOffset(32)]
        public uint layerCount;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FramebufferCreateInfo
    {
        [FieldOffset(0)] 
        public VK.RenderPass pass;
        [FieldOffset(8)] 
        public uint attachmentCount;
        [FieldOffset(16)] 
        public VK.ImageView[] attachments;
        [FieldOffset(24)] 
        public uint width;
        [FieldOffset(28)] 
        public uint height;
        [FieldOffset(32)] 
        public uint layers;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct FramebufferCreateInfoImpl
    {
        [FieldOffset(0)]
        public VK.RenderPass pass;
        [FieldOffset(8)]
        public uint attachmentCount;
        [FieldOffset(16)]
        public VK.ImageView* attachments;
        [FieldOffset(24)]
        public uint width;
        [FieldOffset(28)]
        public uint height;
        [FieldOffset(32)]
        public uint layers;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SemaphoreCreateInfo
    {
        [FieldOffset(0)]
        public uint flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IndexedCommandBuffers
    {
        [FieldOffset(0)]
        public VK.CommandBuffer commandbuffer;
        [FieldOffset(8)]
        public uint[] bufferIndices;

        [FieldOffset(16)] 
        public uint amount;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct IndexedCommandBuffersImpl
    {
        [FieldOffset(0)]
        public VK.CommandBuffer commandbuffer;
        [FieldOffset(8)]
        public uint* bufferIndices;

        [FieldOffset(16)] 
        public uint amount;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SubmitInfo
    {
        [FieldOffset(0)]
        public uint waitSemaphoreCount;
        [FieldOffset(8)]
        public VK.Semaphore[] waitSemaphores;

        [FieldOffset(16)]
        public uint[] waitDstStageMask;

        [FieldOffset(24)]
        public uint commandBufferCount;
        [FieldOffset(32)]
        public IndexedCommandBuffers[] commandBuffer;

        [FieldOffset(40)]
        public uint signalSemaphoreCount;
        [FieldOffset(48)]
        public VK.Semaphore[] signalSemaphores;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct SubmitInfoImpl
    {
        [FieldOffset(0)]
        public uint waitSemaphoreCount;
        [FieldOffset(8)]
        public VK.Semaphore* waitSemaphores;

        [FieldOffset(16)]
        public uint* waitDstStageMask;

        [FieldOffset(24)]
        public uint commandBufferCount;
        [FieldOffset(32)]
        public IndexedCommandBuffersImpl* commandBuffer;

        [FieldOffset(40)]
        public uint signalSemaphoreCount;
        [FieldOffset(48)]
        public VK.Semaphore* signalSemaphores;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PresentInfo
    {
        [FieldOffset(0)]
        public uint waitSemaphoreCount;
        [FieldOffset(8)]
        public VK.Semaphore[] waitSemaphores;

        [FieldOffset(16)]
        public uint swapchainCount;
        [FieldOffset(24)]
        public VK.SwapChain[] swapchains;

        [FieldOffset(32)]
        public uint[] imageIndices;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct PresentInfoImpl
    {
        [FieldOffset(0)]
        public uint waitSemaphoreCount;
        [FieldOffset(8)]
        public VK.Semaphore* waitSemaphores;

        [FieldOffset(16)]
        public uint swapchainCount;
        [FieldOffset(24)]
        public VK.SwapChain* swapchains;

        [FieldOffset(32)]
        public uint* imageIndices;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FenceCreateInfo
    {
        [FieldOffset(0)]
        public uint flags;
    }

    public struct Rect2D
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    public struct ClearValue
    {
        public Vector4 color;
        public float? depth;
    }

    public struct RenderPassBeginInfo
    {
        public VK.RenderPass renderpass;
        public VK.Framebuffer framebuffer;

        public Rect2D renderArea;
        public uint clearValueCount;

        public ClearValue[] clearValues;
    }
}
