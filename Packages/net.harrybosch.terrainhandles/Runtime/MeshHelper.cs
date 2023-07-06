// using System.Collections.Generic;
// using UnityEngine;
//
// namespace TerrainHandles
// {
//     public class MeshHelper
//     {
//         public List<Vertex> vertices = new();
//         public Dictionary<int, List<int>> subMeshes = new();
//
//         public int WorkingSubMesh { get; set; } = 0;
//
//         public List<int> Indices
//         {
//             get
//             {
//                 if (!subMeshes.ContainsKey(WorkingSubMesh)) subMeshes.Add(WorkingSubMesh, new List<int>());
//                 return subMeshes[WorkingSubMesh];
//             }
//         }
//         
//         public MeshHelper()
//         {
//             subMeshes.Add(0, new List<int>());
//         }
//         
//         public void Build(Mesh mesh)
//         {
//             var texCoordChannels = 0;
//             foreach (var v in vertices)
//             {
//                 if (v.texCoords.Length > texCoordChannels) texCoordChannels = v.texCoords.Length;
//             }
//
//             var verts = new Vector3[vertices.Count];
//             var normals = new Vector3[vertices.Count];
//             var texCoords = new Vector3[][texCoordChannels];
//             for (var i = 0; i < texCoordChannels; i++) texCoords[i] = new Vector3[vertices.Count];
//
//             for (var i = 0; i < vertices.Count; i++)
//             {
//                 verts[i] = vertices[i].position;
//                 normals[i] = vertices[i].normal;
//                 for (var j = 0; j < texCoordChannels; j++)
//                 {
//                     texCoords[j][i] = vertices[i].texCoords.Length < j ? vertices[i].texCoords[j] : default;
//                 }
//             }
//
//             mesh.SetVertices(verts);
//             mesh.SetNormals(normals);
//             for (var i = 0; i < texCoordChannels; i++)
//             {
//                 mesh.SetUVs(i, texCoords[i]);
//             }
//
//             foreach (var subMesh in subMeshes)
//             {
//                 mesh.SetIndices(subMesh.Value, MeshTopology.Triangles, subMesh.Key);
//             }
//         }
//
//         public void AddPoly(params Vertex[] points)
//         {
//             Vertex point(int i) => points[(i % points.Length + points.Length) % points.Length];
//
//             for (var i = 0; i < points.Length; i++)
//             {
//                 var v = point(i);
//                 var d = new[]
//                 {
//                     v - point(i - 1),
//                     v + point(i + 1),
//                 };
//
//                 var n = Vector3.Cross(d[0], d[1]).normalized;
//
//                 vertices.Add(new Vertex(v, n));
//             }
//
//             var working = new List<int>();
//             for (var i = 0; i < points.Length; i++) working.Add(i);
//             while (working.Count > 2)
//             {
//                 var a = working[0];
//                 var b = working[1];
//                 var c = working[2];
//
//                 Indices.Add(a);
//                 Indices.Add(b);
//                 Indices.Add(c);
//
//                 working.RemoveAt(1);
//             }
//         }
//
//         public void AddTri(Vertex a, Vertex b, Vertex c)
//         {
//             var i = vertices.Count;
//             vertices.Add(a);
//             vertices.Add(b);
//             vertices.Add(c);
//
//             for (var j = 0; j < 3; j++)
//             {
//                 Indices.Add(i + j);
//             }
//         }
//
//         public void MergeDoubles(float threshold = 0.001f)
//         {
//             foreach (var a in Indices)
//             foreach (var b in Indices)
//             {
//                 var pa = vertices[a];
//                 var pb = vertices[b];
//                 if ((pa - pb).sqrMagnitude > threshold * threshold) continue;
//                 Merge(a, b);
//                 MergeDoubles(threshold);
//                 return;
//             }
//         }
//
//         public void Merge(int from, int to)
//         {
//             for (var i = 0; i < subMeshes.Count; i++)
//             {
//                 if (Indices[i] == to) Indices[i] = from;
//                 if (Indices[i] > to) Indices[i]--;
//             }
//
//             vertices.RemoveAt(to);
//         }
//
//         public struct Vertex
//         {
//             public Vector3 position, normal;
//             public Vector2[] texCoords;
//
//             public Vertex(Vector3 position, Vector3 normal, params Vector2[] texCoords)
//             {
//                 this.position = position;
//                 this.normal = normal;
//                 this.texCoords = texCoords;
//             }
//
//             public static implicit operator Vector3(Vertex vertex) => vertex.position;
//
//             public static Vector3 operator +(Vertex a, Vertex b) => a.position + b.position;
//             public static Vector3 operator -(Vertex a, Vertex b) => a.position - b.position;
//
//             public static Vertex[] FromPoints(params Vector3[] points)
//             {
//                 var l = points.Length;
//                 var res = new Vertex[points.Length];
//
//                 for (var i = 0; i < l; i++)
//                 {
//                     var a = index(points, i - 1);
//                     var b = index(points, i);
//                     var c = index(points, i + 1);
//
//                     res[i].position = b;
//                     res[i].normal = Vector3.Cross(c - b, b - a).normalized;
//                 }
//
//                 return res;
//
//                 T index<T>(T[] array, int i) => array[(i % array.Length + array.Length) % array.Length];
//             }
//         }
//     }
// }