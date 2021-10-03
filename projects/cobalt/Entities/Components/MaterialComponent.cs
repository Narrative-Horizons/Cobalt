using Cobalt.Graphics;
using System;

namespace Cobalt.Entities.Components
{
    public enum EMaterialType
    {
        Translucent,
        Opaque
    };

    public class UIEditorMaterialComponent : BaseComponent
    {
        public Texture Texture { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Texture);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UIEditorMaterialComponent casted))
            {
                return false;
            }

            return Texture == casted.Texture;
        }
    }
    public class PbrMaterialComponent : BaseComponent
    {
        public EMaterialType Type = EMaterialType.Opaque;
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
                   && Object.Equals(Albedo, casted.Albedo)
                   && Object.Equals(Normal, casted.Normal)
                   && Object.Equals(Emission, casted.Emission)
                   && Object.Equals(OcclusionRoughnessMetallic, casted.OcclusionRoughnessMetallic);
        }
    }
}
