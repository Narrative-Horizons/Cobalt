using Cobalt.Math;

namespace Cobalt.Entities.Components
{
    public class CameraComponent : ScriptableComponent
    {
        #region Camera Data
        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }

        public float FieldOfView { get; set; }

        public bool Orthographic { get; set; }
        public float OrthographicSize { get; set; }

        public float Aspect { get; set; }
        #endregion

        #region Camera Values
        public Matrix4 ProjectionMatrix
        {
            get
            {
                if (!Orthographic)
                    return Matrix4.Perspective(FieldOfView, Aspect, NearClipPlane, FarClipPlane);
                else
                    return Matrix4.Identity;
            }
        }
        public Matrix4 ViewMatrix
        { 
            get
            {
                if (!Orthographic)
                    return Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
                else
                    return Matrix4.Identity;
            }
        }

        public static CameraComponent Current { get; internal set; }

        public TransformComponent Transform { get; private set; }

        #endregion

        public override void OnInit()
        {
            Transform = Registry.Get<TransformComponent>(Owner);
        }
    }
}
