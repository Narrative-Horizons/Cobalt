using System;

namespace Cobalt.Entities
{
    public struct Entity
    {
        public uint Generation { get; internal set; }
        public uint Identifier { get; internal set; }

        public static Entity Invalid = new Entity { Generation = ~0U, Identifier = ~0U };

        public bool IsInvalid => Identifier == ~0U;

        public override bool Equals(object obj)
        {
            if (obj is Entity vector)
            {
                return Equals(vector);
            }
            return false;
        }

        public bool Equals(Entity other)
        {
            return Identifier == other.Identifier && Generation == other.Generation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier, Generation);
        }

        public static bool operator ==(Entity lhs, Entity rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Entity lhs, Entity rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
