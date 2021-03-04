using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindGraphicsPipelineCommand : ICommand
    {
        public GraphicsPipeline Pipeline { get; private set; }

        public BindGraphicsPipelineCommand(GraphicsPipeline pipeline)
        {
            Pipeline = pipeline;
        }


        public void Dispose()
        {

        }

        public void Execute()
        {
            // TODO: Bind graphics pipeline
        }
    }
}
