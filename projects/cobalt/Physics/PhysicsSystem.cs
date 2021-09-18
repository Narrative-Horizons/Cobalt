using System;
using System.Collections.Generic;
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

        public PhysicsSystem(Registry registry)
        {
            _registry = registry;
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
                        comp.Rotation = new Vector3(transforms->rx, transforms->ry, transforms->rz);

                        comp.TransformMatrix = Matrix4.Rotate(comp.Rotation) * Matrix4.Translate(comp.Position);
                    }
                }
            }
        }
    }
}
