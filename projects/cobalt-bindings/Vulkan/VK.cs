using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.Vulkan
{
    public static class VK
    {
        #region Named Pointers
        public struct Instance
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Instance window)
            {
                return window.handle;
            }

            public static explicit operator Instance(IntPtr handle) => new Instance(handle);

            public Instance(IntPtr handle)
            {
                this.handle = handle;
            }

            public void Destroy()
            {
                DestroyInstance(this);
            }
        }

        public struct Device
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Device window)
            {
                return window.handle;
            }

            public static explicit operator Device(IntPtr handle) => new Device(handle);

            public Device(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct SwapChain
        {
            public IntPtr handle;

            public static implicit operator IntPtr(SwapChain window)
            {
                return window.handle;
            }

            public static explicit operator SwapChain(IntPtr handle) => new SwapChain(handle);

            public SwapChain(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct RenderPass
        {
            public IntPtr handle;

            public static implicit operator IntPtr(RenderPass window)
            {
                return window.handle;
            }

            public static explicit operator RenderPass(IntPtr handle) => new RenderPass(handle);

            public RenderPass(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct CommandBuffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(CommandBuffer window)
            {
                return window.handle;
            }

            public static explicit operator CommandBuffer(IntPtr handle) => new CommandBuffer(handle);

            public CommandBuffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Buffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Buffer window)
            {
                return window.handle;
            }

            public static explicit operator Buffer(IntPtr handle) => new Buffer(handle);

            public Buffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct ShaderModule
        {
            public IntPtr handle;

            public static implicit operator IntPtr(ShaderModule window)
            {
                return window.handle;
            }

            public static explicit operator ShaderModule(IntPtr handle) => new ShaderModule(handle);

            public ShaderModule(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Shader
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Shader window)
            {
                return window.handle;
            }

            public static explicit operator Shader(IntPtr handle) => new Shader(handle);

            public Shader(IntPtr handle)
            {
                this.handle = handle;
            }
        }
        public struct Framebuffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Framebuffer window)
            {
                return window.handle;
            }

            public static explicit operator Framebuffer(IntPtr handle) => new Framebuffer(handle);

            public Framebuffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Image
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Image window)
            {
                return window.handle;
            }

            public static explicit operator Image(IntPtr handle) => new Image(handle);

            public Image(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct ImageView
        {
            public IntPtr handle;

            public static implicit operator IntPtr(ImageView window)
            {
                return window.handle;
            }

            public static explicit operator ImageView(IntPtr handle) => new ImageView(handle);

            public ImageView(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct DescriptorSet
        {
            public IntPtr handle;

            public static implicit operator IntPtr(DescriptorSet set)
            {
                return set.handle;
            }

            public static explicit operator DescriptorSet(IntPtr handle) => new DescriptorSet(handle);

            public DescriptorSet(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        #endregion

        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/gfx-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/gfx-native-bindings";
#else
        public const string LIBRARY = "bin/gfx-native-bindings";
#endif
        #endregion

        #region DLL Imports
        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_device", CallingConvention = CallingConvention.Cdecl)]
        public static extern Instance CreateInstance(InstanceCreateInfo info);

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_device", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DestroyInstance(Instance instance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern SwapChain CreateSwapchain(Device device, SwapchainCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySwapchain(SwapChain swapchain);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern RenderPass CreateRenderPass(Device device, RenderPassCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyRenderPass(Device device, RenderPass renderpass);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandBuffer CreateCommandBuffer(Device device, CommandBufferCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyCommandBuffer(Device device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Buffer CreateBuffer(Device device, BufferCreateInfo info, BufferMemoryCreateInfo memoryInfo);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyBuffer(Device device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_map_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MapBuffer(Device device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_unmap_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnmapBuffer(Device device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern ShaderModule CreateShaderModule(Device device, ShaderModuleCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyShaderModule(Device device, ShaderModule module);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern Shader CreateShader(Device device, ShaderCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern Image CreateImage(Device device, ImageCreateInfo info, string name, uint frame);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImage(Device device, Image image);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern ImageView CreateImageView(Device device, ImageViewCreateInfo info, string name, uint frame);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImageView(Device device, ImageView imageView);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Framebuffer CreateFramebuffer(Device device, FramebufferCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyFramebuffer(Device device, Framebuffer imageView);
        #endregion
    }
}
