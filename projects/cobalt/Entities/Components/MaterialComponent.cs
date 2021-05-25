using Cobalt.Graphics;
using System;

namespace Cobalt.Entities.Components
{
    public enum EMaterialType
    {
        Translucent,
        Opaque
    };

    public class PbrMaterialComponent : BaseComponent
    {
        public EMaterialType Type;
        public Texture Albedo;
        public Texture Normal;
        public Texture Emission;
        public Texture OcclusionRoughnessMetallic;

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Albedo, Normal, Emission, OcclusionRoughnessMetallic);
        }

        public override bool Equals(object obj)
        {
            PbrMaterialComponent casted = obj as PbrMaterialComponent;
            if (casted == null)
            {
                return false;
            }
            return Type.Equals(casted.Type)
                   && Albedo != null
                   && Albedo.Equals(casted.Albedo)
                   && Normal != null
                   && Normal.Equals(casted.Normal)
                   && Emission != null
                   && Emission.Equals(casted.Emission)
                   && OcclusionRoughnessMetallic != null
                   && OcclusionRoughnessMetallic.Equals(casted.OcclusionRoughnessMetallic);
        }
    }
}
