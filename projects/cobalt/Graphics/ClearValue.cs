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

            public ClearColor(float red, float green, float blue, float alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }
        }

        public sealed class ClearColorUi
        {
            public uint Red { get; set; } = 0;
            public uint Green { get; set; } = 0;
            public uint Blue { get; set; } = 0;
            public uint Alpha { get; set; } = 0;

            public ClearColorUi(uint red, uint green, uint blue, uint alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }
        }

        public ClearValue(ClearColor color)
        {
            Color = color;
        }

        public ClearValue(float depth)
        {
            Depth = depth;
        }

        public ClearValue(ClearColorUi color)
        {
            ColorUi = color;
        }

        public float? Depth { get; set; } = null;

        public ClearColor Color { get; set; } = null;

        public ClearColorUi ColorUi { get; set; } = null;
    }
}
