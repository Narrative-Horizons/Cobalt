using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public class ClearValue
    {
        public sealed class ClearColor
        {
            public float Red { get; set; } = 0;
            public float Green { get; set; } = 0;
            public float Blue { get; set; } = 0;
            public float Alpha { get; set; } = 0;
        }

        public ClearColor color { get; set; }
    }
}
