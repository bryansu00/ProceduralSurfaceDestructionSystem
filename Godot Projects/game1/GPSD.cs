#nullable enable

using Godot;
using PSDSystem;
using System;
using System.Collections.Generic;

namespace PSDSystem
{
    /// <summary>
    /// ADD Comments...
    /// </summary>
    public static class GPSD
    {
        public static void ExtrudeSurface<T>(MeshInstance3D meshInstance,
            SurfaceShape<T> surface, 
            CoordinateConverter coordinateConverter,
            List<Vector2> originalVertices,
            List<Vector2> originalUVs,
            float extrusionDepth) where T : PolygonVertex
        {
            ArrayMesh? arrayMesh = new ArrayMesh();

            #region FRONT_FACE

            // Lists needed for the procedural mesh
            List<Vector3> frontVerts;
            List<Vector2>? frontUvs;
            List<Vector3> frontNormals = new List<Vector3>();
            List<int> frontIndices;

            // Triangulate the surface
            List<Vector2> verts2D;
            PSD.TriangulateSurface(surface, out frontIndices, out verts2D);
            if (frontIndices == null || verts2D == null)
            {
                // Given surface cannot be triangulated,
                // thus no mesh will be generated
                return;
            }

            // Convert triangulation into 3D
            frontVerts = coordinateConverter.ConvertListTo3D(verts2D);
            // Compute the UV coordinates of the front face
            frontUvs = PSD.ComputeUVCoordinates(originalVertices, originalUVs, verts2D);
            if (frontUvs == null) return;

            // Add the normals for the front face, it is Vector3.Back by default
            for (int i = 0; i < frontVerts.Count; i++)
            {
                frontNormals.Add(Vector3.Back);
            }

            // Create the arrays for mesh
            var frontSurfaceArray = new Godot.Collections.Array();
            frontSurfaceArray.Resize((int)Mesh.ArrayType.Max);

            // Convert Lists to arrays and assign to surface array
            frontSurfaceArray[(int)Mesh.ArrayType.Vertex] = frontVerts.ToArray();
            frontSurfaceArray[(int)Mesh.ArrayType.TexUV] = frontUvs.ToArray();
            frontSurfaceArray[(int)Mesh.ArrayType.Normal] = frontNormals.ToArray();
            frontSurfaceArray[(int)Mesh.ArrayType.Index] = frontIndices.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontSurfaceArray);

            #endregion

            #region BACK_FACE

            List<Vector3> backVerts = new List<Vector3>();
            List<Vector2> backUvs = new List<Vector2>();
            List<Vector3> backNormals = new List<Vector3>();
            List<int> backIndices = new List<int>();

            // Reuse the triangulation from prior computation to generate the backface
            for (int i = 0; i < verts2D.Count; i++)
            {
                backVerts.Add(frontVerts[i] - new Vector3(0.0f, 0.0f, extrusionDepth / 2.0f));
                backNormals.Add(Vector3.Forward);
            }

            // Copy the front face UVs, (Should be recalculated using a different
            // originalUVs list for the back face, else the texture will be flipped horizontally)
            backUvs.AddRange(frontUvs);

            // Add the indices of triangulation for the back face
            for (int i = frontIndices.Count - 1; i >= 0; i--)
            {
                backIndices.Add(frontIndices[i]);
            }

            var backSurfaceArray = new Godot.Collections.Array();
            backSurfaceArray.Resize((int)Mesh.ArrayType.Max);

            backSurfaceArray[(int)Mesh.ArrayType.Vertex] = backVerts.ToArray();
            backSurfaceArray[(int)Mesh.ArrayType.TexUV] = backUvs.ToArray();
            backSurfaceArray[(int)Mesh.ArrayType.Normal] = backNormals.ToArray();
            backSurfaceArray[(int)Mesh.ArrayType.Index] = backIndices.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backSurfaceArray);

            #endregion

            #region SIDE_CAP

            List<Vector3> sideVerts = new List<Vector3>();
            List<Vector2> sideUvs = new List<Vector2>();
            List<Vector3> sideNormals = new List<Vector3>();
            List<int> sideIndices = new List<int>();

            PSD.CreateSideCapOfSurface(surface, coordinateConverter, Vector3.Back, extrusionDepth / 2.0f,
                sideVerts, sideNormals, sideIndices, sideUvs);

            var sideSurfaceArray = new Godot.Collections.Array();
            sideSurfaceArray.Resize((int)Mesh.ArrayType.Max);

            sideSurfaceArray[(int)Mesh.ArrayType.Vertex] = sideVerts.ToArray();
            sideSurfaceArray[(int)Mesh.ArrayType.TexUV] = sideUvs.ToArray();
            sideSurfaceArray[(int)Mesh.ArrayType.Normal] = sideNormals.ToArray();
            sideSurfaceArray[(int)Mesh.ArrayType.Index] = sideIndices.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, sideSurfaceArray);

            #endregion

            meshInstance.Mesh = arrayMesh;
        }

        public static void GenerateCollisionShape<T>(StaticBody3D staticBody, SurfaceShape<T> surface, CoordinateConverter coordinateConverter, float extrusionDepth) where T : PolygonVertex
        {
            // Find Convex groups of vertices in 2D
            List<List<Vector2>>? convexVertices = PSD.FindConvexVerticesOfSurface(surface);

            // Clear nodes of collision from prior computation
            foreach (Node child in staticBody.GetChildren())
            {
                staticBody.RemoveChild(child);
                child.QueueFree();
            }

            if (convexVertices == null) return;

            // Create Collision Shape from each convex group of vertices
            foreach (List<Vector2> vertexGroup in convexVertices)
            {
                List<Vector3> vertices = coordinateConverter.ConvertListTo3D(vertexGroup);
                for (int i = vertices.Count - 1; i >= 0; i--)
                {
                    vertices.Add(vertices[i] - new Vector3(0.0f, 0.0f, extrusionDepth / 2.0f));
                }

                ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
                shape.Points = vertices.ToArray();

                CollisionShape3D collisionShape3D = new CollisionShape3D();
                collisionShape3D.Shape = shape;

                staticBody.AddChild(collisionShape3D);
            }
        }
    }
}
