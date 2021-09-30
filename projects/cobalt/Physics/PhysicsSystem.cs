using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using Cobalt.Bindings.PhysX;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Math;

namespace Cobalt.Physics
{
    public class PhysicsSystem
    {
        private readonly Registry _registry;
        private readonly EntityView _physicsView;

        public PhysicsSystem(Registry registry)
        {
            _registry = registry;
            _physicsView = _registry.GetView().Requires<TransformComponent>().Requires<RigidBodyComponent>();
        }

        public void Simulate()
        {
            PhysX.Simulate();
        }

        public void Update()
        {
            var results = PhysX.FetchResults();
            if (results.count > 0)
            {
                unsafe
                {
                    PhysX.PhysicsTransform* transforms = results.transforms;
                    for (int i = 0; i < results.count; i++)
                    {
                        PhysX.PhysicsTransform transform = transforms[i];

                        Entity e = new Entity()
                        {
                            Generation = transform.generation,
                            Identifier = transform.identifier
                        };

                        TransformComponent comp = _registry.Get<TransformComponent>(e);
                        comp.Position = new Vector3(transforms->x, transforms->y, transforms->z);
                        comp.Rotation = new Quaternion(transforms->rx, transforms->ry, transforms->rz, transforms->rw);
                        comp.isDirty = false;
                    }
                }
            }
        }

        internal void Sync()
        {
            var results = PhysX.GetResults();
            if (results.count == 0)
                return;

            int index = 0;
            _physicsView.ForEach((ent, reg) =>
            {
                TransformComponent transform = reg.Get<TransformComponent>(ent);
                if (transform.isDirty)
                {
                    unsafe
                    {
                        PhysX.PhysicsTransform* transforms = results.transforms;
                        transforms[index++] = new PhysX.PhysicsTransform()
                        {
                            generation = ent.Generation,
                            identifier = ent.Identifier,
                            x = transform.Position.x,
                            y = transform.Position.y,
                            z = transform.Position.z,
                            rx = transform.Rotation.x,
                            ry = transform.Rotation.y,
                            rz = transform.Rotation.z,
                            rw = transform.Rotation.w
                        };
                    }
                }
            });

            PhysX.Sync();
        }
    }
}
