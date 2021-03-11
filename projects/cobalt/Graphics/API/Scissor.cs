using System;

namespace Cobalt.Graphics.API
{
    public class Scissor : IEquatable<Scissor>
    {
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int ExtentX { get; set; }
        public int ExtentY { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is Scissor scissor)
            {
                return Equals(scissor);
            }

            return false;
        }
        public bool Equals(Scissor other)
        {
            return OffsetX == other.OffsetX && OffsetY == other.OffsetY && ExtentX == other.ExtentX && ExtentY == other.ExtentY;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OffsetX, OffsetY, ExtentX, ExtentY);
        }
    }
}
