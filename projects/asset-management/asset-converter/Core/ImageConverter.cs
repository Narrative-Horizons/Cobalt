using static Cobalt.Bindings.STB.ImageLoader;

namespace CobaltConverter.Core
{
    public class ImageConverter
    {
        public static ImagePayload ConvertToORM(string roughnessMetallicPath, string occlusionPath)
        {
            ImagePayload ORMPayload = CombineORMImage(roughnessMetallicPath, occlusionPath);

            return ORMPayload;
        }
    }
}
