using Cobalt.Core;
using Cobalt.Graphics;

namespace Cobalt.Sandbox
{
    public class Editor : BaseApplication
    {
        public RenderSystem RenderSystem { get; internal set; }

        public override void Setup()
        {
            var engine = Engine<Editor>.Instance();

            var window = engine.CreateWindow(new Window.CreateInfo.Builder()
                .Width(1280)
                .Height(720)
                .Name("Cobalt Editor")
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
            var rs = Engine<Editor>.Instance().Render;
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
            Engine<Editor>.Initialize(new Editor());
            Engine<Editor>.Instance().Run();
            Engine<Editor>.Destruct();
        }
    }
}