using Cobalt.Math;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public static class GraphicsPrimitives
    {
        public static void IcoSphere(float radius, uint recursionLevel)
        {
            static uint getMiddlePoint(uint p1, uint p2, ref List<Vector3> vertices, ref Dictionary<long, uint> cache, float radius)
            {
                bool firstIsSmaller = p1 < p2;
                long smallerIndex = firstIsSmaller ? p1 : p2;
                long greaterIndex = firstIsSmaller ? p2 : p1;
                long key = (smallerIndex << 32) + greaterIndex;

                if (cache.TryGetValue(key, out uint ret))
                {
                    return ret;
                }

                Vector3 point1 = vertices[(int)p1];
                Vector3 point2 = vertices[(int)p2];
                Vector3 middle = new Vector3
                (
                    (point1.x + point2.x) / 2.0f,
                    (point1.y + point2.y) / 2.0f,
                    (point1.z + point2.z) / 2.0f
                );

                uint i = (uint)vertices.Count;
                vertices.Add(middle.Normalized() * radius);

                cache.Add(key, i);

                return i;
            }

            List<Vector3> vertexList = new List<Vector3>();
            Dictionary<long, uint> middlePointIndexCache = new Dictionary<long, uint>();

            float t = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;

            vertexList.Add(new Vector3(-1.0f, t, 0.0f).Normalized() * radius);
            vertexList.Add(new Vector3(1.0f, t, 0.0f).Normalized() * radius);
            vertexList.Add(new Vector3(-1.0f, -t, 0.0f).Normalized() * radius);
            vertexList.Add(new Vector3(1.0f, -t, 0.0f).Normalized() * radius);

            vertexList.Add(new Vector3(0.0f, -1f, t).Normalized() * radius);
            vertexList.Add(new Vector3(0.0f, 1f, t).Normalized() * radius);
            vertexList.Add(new Vector3(0.0f, -1f, -t).Normalized() * radius);
            vertexList.Add(new Vector3(0.0f, 1f, -t).Normalized() * radius);

            vertexList.Add(new Vector3(t, 0.0f, -1.0f).Normalized() * radius);
            vertexList.Add(new Vector3(t, 0.0f, 1.0f).Normalized() * radius);
            vertexList.Add(new Vector3(-t, 0.0f, -1.0f).Normalized() * radius);
            vertexList.Add(new Vector3(-t, 0.0f, 1.0f).Normalized() * radius);

            List<Tuple<uint, uint, uint>> faces = new List<Tuple<uint, uint, uint>>
            {
                new Tuple<uint, uint, uint>(0, 11, 5),
                new Tuple<uint, uint, uint>(0, 5, 1),
                new Tuple<uint, uint, uint>(0, 1, 7),
                new Tuple<uint, uint, uint>(0, 7, 10),
                new Tuple<uint, uint, uint>(0, 10, 11),

                new Tuple<uint, uint, uint>(1, 5, 9),
                new Tuple<uint, uint, uint>(5, 11, 4),
                new Tuple<uint, uint, uint>(11, 10, 2),
                new Tuple<uint, uint, uint>(10, 7, 6),
                new Tuple<uint, uint, uint>(7, 1, 8),

                new Tuple<uint, uint, uint>(3, 9, 4),
                new Tuple<uint, uint, uint>(3, 4, 2),
                new Tuple<uint, uint, uint>(3, 2, 6),
                new Tuple<uint, uint, uint>(3, 6, 8),
                new Tuple<uint, uint, uint>(3, 8, 9),

                new Tuple<uint, uint, uint>(4, 9, 5),
                new Tuple<uint, uint, uint>(2, 4, 11),
                new Tuple<uint, uint, uint>(6, 2, 10),
                new Tuple<uint, uint, uint>(8, 6, 7),
                new Tuple<uint, uint, uint>(9, 8, 1),
            };

            for (int i = 0; i < recursionLevel; i++)
            {
                List<Tuple<uint, uint, uint>> faces2 = new List<Tuple<uint, uint, uint>>();
                foreach (Tuple<uint, uint, uint> tri in faces)
                {
                    uint a = getMiddlePoint(tri.Item1, tri.Item2, ref vertexList, ref middlePointIndexCache, radius);
                    uint b = getMiddlePoint(tri.Item1, tri.Item2, ref vertexList, ref middlePointIndexCache, radius);
                    uint c = getMiddlePoint(tri.Item1, tri.Item2, ref vertexList, ref middlePointIndexCache, radius);

                    faces2.Add(new Tuple<uint, uint, uint>(tri.Item1, a, c));
                    faces2.Add(new Tuple<uint, uint, uint>(tri.Item2, b, a));
                    faces2.Add(new Tuple<uint, uint, uint>(tri.Item3, c, b));
                    faces2.Add(new Tuple<uint, uint, uint>(a, b, c));
                }
                faces = faces2;
            }

            List<uint> triList = new List<uint>();
            for (int i = 0; i < faces.Count; i++)
            {
                triList.Add(faces[i].Item1);
                triList.Add(faces[i].Item2);
                triList.Add(faces[i].Item3);
            }

            Vector2[] UVs = new Vector2[vertexList.Count];
            for (int i = 0; i < vertexList.Count; i++)
            {
                Vector3 unitVector = vertexList[i].Normalized();
                Vector2 ICOuv = Vector2.Zero;
                ICOuv.x = (MathF.Atan2(unitVector.x, unitVector.z) + MathF.PI) / MathF.PI / 2;
                ICOuv.y = (MathF.Acos(unitVector.y) + MathF.PI) / MathF.PI - 1;
                UVs[i] = new Vector2(ICOuv.x, ICOuv.y);
            }

            Vector3[] normals = new Vector3[vertexList.Count];
            for (int i = 0; i < vertexList.Count; i++)
            {
                normals[i] = vertexList[i].Normalized();
            }
        }
    }
}
