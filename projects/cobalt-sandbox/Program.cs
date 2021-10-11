using System;
using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Math;
using System.Runtime.InteropServices;
using Cobalt.Bindings.PhysX;
using Cobalt.Graphics.VK;

namespace Cobalt.Sandbox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 position;
        public Vector2 uv;
    }

    public class Sandbox : BaseApplication
    {
        public RenderSystem RenderSystem { get; internal set; }

        public override void Setup()
        {
            var engine = Engine<Sandbox>.Instance();

            var window = engine.CreateWindow(new Window.CreateInfo.Builder()
                .Width(1280)
                .Height(720)
                .Name("Cobalt Sandbox")
                .Build());
            engine.CreateGraphicsContext(window);
        }

        public override void Initialize()
        {
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            var rs = Engine<Sandbox>.Instance().Render;
            rs.PreRender();
            rs.Render();
            rs.PostRender();
        }

        public override void Cleanup()
        {
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Engine<Sandbox>.Initialize(new Sandbox());
            Engine<Sandbox>.Instance().Run();
            Engine<Sandbox>.Destruct();
        }
    }
}