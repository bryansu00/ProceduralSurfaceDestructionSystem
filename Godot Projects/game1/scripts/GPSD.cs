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

            CreateSideCapOfSurface(surface, coordinateConverter, Vector3.Back, extrusionDepth / 2.0f,
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

        /// <summary>
        /// Generate the values needed for the sides of the surfaces
        /// </summary>
        /// <typeparam name="T">The original polygon vertex type of polygon and surface</typeparam>
        /// <param name="surface">The surface to create the sides for</param>
        /// <param name="coordinateConverter">The CoordinateConverter needed for translating from 2D to 3D</param>
        /// <param name="frontNormal">The normal of the front face of the surface, needed to determine the direction of the sides cap to extrude to</param>
        /// <param name="depth">The depth of the sides</param>
        /// <param name="vertices">List of vertices to append the new vertices into</param>
        /// <param name="normals">List of normals to append the new normals into</param>
        /// <param name="indices">List of triangles to append the new indices into</param>
        /// <param name="uvs">List of UVs to append the new uvs into</param>
        public static void CreateSideCapOfSurface<T>(SurfaceShape<T> surface, CoordinateConverter coordinateConverter, Vector3 frontNormal, float depth,
            List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs) where T : PolygonVertex
        {
            frontNormal = frontNormal.Normalized();

            // For each group...
            foreach (PolygonGroup<T> group in surface.Polygons)
            {
                // Create side cap for each line segment of the outer polygon
                List<Vector2> outerVertices = group.OuterPolygon.Vertices;
                VertexNode<T> outerNow = group.OuterPolygon.Head;
                do
                {
                    int verticesPreviousCount = vertices.Count;

                    Vector3 a = coordinateConverter.ConvertTo3D(outerVertices[outerNow.Data.Index]);
                    Vector3 b = coordinateConverter.ConvertTo3D(outerVertices[outerNow.Previous.Data.Index]);

                    Vector3 aCopy = a - (frontNormal * depth);
                    Vector3 bCopy = b - (frontNormal * depth);

                    // Add these vertices to the list
                    vertices.Add(a);
                    vertices.Add(b);
                    vertices.Add(bCopy);
                    vertices.Add(aCopy);

                    // Add the indices for the triangle
                    indices.Add(verticesPreviousCount + 0);
                    indices.Add(verticesPreviousCount + 1);
                    indices.Add(verticesPreviousCount + 2);
                    // The other triangle
                    indices.Add(verticesPreviousCount + 2);
                    indices.Add(verticesPreviousCount + 3);
                    indices.Add(verticesPreviousCount + 0);

                    // Calculate normal for the triangles and add it to the list of normals
                    Vector3 triangleNormal = (bCopy - a).Cross(b - a).Normalized();
                    normals.Add(triangleNormal);
                    normals.Add(triangleNormal);
                    normals.Add(triangleNormal);
                    normals.Add(triangleNormal);

                    // Add uvs...
                    uvs.Add(Vector2.Zero);
                    uvs.Add(Vector2.Zero);
                    uvs.Add(Vector2.Zero);
                    uvs.Add(Vector2.Zero);

                    outerNow = outerNow.Previous; // Assuming the outer polygon is CW, we must do this CCW to get correct triangles
                } while (outerNow != group.OuterPolygon.Head);

                // Now for each inner polygons
                foreach (Polygon<T> innerPolygon in group.InnerPolygons)
                {
                    List<Vector2> innerVertices = innerPolygon.Vertices;
                    VertexNode<T> innerNow = innerPolygon.Head;
                    do
                    {
                        int verticesPreviousCount = vertices.Count;

                        Vector3 a = coordinateConverter.ConvertTo3D(innerVertices[innerNow.Data.Index]);
                        Vector3 b = coordinateConverter.ConvertTo3D(innerVertices[innerNow.Previous.Data.Index]);

                        Vector3 aCopy = a - (frontNormal * depth);
                        Vector3 bCopy = b - (frontNormal * depth);

                        // Add these vertices to the list
                        vertices.Add(a);
                        vertices.Add(b);
                        vertices.Add(bCopy);
                        vertices.Add(aCopy);

                        // Add the indices for the triangle
                        indices.Add(verticesPreviousCount + 0);
                        indices.Add(verticesPreviousCount + 1);
                        indices.Add(verticesPreviousCount + 2);
                        // The other triangle
                        indices.Add(verticesPreviousCount + 2);
                        indices.Add(verticesPreviousCount + 3);
                        indices.Add(verticesPreviousCount + 0);

                        // Calculate normal for the triangles and add it to the list of normals
                        Vector3 triangleNormal = (bCopy - a).Cross(b - a).Normalized();
                        normals.Add(triangleNormal);
                        normals.Add(triangleNormal);
                        normals.Add(triangleNormal);
                        normals.Add(triangleNormal);

                        // Add uvs...
                        uvs.Add(Vector2.Zero);
                        uvs.Add(Vector2.Zero);
                        uvs.Add(Vector2.Zero);
                        uvs.Add(Vector2.Zero);

                        innerNow = innerNow.Previous;
                    } while (innerNow != innerPolygon.Head);
                }
            }
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
