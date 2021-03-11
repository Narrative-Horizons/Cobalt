using System;

namespace Cobalt.Graphics.API
{
    public class Viewport : IEquatable<Viewport>
    {
        public float LeftX { get; set; }
        public float UpperY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float MinDepth { get; set; }
        public float MaxDepth { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is Viewport viewport)
            {
                return Equals(viewport);
            }

            return false;
        }

        public bool Equals(Viewport other)
        {
            return (LeftX == other.LeftX && UpperY == other.UpperY && Width == other.Width && Height == other.Height && MinDepth == other.MinDepth && MaxDepth == other.MaxDepth);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LeftX, UpperY, Width, Height, MinDepth, MaxDepth);
        }
    }
}
