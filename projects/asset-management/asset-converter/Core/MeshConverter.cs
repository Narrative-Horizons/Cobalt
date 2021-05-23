using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Silk.NET.Assimp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CobaltConverter.Core
{
    [Serializable]
    public struct MeshVector2
    {
        public MeshVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x, y;
    }
    
    [Serializable]
    public struct MeshVector3
    {
        public MeshVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x, y, z;
    }

    [Serializable]
    public struct MeshVector4
    {
        public MeshVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x, y, z, w;
    }

    [Serializable]
    public struct MeshMatrix4
    {
        public static MeshMatrix4 Identity()
        {
            MeshMatrix4 m = new MeshMatrix4();
            m.m00 = 1;
            m.m11 = 1;
            m.m22 = 1;
            m.m33 = 1;

            return m;
        }

        public float m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex
    {
        public MeshVector3 position;
        public MeshVector2[] texcoords;
        public MeshVector3 normal;
        public MeshVector3 tangent;
        public MeshVector3 binormal;
        public MeshVector4[] colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshFace
    {
        public uint[] indices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMesh
    {
        public MeshVertex[] Vertices { get; internal set; }
        public MeshFace[] Faces { get; internal set; }

        public uint MaterialIndex { get; internal set; }
    }

    public enum CobaltMeshTextureType
    {
        BaseColor = 0,
        Normal,
        Emission,
        RoughnessMetallic,
        OcclusionRoughnessMetallic,
        Occlusion,
        Roughness,
        Metallic
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMeshTexture
    {
        public CobaltMeshTextureType Type { get; internal set; }
        public string Path { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMeshMaterial
    {
        public string Name { get; internal set; }
        public CobaltMeshTexture[] Textures { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CobaltModelNode
    {
        public string Name { get; internal set; }
        public MeshMatrix4 Transformation { get; internal set; }
        //public CobaltModelNode Parent { get; internal set; } = null;
        public CobaltModelNode[] Children { get; internal set; } = null;
        public uint[] Meshes { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CobaltModel
    {
        public uint Flags { get; internal set; }
        public CobaltModelNode RootNode { get; internal set; }
        public CobaltMesh[] Meshes { get; internal set; }
        public CobaltMeshMaterial[] Materials { get; internal set; }

        /// TODO: Animations
        /// TODO: Other scene objects (lights, camera, etc)
    }

    public class MeshConverter
    {
        private static unsafe void ProcessMaterials(CobaltModel rootModel, Node* assNode, Scene* assScene)
        {
            uint numMaterials = assScene->MNumMaterials;

            if (numMaterials == 0)
                return;

            rootModel.Materials = new CobaltMeshMaterial[numMaterials];
            Assimp assimp = Assimp.GetApi();

            for (int i = 0; i < numMaterials; i++)
            {
                CobaltMeshMaterial material = new CobaltMeshMaterial();
                rootModel.Materials[i] = material;

                material.Name = assScene->MMaterials[i]->ToString();
            }
        }

        private static unsafe void ProcessNode(CobaltModel rootModel, CobaltModelNode node, Node* assNode, Scene* assScene, MeshMatrix4 parentMatrix)
        {
            node.Meshes = new uint[assNode->MNumMeshes];
            node.Name = assNode->MName;

            for (int i = 0; i < assNode->MNumMeshes; i++)
            {
                Mesh* assMesh = assScene->MMeshes[assNode->MMeshes[i]];

                rootModel.Meshes[assNode->MMeshes[i]] = ProcessMesh(node, assMesh, assScene);

                node.Meshes[i] = assNode->MMeshes[i];
            }

            node.Children = new CobaltModelNode[assNode->MNumChildren];
            for(int i = 0; i < assNode->MNumChildren; i++)
            {
                CobaltModelNode childNode = new CobaltModelNode();
                ProcessNode(rootModel, childNode, assNode->MChildren[i], assScene, parentMatrix);

                //childNode.Parent = node;
                node.Children[i] = childNode;
            }
        }

        private static unsafe CobaltMesh ProcessMesh(CobaltModelNode node, Mesh* assMesh, Scene* assScene)
        {
            CobaltMesh mesh = new CobaltMesh
            {
                Vertices = new MeshVertex[assMesh->MNumVertices],
                Faces = new MeshFace[assMesh->MNumFaces],
                MaterialIndex = assMesh->MMaterialIndex
            };

            for(int i = 0; i < assMesh->MNumVertices; i++)
            {
                mesh.Vertices[i].position = new MeshVector3(assMesh->MVertices[i].X, assMesh->MVertices[i].Y, assMesh->MVertices[i].Z);

                if (assMesh->MNormals != null)
                    mesh.Vertices[i].normal = new MeshVector3(assMesh->MNormals[i].X, assMesh->MNormals[i].Y, assMesh->MNormals[i].Z);
                else
                    mesh.Vertices[i].normal = new MeshVector3(0, 1, 0);

                mesh.Vertices[i].texcoords = new MeshVector2[1];

                if (assMesh->MTextureCoords.Element0 != null)
                    mesh.Vertices[i].texcoords[0] = new MeshVector2(assMesh->MTextureCoords.Element0[i].X, assMesh->MTextureCoords.Element0[i].Y);
                else
                    mesh.Vertices[i].texcoords[0] = new MeshVector2(0, 0);

                if (assMesh->MTangents != null)
                    mesh.Vertices[i].tangent = new MeshVector3(assMesh->MTangents[i].X, assMesh->MTangents[i].Y, assMesh->MTangents[i].Z);
                else
                    mesh.Vertices[i].tangent = new MeshVector3(1, 0, 0);

                if (assMesh->MBitangents != null)
                    mesh.Vertices[i].binormal = new MeshVector3(assMesh->MBitangents[i].X, assMesh->MBitangents[i].Y, assMesh->MBitangents[i].Z);
                else
                    mesh.Vertices[i].binormal = new MeshVector3(0, 0, 1);
            }

            for(int i = 0; i < assMesh->MNumFaces; i++)
            {
                MeshFace face = new MeshFace
                {
                    indices = new uint[assMesh->MFaces[i].MNumIndices]
                };

                for (int j = 0; j < face.indices.Length; j++)
                {
                    face.indices[j] = assMesh->MFaces[i].MIndices[j];
                }

                mesh.Faces[i] = face;
            }

            return mesh;
        }

        public static CobaltModel ConvertModel(string path)
        {
            CobaltModel model = new CobaltModel();
            unsafe
            {
                Assimp assimp = Assimp.GetApi();
                Scene* scene = assimp.ImportFile(path, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

                model.Meshes = new CobaltMesh[scene->MNumMeshes];
                model.Materials = new CobaltMeshMaterial[scene->MNumMaterials];
                model.Flags = scene->MFlags;
                model.RootNode = new CobaltModelNode();

                /// TODO: User assimp matrices
                ProcessNode(model, model.RootNode, scene->MRootNode, scene, MeshMatrix4.Identity());
            }

            return model;
        }

        public static void ExportBinary(MemoryStream exportStream, CobaltModel model)
        {
            using(BsonDataWriter writer = new BsonDataWriter(exportStream))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, model);
            }
        }

        public static void Export(MemoryStream exportStream, CobaltModel model)
        {
            using (StreamWriter wr = new StreamWriter(exportStream))
            {
                using(JsonWriter writer = new JsonTextWriter(wr))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, model);
                }
            }
        }

        public static CobaltModel Import(string path)
        {
            if(path.Contains(".caf"))
            {
                string data = System.IO.File.ReadAllText(path);
                return JsonConvert.DeserializeObject<CobaltModel>(data);
            }
            else if(path.Contains(".bcaf"))
            {
                MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(path));
                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return serializer.Deserialize<CobaltModel>(reader);
                }
            }

            return null;
        }
    }
}
