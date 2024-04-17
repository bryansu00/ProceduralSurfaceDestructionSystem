#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PSDSystem
{
    public static class PSD
    {
        public enum IntersectionResult
        {
            FAILED = -1,
            INTERSECTS = 0,
            CUTTER_IS_INSIDE = 1,
            POLYGON_IS_INSIDE = 2,
            BOTH_OUTSIDE = 3
        }

        public enum CutSurfaceResult
        {
            UNKNOWN_ERROR = -2,
            FAILED = -1,
            OVERLAPPED_OUTER = 0,
            CUTTER_INSIDE_OF_INNER = 1,
            CUTTER_INSIDE_OUTER_BUT_NOT_INNERS = 2,
            CUTTER_INSIDE_OUTER_OVERLAPPED_INNER = 3,
            CUTTER_INSIDE_OUTER_OVERLAPPED_INNER_PRODUCE_MULTI = 4
        }

        /// <summary>
        /// Given a surface and a polygon that represent the cutter, cut a 'hole' on to the surface
        /// </summary>
        /// <typeparam name="T">The original polygon vertex type of polygon and surface</typeparam>
        /// <typeparam name="U">The polygon vertex containing properties needed for polygon boolean operations</typeparam>
        /// <param name="surface">The surface to be cut</param>
        /// <param name="cutter">The cutter polygon</param>
        /// <returns>
        /// -2 if something went wrong (reason is unknown).
        /// -1 if the operation can not be performed.
        /// 0 if the cutter polygon overlaps with one or more outer polygons, and one or more outer polygons was replaced with new one
        /// 1 if the cutter polygon is completely inside of an inner polygon, thus operation cannot be performed.
        /// 2 if the cutter polygon is completely inside of an outer polygon, does not overlap with any inner polygon, 
        /// thus the cutter polygon can be placed as an inner polygon without any boolean operations.
        /// 3 if the cutter polygon is completely inside of an outer polygon, does overlap with any inner polygon, thus boolean operation was performed producing only 1 polygon.
        /// 4 if the cutter polygon is completely inside of an outer polygon, does overlap with any inner polygon, thus boolean operation was performed producing MULTIPLE polygons, but only 1 of those polygons were kept.
        /// </returns>
        public static CutSurfaceResult CutSurface<T, U>(SurfaceShape<T> surface, Polygon<T> cutter)
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            if (surface.Polygons.Count == 0 || cutter.Head == null || cutter.Vertices == null) return CutSurfaceResult.FAILED;

            // Keep track of what groups to remove
            List<PolygonGroup<T>> groupsToRemove = new List<PolygonGroup<T>>();

            // List of tuples to store boolean lists and intersectionPoints generated through performing intersection tests.
            List<Tuple<PolygonGroup<T>, Polygon<U>, IntersectionPoints<U>>> intersectedGroups =
                new List<Tuple<PolygonGroup<T>, Polygon<U>, IntersectionPoints<U>>>();

            // If the cutter happens to be completely inside of a polygon, store that group
            PolygonGroup<T>? groupCutterIsIn = null;

            // Perform intersection test with cutter against all outer polygons
            foreach (PolygonGroup<T> group in surface.Polygons)
            {
                Polygon<U> booleanCutter = ConvertPolygonToBooleanList<T, U>(cutter);
                Polygon<U> booleanPolygon = ConvertPolygonToBooleanList<T, U>(group.OuterPolygon);
                IntersectionResult result = IntersectCutterAndPolygon(booleanCutter, booleanPolygon, out IntersectionPoints<U>? intersectionPoints);

                if (result == IntersectionResult.CUTTER_IS_INSIDE)
                {
                    groupCutterIsIn = group;
                    break;
                }

                switch (result)
                {
                    case IntersectionResult.INTERSECTS:
                        // Store intersection result and remove the intersected groups
                        intersectedGroups.Add(new Tuple<PolygonGroup<T>, Polygon<U>, IntersectionPoints<U>>(group, booleanCutter, intersectionPoints));
                        groupsToRemove.Add(group);
                        break;
                    case IntersectionResult.POLYGON_IS_INSIDE:
                        // Remove the group
                        groupsToRemove.Add(group);
                        break;
                    default:
                        break;
                }
            }

            // Remove the groups
            foreach (PolygonGroup<T> group in groupsToRemove)
            {
                surface.RemoveGroup(group);
            }

            // Case 2
            // For each outer polygon that was intersected...
            foreach (Tuple<PolygonGroup<T>, Polygon<U>, IntersectionPoints<U>> tuple in intersectedGroups)
            {
                PolygonGroup<T> group = tuple.Item1;
                Polygon<U> booleanCutter = tuple.Item2;
                IntersectionPoints<U> outerIntersectionPoints = tuple.Item3;

                // Intersect each inner polygon of the group
                List<Polygon<T>> nonIntersectedInnerPolygons = new List<Polygon<T>>();
                List<Polygon<U>> innerPolygonsToCombineWith = new List<Polygon<U>>();
                List<IntersectionPoints<U>> listOfInnerIntersections = new List<IntersectionPoints<U>>();
                foreach (Polygon<T> innerPolygon in group.InnerPolygons)
                {
                    Polygon<U> booleanInnerPolygon = ConvertPolygonToBooleanList<T, U>(innerPolygon);
                    // Doing the operation below will incorrectly modify the IsOutside flag
                    // TO DO: FIX THIS LOGIC
                    IntersectionResult result = IntersectCutterAndPolygon(booleanCutter, booleanInnerPolygon, out IntersectionPoints<U>? intersectionPoints);

                    switch (result)
                    {
                        case IntersectionResult.INTERSECTS:
                            // Store Intersection result
                            innerPolygonsToCombineWith.Add(booleanInnerPolygon);
                            listOfInnerIntersections.Add(intersectionPoints);
                            break;
                        case IntersectionResult.BOTH_OUTSIDE:
                            // Keep the polygon
                            nonIntersectedInnerPolygons.Add(innerPolygon);
                            break;
                        default:
                            break;
                    }
                }

                // The implementation of CombinePolygons below has a work around for the incorrect logic define above. Should be fixed
                List<Polygon<T>> polygonsProduced = CombinePolygons<T, U>(booleanCutter, outerIntersectionPoints, listOfInnerIntersections);
                if (polygonsProduced.Count == 1)
                {
                    // Only one polygon was produced, add it as a new outer polygon
                    surface.AddPair(polygonsProduced[0], nonIntersectedInnerPolygons);
                }
                else if (polygonsProduced.Count > 1)
                {
                    // More than one polygon was produced
                    for (int i = 0; i < polygonsProduced.Count; i++)
                    {
                        // Find which nonIntersectedInnerPolygons belong to which new polygon
                        List<Polygon<T>> newInnerPolygons = new List<Polygon<T>>();

                        foreach (Polygon<T> innerPolygon in nonIntersectedInnerPolygons)
                        {
                            if (innerPolygon.Vertices == null || innerPolygon.Head == null) continue;

                            if (PointIsInsidePolygon(innerPolygon.Vertices[innerPolygon.Head.Data.Index], polygonsProduced[i]) == 1)
                            {
                                newInnerPolygons.Add(innerPolygon);
                            }
                        }

                        // Add the new polygon as an outer polygon a long with the list of inner polygons
                        surface.AddPair(polygonsProduced[i], newInnerPolygons);
                    }
                }
            }

            // Case 1
            if (groupCutterIsIn != null)
            {
                Polygon<U> booleanCutter = ConvertPolygonToBooleanList<T, U>(cutter);

                List<Polygon<T>> newInnerPolygonsList = new List<Polygon<T>>();
                List<Polygon<U>> innerPolygonsToCombineWith = new List<Polygon<U>>();
                List<IntersectionPoints<U>> innerIntersectionPoints = new List<IntersectionPoints<U>>();

                // Perform intersection test with cutter against all inner polygons of the group the cutter is in
                foreach (Polygon<T> innerPolygon in groupCutterIsIn.InnerPolygons)
                {
                    Polygon<U> booleanPolygon = PSD.ConvertPolygonToBooleanList<T, U>(innerPolygon);
                    IntersectionResult result = IntersectCutterAndPolygon(booleanCutter, booleanPolygon, out IntersectionPoints<U>? intersectionPoints);

                    if (result == IntersectionResult.CUTTER_IS_INSIDE)
                    {
                        // Cutter is completely inside an inner polygon,
                        // cutter will not produce any new polygons
                        return CutSurfaceResult.CUTTER_INSIDE_OF_INNER;
                    }

                    switch (result)
                    {
                        case IntersectionResult.INTERSECTS:
                            // Store intersection result
                            innerPolygonsToCombineWith.Add(booleanPolygon);
                            innerIntersectionPoints.Add(intersectionPoints);
                            break;
                        case IntersectionResult.BOTH_OUTSIDE:
                            // Keep the inner polygon
                            newInnerPolygonsList.Add(innerPolygon);
                            break;
                        default:
                            break;
                    }
                }

                if (innerIntersectionPoints.Count == 0)
                {
                    // No intersection was found, cutter is a new inner polygon
                    groupCutterIsIn.InnerPolygons.Add(cutter);
                    return CutSurfaceResult.CUTTER_INSIDE_OUTER_BUT_NOT_INNERS;
                }

                // Replace the list
                groupCutterIsIn.InnerPolygons = newInnerPolygonsList;

                // Perform polygon addition operation
                List<Polygon<T>> polygonsProduced = CombinePolygons<T, U>(booleanCutter, innerIntersectionPoints);
                if (polygonsProduced.Count == 1)
                {
                    // Only 1 polygon was produced
                    groupCutterIsIn.InnerPolygons.Add(polygonsProduced[0]);
                    return CutSurfaceResult.CUTTER_INSIDE_OUTER_OVERLAPPED_INNER;
                }
                else if (polygonsProduced.Count > 1)
                {
                    // NOTE: Some optimization can be done in the AddPolygons() function side

                    // Multiple polygons was produced
                    // Find the polygon that is on the outside
                    int outsidePolygonIndex = 0;
                    bool outsidePolygonFound = false;
                    while (!outsidePolygonFound)
                    {
                        outsidePolygonFound = true;
                        foreach (Polygon<T> polygon in polygonsProduced)
                        {
                            if (polygon == polygonsProduced[outsidePolygonIndex]) continue;

                            Polygon<T> polygonBeingObserved = polygonsProduced[outsidePolygonIndex];

                            if (PointIsInsidePolygon(polygonBeingObserved.Vertices[polygonBeingObserved.Head.Data.Index], polygon) != -1)
                            {
                                outsidePolygonFound = false;
                                break;
                            }
                        }

                        if (!outsidePolygonFound)
                        {
                            outsidePolygonIndex++;
                        }
                    }

                    groupCutterIsIn.InnerPolygons.Add(polygonsProduced[outsidePolygonIndex]);
                    return CutSurfaceResult.CUTTER_INSIDE_OUTER_OVERLAPPED_INNER_PRODUCE_MULTI;
                }
                else
                {
                    return CutSurfaceResult.UNKNOWN_ERROR;
                }
            }

            return CutSurfaceResult.OVERLAPPED_OUTER;
        }

        /// <summary>
        /// Triangulate a polygon surface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="surface">The surface to be triangulated</param>
        /// <param name="triangles">The list of triangles generated from triangulation</param>
        /// <param name="vertices">The list of vertices that make up the surface</param>
        public static void TriangulateSurface<T>(SurfaceShape<T> surface, out List<int>? triangles, out List<Vector2>? vertices) where T : PolygonVertex
        {
            if (surface.Polygons.Count <= 0)
            {
                triangles = null;
                vertices = null;
                return;
            }

            triangles = new List<int>();
            vertices = new List<Vector2>();

            // Triangulate each group
            foreach (PolygonGroup<T> group in surface.Polygons)
            {
                TriangulateGroup(group, triangles, vertices);
            }

            return;
        }

        //public static void CreateSideCapOfSurface<T>(SurfaceShape<T> surface, CoordinateConverter coordinateConverter, Vector3 frontNormal, float depth,
        //    List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs) where T : PolygonVertex
        //{
        //    frontNormal = frontNormal.Normalized();

        //    // For each group...
        //    foreach (PolygonGroup<T> group in surface.Polygons)
        //    {
        //        // Create side cap for each line segment of the outer polygon
        //        List<Vector2> outerVertices = group.OuterPolygon.Vertices;
        //        VertexNode<T> outerNow = group.OuterPolygon.Head;
        //        do
        //        {
        //            int verticesPreviousCount = vertices.Count;

        //            Vector3 a = coordinateConverter.ConvertTo3D(outerVertices[outerNow.Data.Index]);
        //            Vector3 b = coordinateConverter.ConvertTo3D(outerVertices[outerNow.Previous.Data.Index]);

        //            Vector3 aCopy = a - (frontNormal * depth);
        //            Vector3 bCopy = b - (frontNormal * depth);

        //            // Add these vertices to the list
        //            vertices.Add(a);
        //            vertices.Add(b);
        //            vertices.Add(bCopy);
        //            vertices.Add(aCopy);

        //            // Add the indices for the triangle
        //            indices.Add(verticesPreviousCount + 0);
        //            indices.Add(verticesPreviousCount + 1);
        //            indices.Add(verticesPreviousCount + 2);
        //            // The other triangle
        //            indices.Add(verticesPreviousCount + 2);
        //            indices.Add(verticesPreviousCount + 3);
        //            indices.Add(verticesPreviousCount + 0);

        //            // Calculate normal for the triangles and add it to the list of normals
        //            Vector3 triangleNormal = (bCopy - a).Cross(b - a).Normalized();
        //            normals.Add(triangleNormal);
        //            normals.Add(triangleNormal);
        //            normals.Add(triangleNormal);
        //            normals.Add(triangleNormal);

        //            // Add uvs...
        //            uvs.Add(Vector2.Zero);
        //            uvs.Add(Vector2.Zero);
        //            uvs.Add(Vector2.Zero);
        //            uvs.Add(Vector2.Zero);

        //            outerNow = outerNow.Previous; // Assuming the outer polygon is CW, we must do this CCW to get correct triangles
        //        } while (outerNow != group.OuterPolygon.Head);

        //        // Now for each inner polygons
        //        foreach (Polygon<T> innerPolygon in group.InnerPolygons)
        //        {
        //            List<Vector2> innerVertices = innerPolygon.Vertices;
        //            VertexNode<T> innerNow = innerPolygon.Head;
        //            do
        //            {
        //                int verticesPreviousCount = vertices.Count;

        //                Vector3 a = coordinateConverter.ConvertTo3D(innerVertices[innerNow.Data.Index]);
        //                Vector3 b = coordinateConverter.ConvertTo3D(innerVertices[innerNow.Previous.Data.Index]);

        //                Vector3 aCopy = a - (frontNormal * depth);
        //                Vector3 bCopy = b - (frontNormal * depth);

        //                // Add these vertices to the list
        //                vertices.Add(a);
        //                vertices.Add(b);
        //                vertices.Add(bCopy);
        //                vertices.Add(aCopy);

        //                // Add the indices for the triangle
        //                indices.Add(verticesPreviousCount + 0);
        //                indices.Add(verticesPreviousCount + 1);
        //                indices.Add(verticesPreviousCount + 2);
        //                // The other triangle
        //                indices.Add(verticesPreviousCount + 2);
        //                indices.Add(verticesPreviousCount + 3);
        //                indices.Add(verticesPreviousCount + 0);

        //                // Calculate normal for the triangles and add it to the list of normals
        //                Vector3 triangleNormal = (bCopy - a).Cross(b - a).Normalized();
        //                normals.Add(triangleNormal);
        //                normals.Add(triangleNormal);
        //                normals.Add(triangleNormal);
        //                normals.Add(triangleNormal);

        //                // Add uvs...
        //                uvs.Add(Vector2.Zero);
        //                uvs.Add(Vector2.Zero);
        //                uvs.Add(Vector2.Zero);
        //                uvs.Add(Vector2.Zero);

        //                innerNow = innerNow.Previous;
        //            } while (innerNow != innerPolygon.Head);
        //        }
        //    }
        //}

        public static List<List<Vector2>>? FindConvexVerticesOfSurface<T>(SurfaceShape<T> surface) where T : PolygonVertex
        {
            if (surface.Polygons.Count <= 0) return null;

            List<List<Vector2>> output = new List<List<Vector2>>();

            // Find convex vertices of each group
            foreach (PolygonGroup<T> group in surface.Polygons)
            {
                FindConvexVerticesOfGroup(group, output);
            }

            return output;
        }

        public static List<Vector2>? ComputeUVCoordinates(List<Vector2> originalVertices, List<Vector2> originalUVCoordinates, List<Vector2> currentVertices)
        {
            if (originalVertices.Count <= 0 || originalUVCoordinates.Count <= 0) return null;

            List<Vector2> output = new List<Vector2>();
            
            foreach (Vector2 vertex in currentVertices)
            {
                for (int i = 0; i < originalVertices.Count; i++)
                {
                    // Find which 'triangle the vertex belongs to'
                    Vector2 a = originalVertices[i];
                    Vector2 b = originalVertices[(i + 1) % originalVertices.Count];
                    Vector2 c = originalVertices[(i + 2) % originalVertices.Count];

                    if (BarycentricCoordinates(vertex, a, b, c, out float u, out float v, out float w))
                    {
                        Vector2 aUv = originalUVCoordinates[i];
                        Vector2 bUv = originalUVCoordinates[(i + 1) % originalVertices.Count];
                        Vector2 cUv = originalUVCoordinates[(i + 2) % originalVertices.Count];

                        Vector2 newUv = (aUv * w) + (bUv * u) + (cUv * v);
                        output.Add(newUv);

                        break;
                    }
                }
            }

            return output;
        }

        private static void PrintBooleanList<T>(Polygon<T> polygon)
            where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            if (polygon.Head == null) return;

            StringBuilder sb = new StringBuilder();
            VertexNode<T> now = polygon.Head;
            do
            {
                sb.Append(now.Data.Index);

                sb.Append(", Outside: ");
                sb.Append(now.Data.IsOutside);

                sb.Append(", Cross: ");
                if (now.Data.Cross != null) sb.Append(now.Data.Cross.Data.Index);
                else sb.Append("None");

                sb.Append("\n");

                now = now.Next;
            } while (now != polygon.Head);

            Console.Write(sb.ToString());
        }

        /// <summary>
        /// Perform a boolean addition operation using the given information
        /// </summary>
        /// <typeparam name="T">The polygon vertex type to be return</typeparam>
        /// <typeparam name="U">The polygon vertex that has properties needed to perform boolean operations</typeparam>
        /// <param name="center">The polygon where the operation will be performed around (should be the hole inserted)</param>
        /// <param name="intersections">List of intersection points produced from IntersectCutterAndPolygon</param>
        /// <returns>List of polygons produced from this operation</returns>
        private static List<Polygon<T>> CombinePolygons<T, U>(Polygon<U> center, List<IntersectionPoints<U>> intersections)
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            List<Polygon<T>> outputPolygons = new List<Polygon<T>>();

            if (center.Head == null || center.Count < 3 || intersections.Count == 0)
                return outputPolygons;

            // Function for determining if the given point is a crossing point
            bool CheckSpecialAdditionCase(VertexNode<U> node)
            {
                if (node.Data.Cross == null) return false;
                if (!node.Data.Cross.Next.Data.IsOutside) return false;
                if (node.Data.Cross.Next.Data.Cross == null) return true;
                var crossback = node.Data.Cross.Next.Data.Cross;
                if (crossback == node.Next) return false;
                return true;
            }

            // Insert Points
            InsertIntersectionPoints(center, intersections);

            // Below is used for printing boolean list for debugging purposes
            HashSet<Polygon<U>> booleanPolygons = new HashSet<Polygon<U>>();

            // INFINITE LOOP CAN OCCUR IN THIS SECTION OF CODE
            // WHY: I DO NOT KNOW, PROBABLY HAS TO DO WITH ONE OF THE EDGE CASES
            while (true)
            {
                VertexNode<U>? firstPoint = null, point = null;
                VertexNode<U> now = center.Head;
                do
                {
                    if (!now.Data.Processed && now.Data.IsOutside)
                    {
                        firstPoint = point = now;
                        break;
                    }
                    now = now.Next;
                } while (now != center.Head);

                if (point == null) break;

                Polygon<T> newPolygon = new Polygon<T>();
                newPolygon.Vertices = new List<Vector2>();

                do
                {
                    if (point.Owner.Vertices == null)
                    {
                        point = point.Next;
                        continue;
                    }

                    // DEBUG STUFF
                    if (point.Owner != center)
                    {
                        booleanPolygons.Add(point.Owner);
                    }

                    bool pointIsCrossingPoint = CheckSpecialAdditionCase(point);

                    int insertionIndex = newPolygon.Vertices.Count;
                    newPolygon.Vertices.Add(point.Owner.Vertices[point.Data.Index]);
                    newPolygon.InsertVertexAtBack(insertionIndex);

                    point.Data.Processed = true;

                    if (pointIsCrossingPoint)
                    {
                        point.Data.Cross.Data.Processed = true;
                        point = point.Data.Cross;
                    }

                    point = point.Next;
                } while (point != firstPoint);

                outputPolygons.Add(newPolygon);
            }

            // Print Boolean List for Debugging
            Console.WriteLine("\nFrom Within the Polygon Addition Version CombinePolygons()");
            Console.WriteLine("Cutter:");
            PrintBooleanList(center);
            int innerCount = 0;
            foreach (Polygon<U> polygon in booleanPolygons)
            {
                Console.WriteLine(string.Format("Inner Polygon {0}:", innerCount));
                PrintBooleanList(polygon);
                innerCount++;
            }

            return outputPolygons;
        }

        /// <summary>
        /// Perform a boolean mixed addition-subtraction operation using the given information
        /// </summary>
        /// <typeparam name="T">The polygon vertex type to be return</typeparam>
        /// <typeparam name="U">The polygon vertex that has properties needed to perform boolean operations</typeparam>
        /// <param name="center">The polygon where the operation will be performed around (should be the hole inserted)</param>
        /// <param name="outerIntersections">The intersecion points for the outer polygon produced from IntersectCutterAndPolygon</param>
        /// <param name="innerIntersections">List of intersection points for inner polygons produced from IntersectCutterAndPolygon</param>
        /// <returns>List of polygons produced from this operation</returns>
        private static List<Polygon<T>> CombinePolygons<T, U>(Polygon<U> center, IntersectionPoints<U> outerIntersections, List<IntersectionPoints<U>> innerIntersections)
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            List<Polygon<T>> outputPolygons = new List<Polygon<T>>();

            if (center.Head == null || center.Count < 3 || outerIntersections.Polygon.Head == null)
                return outputPolygons;

            Polygon<U> outerPolygon = outerIntersections.Polygon;

            // Function for determining if the given node is a crossing point
            // TO DO FIX THE LOGIC AROUND THE CENTER POLYGON HAVING INCORRECT IsOutside FLAG
            bool CheckSpecialSubtractionCase(VertexNode<U> node)
            {
                if (node.Data.Cross == null) return false;

                var crossback = node.Data.Cross.Next.Data.Cross;

                // Traversing the outer polygon, thus the cross should always be the center, but the crossback is unknown, could either be outer or another polygon
                if (node.Owner == outerPolygon)
                {
                    if (node.Data.Cross.Next.Data.IsOutside) return false;
                }

                // From this point, we are either traversing the center or any other inner polygon

                // Traversing the center, cross can either be outer or inner from here
                if (node.Owner == center)
                {
                    if (!node.Data.Cross.Next.Data.IsOutside) return false;
                }
                // Traversing an inner here
                else
                {
                    if (node.Data.Cross.Next.Data.IsOutside) return false;
                }

                if (crossback == null) return true;
                if (crossback == node.Next) return false;
                return true;
            }

            InsertIntersectionPoints(center, outerIntersections, innerIntersections);

            // Below is used for printing boolean list for debugging purposes
            HashSet<Polygon<U>> booleanPolygons = new HashSet<Polygon<U>>();

            while (true)
            {
                VertexNode<U>? firstPoint = null, point = null;
                VertexNode<U> now = outerPolygon.Head;
                do
                {
                    if (!now.Data.Processed && now.Data.IsOutside)
                    {
                        firstPoint = point = now;
                        break;
                    }
                    now = now.Next;
                } while (now != outerPolygon.Head);

                if (point == null) break;

                Polygon<T> newPolygon = new Polygon<T>();
                newPolygon.Vertices = new List<Vector2>();

                do
                {
                    if (point.Owner.Vertices == null)
                    {
                        point = point.Next;
                        continue;
                    }

                    // DEBUG STUFF
                    if (point.Owner != center && point.Owner != outerPolygon)
                    {
                        booleanPolygons.Add(point.Owner);
                    }

                    bool pointIsCrossingPoint = CheckSpecialSubtractionCase(point);

                    int insertionIndex = newPolygon.Vertices.Count;
                    newPolygon.Vertices.Add(point.Owner.Vertices[point.Data.Index]);
                    newPolygon.InsertVertexAtBack(insertionIndex);

                    point.Data.Processed = true;

                    if (pointIsCrossingPoint)
                    {
                        point.Data.Cross.Data.Processed = true;
                        point = point.Data.Cross;
                    }

                    point = point.Next;
                } while (point != firstPoint && newPolygon.Vertices.Count < 1000);

                outputPolygons.Add(newPolygon);
            }

            // Print Boolean List for Debugging
            Console.WriteLine("\nFrom Within the Polygon Mixed Addition-Subtraction Version CombinePolygons()");
            Console.WriteLine("Cutter:");
            PrintBooleanList(center);
            Console.WriteLine("Outer Polygon:");
            PrintBooleanList(outerPolygon);
            int innerCount = 0;
            foreach (Polygon<U> polygon in booleanPolygons)
            {
                Console.WriteLine(string.Format("Inner Polygon {0}:", innerCount));
                PrintBooleanList(polygon);
                innerCount++;
            }

            return outputPolygons;
        }

        /// <summary>
        /// Intersect a cutter polygon with another polygon and returns the findings between the two
        /// </summary>
        /// <typeparam name="T">A vertex class that has boolean vertex properties to represent each vertex of a polygon and store additional results from the intersection test</typeparam>
        /// <param name="cutter">The cutter polygon</param>
        /// <param name="polygon">The other polygon</param>
        /// <param name="intersectionPoints">List of intersection points to return</param>
        /// <returns>The result from intersecting the two given polygons</returns>
        private static IntersectionResult IntersectCutterAndPolygon<T>(Polygon<T> cutter, Polygon<T> polygon, out IntersectionPoints<T>? intersectionPoints) where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            // Intersection cannot performed, return for invalid operation
            if (cutter.Count < 3 || cutter.Head == null || cutter.Vertices == null ||
                polygon.Count < 3 || polygon.Head == null || polygon.Vertices == null)
            {
                intersectionPoints = null;
                return IntersectionResult.FAILED;
            }

            // Get list of vertices
            List<Vector2> polygonVertices = polygon.Vertices;
            List<Vector2> cutterVertices = cutter.Vertices;

            // Remember the original bool value for these
            // Needed for determining if cutter is outside of the polygon or vice-versa
            bool originalPolygonOutsideFlag = polygon.Head.Data.IsOutside;
            bool originalCutterOutsideFlag = cutter.Head.Data.IsOutside;

            #region IntersectionTest

            // List of intersection point, polygon vertex involved, t value, cutter vertex involved, and u value
            List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> intersections = new List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>>();

            VertexNode<T> polygonNow = polygon.Head;
            do
            {
                Vector2 a0 = polygonVertices[polygonNow.Data.Index];
                Vector2 a1 = polygonVertices[polygonNow.Next.Data.Index];

                VertexNode<T> cutterNow = cutter.Head;
                do
                {
                    Vector2 b0 = cutterVertices[cutterNow.Data.Index];
                    Vector2 b1 = cutterVertices[cutterNow.Next.Data.Index];

                    // Perform a line calculation
                    int result = PartialLineIntersection(a0, a1, b0, b1, out float t, out float u);

                    // Both line segments are non-colinear, thus t and u can be used to determine if they intersect
                    if (result == 1)
                    {
                        #region EdgeCase
                        // ----------------------------------------------------------------------------
                        // This is extremely unlikely due to floating point precision error,
                        // but just in case...
                        bool a0IsOnInfiniteRay = IsNearlyEqual(t, 0.0f); // a0 is intersecting with the cutter's infinite ray
                        bool a1IsOnInfiniteRay = IsNearlyEqual(t, 1.0f);
                        bool b0IsOnInfiniteRay = IsNearlyEqual(u, 0.0f);
                        bool b1IsOnInfiniteRay = IsNearlyEqual(u, 1.0f);

                        // Check if a0 or a1 is on an edge
                        if (u >= 0.0f && u <= 1.0f)
                        {
                            // a0 is the intersection point...
                            if (a0IsOnInfiniteRay)
                            {
                                polygonNow.Data.IsOutside = false;
                                polygonNow.Data.OnEdge = true;
                            }
                            // a1 is the intersection point...
                            else if (a1IsOnInfiniteRay)
                            {
                                polygonNow.Next.Data.IsOutside = false;
                                polygonNow.Next.Data.OnEdge = true;
                            }
                        }

                        // Check if b0 or b1 is on an edge
                        if (t >= 0.0f && t <= 1.0f)
                        {
                            // b0 is the intersection point...
                            if (b0IsOnInfiniteRay)
                            {
                                cutterNow.Data.IsOutside = false;
                                cutterNow.Data.OnEdge = true;
                            }
                            // b0 is the intersection point...
                            else if (b1IsOnInfiniteRay)
                            {
                                cutterNow.Next.Data.IsOutside = false;
                                cutterNow.Next.Data.OnEdge = true;
                            }
                        }

                        // Modify polygonBoolVertex.Data.IsOutside value
                        if (polygonNow.Data.OnEdge == false)
                        {
                            bool polygonIntersectsAPoint = b0IsOnInfiniteRay || b1IsOnInfiniteRay;
                            float polygonCrossProductToCuttersLine = 0.0f;
                            // Line intersection occured at cutter's point b0
                            if (b0IsOnInfiniteRay) polygonCrossProductToCuttersLine = CrossProduct2D(b0 - a0, b1 - a0);
                            // Line intersection occured at cutter's point b1
                            else if (b1IsOnInfiniteRay) polygonCrossProductToCuttersLine = CrossProduct2D(b1 - a0, b0 - a0);

                            if (polygonIntersectsAPoint && polygonCrossProductToCuttersLine > 0.0f)
                            {
                                // Handle edge case where infinite ray of targetNow passes end points b0 and b1,
                                // by ignoring if the other vertex is CCW or CW (does not matter which) or colinear to the first vertex
                                // and changing isOutside if not ignored
                                polygonNow.Data.IsOutside = !polygonNow.Data.IsOutside;
                            }

                            if (polygonIntersectsAPoint == false && t >= 0.0f && u >= 0.0f && u <= 1.0f)
                            {
                                // infinite ray from polygon's a0 intersects cutter's line segment
                                polygonNow.Data.IsOutside = !polygonNow.Data.IsOutside;
                            }
                        }

                        // Modify cutterBoolVertex.Data.IsOutside value
                        if (cutterNow.Data.OnEdge == false)
                        {
                            bool cutterIntersectsAPoint = a0IsOnInfiniteRay || a1IsOnInfiniteRay;
                            float cutterCrossProductToPolygonLine = 0.0f;
                            // Line intersection occured at polygon's a0
                            if (a0IsOnInfiniteRay) cutterCrossProductToPolygonLine = CrossProduct2D(a0 - b0, a1 - b0);
                            // Line intersection occured at polygon's a1
                            else if (a1IsOnInfiniteRay) cutterCrossProductToPolygonLine = CrossProduct2D(a1 - b0, a0 - b0);

                            if (cutterIntersectsAPoint && cutterCrossProductToPolygonLine > 0.0f)
                            {
                                cutterNow.Data.IsOutside = !cutterNow.Data.IsOutside;
                            }

                            // infinite ray from cutter's b0 intersects cutter's line segment
                            if (cutterIntersectsAPoint == false && u >= 0.0f && t >= 0.0f && t <= 1.0f)
                            {
                                cutterNow.Data.IsOutside = !cutterNow.Data.IsOutside;
                            }
                        }

                        // ----------------------------------------------------------------------------
                        #endregion

                        if (t >= 0.0f && t <= 1.0f && u >= 0.0f && u <= 1.0f)
                        {
                            Vector2 intersectionPoint = new Vector2(a0.X + t * (a1.X - a0.X), a0.Y + t * (a1.Y - a0.Y));
                            intersections.Add(
                                new Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>(intersectionPoint, polygonNow, t, cutterNow, u)
                                );
                        }

                    }
                    // Both line segments are colinear, (This is unlikely due to floating point error, just in case though...)
                    else if (result == 0)
                    {
                        // polygonNow is on the edge of cutter's line segment
                        if (t >= 0.0f && t <= 1.0f)
                        {
                            polygonNow.Data.IsOutside = false;
                            polygonNow.Data.OnEdge = true;
                        }
                        // cutterNow is on the edge of target's line segment
                        if (u >= 0.0f && u <= 0.0f)
                        {
                            cutterNow.Data.IsOutside = false;
                            cutterNow.Data.OnEdge = true;
                        }
                    }

                    cutterNow = cutterNow.Next;
                } while (cutterNow != cutter.Head);

                polygonNow = polygonNow.Next;
            } while (polygonNow != polygon.Head);
            #endregion

            if (intersections.Count == 0)
            {
                // No intersection between the two given polygons, thus end function

                // Determine which polygon is outside, or if both are outside each other, and then return the result.
                bool cutterIsOutsidePolygon = originalCutterOutsideFlag ? cutter.Head.Data.IsOutside : !cutter.Head.Data.IsOutside;
                bool polygonIsOutsidePolygon = originalPolygonOutsideFlag ? polygon.Head.Data.IsOutside : !polygon.Head.Data.IsOutside;

                intersectionPoints = null;
                if (polygonIsOutsidePolygon && cutterIsOutsidePolygon) return IntersectionResult.BOTH_OUTSIDE;
                return cutterIsOutsidePolygon ? IntersectionResult.POLYGON_IS_INSIDE : IntersectionResult.CUTTER_IS_INSIDE;
            }

            intersectionPoints = new IntersectionPoints<T>(polygon, intersections);
            return IntersectionResult.INTERSECTS;
        }

        /// <summary>
        /// Helper function for inserting intersection points for an addition boolean operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cutter"></param>
        /// <param name="allIntersections"></param>
        private static void InsertIntersectionPoints<T>(Polygon<T> cutter, List<IntersectionPoints<T>> allIntersections) where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            if (allIntersections.Count == 0 || cutter.Vertices == null) return;

            List<VertexNode<T>> insertedCutterIntersection = new List<VertexNode<T>>();

            foreach (IntersectionPoints<T> intersectionResults in allIntersections)
            {
                foreach (var result in intersectionResults.Intersections)
                {
                    Vector2 intersectionPoint = result.Item1;
                    VertexNode<T> polygonNode = result.Item2;
                    float t = result.Item3;
                    VertexNode<T> cutterNode = result.Item4;
                    float u = result.Item5;

                    if (polygonNode.Owner.Vertices == null || cutterNode.Owner.Vertices == null) continue;

                    List<Vector2> polygonVertices = polygonNode.Owner.Vertices;
                    List<Vector2> cutterVertices = cutterNode.Owner.Vertices;

                    VertexNode<T> nodeAddedToPolygon;
                    VertexNode<T> nodeAddedToCutter;

                    // The two if statements will in 99.9% of all cases never happen...
                    // but just in case that 0.1% case does happen...
                    if (IsNearlyEqual(t, 0.0f)) nodeAddedToPolygon = polygonNode;
                    else if (IsNearlyEqual(t, 1.0f)) nodeAddedToPolygon = polygonNode.Next;
                    else
                    {
                        // Make sure to insert the intersectionPoint and the correct location
                        // if the next vertex happens to be another intersectionPoint
                        while (polygonNode.Next.Data.Cross != null && polygonNode.Next.Data.IsAnAddedVertex &&
                            SegmentLengthSquared(polygonVertices[polygonNode.Data.Index], intersectionPoint) >
                            SegmentLengthSquared(polygonVertices[polygonNode.Data.Index], polygonVertices[polygonNode.Next.Data.Index]))
                        {
                            polygonNode = polygonNode.Next;
                        }
                        // Do the actual insertions
                        // NOTE: THIS FAILS TO HANDLE THE EDGE CASE WHERE VERTEX INTERSECTS THE EDGE
                        int insertedVertexLocation = polygonVertices.Count;
                        polygonVertices.Add(intersectionPoint);
                        nodeAddedToPolygon = polygonNode.Owner.InsertVertexAfter(polygonNode, insertedVertexLocation);
                        nodeAddedToPolygon.Data.IsAnAddedVertex = true;
                    }

                    // Do the same thing as above, but with cutter this time
                    if (IsNearlyEqual(u, 0.0f)) nodeAddedToCutter = cutterNode;
                    else if (IsNearlyEqual(u, 1.0f)) nodeAddedToCutter = cutterNode.Next;
                    else
                    {
                        while (cutterNode.Next.Data.Cross != null && cutterNode.Next.Data.IsAnAddedVertex &&
                            SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], intersectionPoint) >
                            SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], cutterVertices[cutterNode.Next.Data.Index]))
                        {
                            cutterNode = cutterNode.Next;
                        }
                        int insertedVertexLocation = cutterVertices.Count;
                        cutterVertices.Add(intersectionPoint);
                        nodeAddedToCutter = cutterNode.Owner.InsertVertexAfter(cutterNode, insertedVertexLocation);
                        nodeAddedToCutter.Data.IsAnAddedVertex = true;
                    }

                    nodeAddedToPolygon.Data.Cross = nodeAddedToCutter;
                    nodeAddedToCutter.Data.Cross = nodeAddedToPolygon;

                    nodeAddedToPolygon.Data.IsOutside = false;
                    nodeAddedToPolygon.Data.OnEdge = true;

                    nodeAddedToCutter.Data.IsOutside = false;
                    nodeAddedToCutter.Data.OnEdge = true;

                    insertedCutterIntersection.Add(nodeAddedToCutter);
                }
            }

            // Add additional vertices, needed for boolean-subtraction operations
            foreach (VertexNode<T> node in insertedCutterIntersection)
            {
                if (node.Next.Data.IsOutside) continue;

                Vector2 extraInsertionPoint = new Vector2(
                    (cutter.Vertices[node.Next.Data.Index].X - cutter.Vertices[node.Data.Index].X) / 2.0f + cutter.Vertices[node.Data.Index].X,
                    (cutter.Vertices[node.Next.Data.Index].Y - cutter.Vertices[node.Data.Index].Y) / 2.0f + cutter.Vertices[node.Data.Index].Y
                );

                bool insideAnotherPoly = false;
                foreach (IntersectionPoints<T> intersectionResults in allIntersections)
                {
                    if (PointIsInsidePolygon(extraInsertionPoint, intersectionResults.Polygon) != -1)
                    {
                        insideAnotherPoly = true;
                        break;
                    }
                }

                if (!insideAnotherPoly)
                {
                    int insertedVertexLocation = cutter.Vertices.Count;
                    cutter.Vertices.Add(extraInsertionPoint);
                    VertexNode<T> addedNode = cutter.InsertVertexAfter(node, insertedVertexLocation);
                    addedNode.Data.IsAnAddedVertex = true;
                }
            }
        }

        /// <summary>
        /// Helper function for inserting intersection points need for a mixed addition-subtraction boolean operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cutter"></param>
        /// <param name="outerIntersections"></param>
        /// <param name="innerIntersections"></param>
        private static void InsertIntersectionPoints<T>(Polygon<T> cutter, IntersectionPoints<T> outerIntersections, List<IntersectionPoints<T>> innerIntersections) where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            if (cutter.Vertices == null || outerIntersections.Polygon.Vertices == null) return;

            List<VertexNode<T>> insertedOuterIntersection = new List<VertexNode<T>>();       
            List<Vector2> cutterVertices = cutter.Vertices;
            List<Vector2> outerVertices = outerIntersections.Polygon.Vertices;

            // Insert for the outer polygon
            foreach (var result in outerIntersections.Intersections)
            {
                Vector2 intersectionPoint = result.Item1;
                VertexNode<T> polygonNode = result.Item2;
                float t = result.Item3;
                VertexNode<T> cutterNode = result.Item4;
                float u = result.Item5;

                VertexNode<T> nodeAddedToPolygon;
                VertexNode<T> nodeAddedToCutter;

                // The two if statements will in 99.9% of all cases never happen...
                // but just in case that 0.1% case does happen...
                if (IsNearlyEqual(t, 0.0f)) nodeAddedToPolygon = polygonNode;
                else if (IsNearlyEqual(t, 1.0f)) nodeAddedToPolygon = polygonNode.Next;
                else
                {
                    // Make sure to insert the intersectionPoint and the correct location
                    // if the next vertex happens to be another intersectionPoint
                    float distanceFromNodeToIntersection = SegmentLengthSquared(outerVertices[polygonNode.Data.Index], intersectionPoint);
                    float distanceFromNodeToNext = SegmentLengthSquared(outerVertices[polygonNode.Data.Index], outerVertices[polygonNode.Next.Data.Index]);
                    while (polygonNode.Next.Data.Cross != null && polygonNode.Next.Data.IsAnAddedVertex &&
                        distanceFromNodeToIntersection > distanceFromNodeToNext)
                    {
                        polygonNode = polygonNode.Next;
                        distanceFromNodeToIntersection = SegmentLengthSquared(outerVertices[polygonNode.Data.Index], intersectionPoint);
                        distanceFromNodeToNext = SegmentLengthSquared(outerVertices[polygonNode.Data.Index], outerVertices[polygonNode.Next.Data.Index]);
                    }

                    // Do the actual insertions
                    if (!IsNearlyEqual(distanceFromNodeToIntersection, distanceFromNodeToNext))
                    {
                        int insertedVertexLocation = outerVertices.Count;
                        outerVertices.Add(intersectionPoint);
                        nodeAddedToPolygon = polygonNode.Owner.InsertVertexAfter(polygonNode, insertedVertexLocation);
                        nodeAddedToPolygon.Data.IsAnAddedVertex = true;
                    }
                    else
                    {
                        nodeAddedToPolygon = polygonNode.Next;
                        Console.WriteLine("Outer Edge case detected 1");
                    }
                }

                // Do the same thing as above, but with cutter this time
                if (IsNearlyEqual(u, 0.0f)) nodeAddedToCutter = cutterNode;
                else if (IsNearlyEqual(u, 1.0f)) nodeAddedToCutter = cutterNode.Next;
                else
                {
                    float distanceFromNodeToIntersection = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], intersectionPoint);
                    float distanceFromNodeToNext = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], cutterVertices[cutterNode.Next.Data.Index]);
                    while (cutterNode.Next.Data.Cross != null && cutterNode.Next.Data.IsAnAddedVertex &&
                        distanceFromNodeToIntersection > distanceFromNodeToNext)
                    {
                        cutterNode = cutterNode.Next;
                        distanceFromNodeToIntersection = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], intersectionPoint);
                        distanceFromNodeToNext = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], cutterVertices[cutterNode.Next.Data.Index]);
                    }

                    if (!IsNearlyEqual(distanceFromNodeToIntersection, distanceFromNodeToNext))
                    {
                        int insertedVertexLocation = cutterVertices.Count;
                        cutterVertices.Add(intersectionPoint);
                        nodeAddedToCutter = cutterNode.Owner.InsertVertexAfter(cutterNode, insertedVertexLocation);
                        nodeAddedToCutter.Data.IsAnAddedVertex = true;
                    }
                    else
                    {
                        nodeAddedToCutter = cutterNode.Next;
                        Console.WriteLine("Outer Edge case detected 2");
                    }
                }

                nodeAddedToPolygon.Data.Cross = nodeAddedToCutter;
                nodeAddedToCutter.Data.Cross = nodeAddedToPolygon;

                nodeAddedToPolygon.Data.IsOutside = false;
                nodeAddedToPolygon.Data.OnEdge = true;

                nodeAddedToCutter.Data.IsOutside = false;
                nodeAddedToCutter.Data.OnEdge = true;

                insertedOuterIntersection.Add(nodeAddedToPolygon);
            }

            // Insert for the inner polygons
            foreach (IntersectionPoints<T> intersectionResults in innerIntersections)
            {
                foreach (var result in intersectionResults.Intersections)
                {
                    Vector2 intersectionPoint = result.Item1;
                    VertexNode<T> polygonNode = result.Item2;
                    float t = result.Item3;
                    VertexNode<T> cutterNode = result.Item4;
                    float u = result.Item5;

                    if (polygonNode.Owner.Vertices == null) continue;

                    List<Vector2> polygonVertices = polygonNode.Owner.Vertices;

                    VertexNode<T> nodeAddedToPolygon;
                    VertexNode<T> nodeAddedToCutter;

                    // The two if statements will in 99.9% of all cases never happen...
                    // but just in case that 0.1% case does happen...
                    if (IsNearlyEqual(t, 0.0f)) nodeAddedToPolygon = polygonNode;
                    else if (IsNearlyEqual(t, 1.0f)) nodeAddedToPolygon = polygonNode.Next;
                    else
                    {
                        // Make sure to insert the intersectionPoint and the correct location
                        // if the next vertex happens to be another intersectionPoint
                        float distanceFromNodeToIntersection = SegmentLengthSquared(polygonVertices[polygonNode.Data.Index], intersectionPoint);
                        float distanceFromNodeToNext = SegmentLengthSquared(polygonVertices[polygonNode.Data.Index], polygonVertices[polygonNode.Next.Data.Index]);
                        while (polygonNode.Next.Data.Cross != null && polygonNode.Next.Data.IsAnAddedVertex &&
                            distanceFromNodeToIntersection > distanceFromNodeToNext)
                        {
                            polygonNode = polygonNode.Next;
                        }

                        // Do the actual insertions
                        if (!IsNearlyEqual(distanceFromNodeToIntersection, distanceFromNodeToNext))
                        {
                            int insertedVertexLocation = polygonVertices.Count;
                            polygonVertices.Add(intersectionPoint);
                            nodeAddedToPolygon = polygonNode.Owner.InsertVertexAfter(polygonNode, insertedVertexLocation);
                            nodeAddedToPolygon.Data.IsAnAddedVertex = true;                
                        }
                        else
                        {
                            nodeAddedToPolygon = polygonNode.Next;
                        }
                    }

                    // Do the same thing as above, but with cutter this time
                    if (IsNearlyEqual(u, 0.0f)) nodeAddedToCutter = cutterNode;
                    else if (IsNearlyEqual(u, 1.0f)) nodeAddedToCutter = cutterNode.Next;
                    else
                    {
                        float distanceFromNodeToIntersection = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], intersectionPoint);
                        float distanceFromNodeToNext = SegmentLengthSquared(cutterVertices[cutterNode.Data.Index], cutterVertices[cutterNode.Next.Data.Index]);
                        while (cutterNode.Next.Data.Cross != null && cutterNode.Next.Data.IsAnAddedVertex &&
                            distanceFromNodeToIntersection > distanceFromNodeToNext)
                        {
                            cutterNode = cutterNode.Next;
                        }

                        if (!IsNearlyEqual(distanceFromNodeToIntersection, distanceFromNodeToNext))
                        {
                            int insertedVertexLocation = cutterVertices.Count;
                            cutterVertices.Add(intersectionPoint);
                            nodeAddedToCutter = cutterNode.Owner.InsertVertexAfter(cutterNode, insertedVertexLocation);
                            nodeAddedToCutter.Data.IsAnAddedVertex = true;
                        }
                        else
                        {
                            nodeAddedToCutter = cutterNode.Next;
                        }
                    }

                    nodeAddedToPolygon.Data.Cross = nodeAddedToCutter;
                    nodeAddedToCutter.Data.Cross = nodeAddedToPolygon;

                    nodeAddedToPolygon.Data.IsOutside = false;
                    nodeAddedToPolygon.Data.OnEdge = true;

                    nodeAddedToCutter.Data.IsOutside = false;
                    nodeAddedToCutter.Data.OnEdge = true;
                }
            }

            // Add additional vertices, needed for boolean-subtraction operations
            foreach (VertexNode<T> node in insertedOuterIntersection)
            {
                if (node.Next.Data.IsOutside) continue;

                Vector2 extraInsertionPoint = new Vector2(
                    (outerVertices[node.Next.Data.Index].X - outerVertices[node.Data.Index].X) / 2.0f + outerVertices[node.Data.Index].X,
                    (outerVertices[node.Next.Data.Index].Y - outerVertices[node.Data.Index].Y) / 2.0f + outerVertices[node.Data.Index].Y
                );

                if (PointIsInsidePolygon(extraInsertionPoint, cutter) != -1) continue;
                
                int insertedVertexLocation = outerVertices.Count;
                outerVertices.Add(extraInsertionPoint);
                VertexNode<T> addedNode = outerIntersections.Polygon.InsertVertexAfter(node, insertedVertexLocation);
                addedNode.Data.IsAnAddedVertex = true; 
            }
        }

        private static Polygon<T>? CombineGroupIntoOne<T>(PolygonGroup<T> group) where T : PolygonVertex
        {
            Polygon<T> currentPolygon;
            List<Vector2>? currentVertices;
            // If there is any inner polygons, combined with the outer polygon
            if (group.InnerPolygons.Count > 0)
            {
                // Sort inner polygons from right-most to least right-most
                group.InnerPolygons.Sort((polygonA, polygonB) => -polygonA.Vertices[polygonA.RightMostVertex.Data.Index].X.CompareTo(polygonB.Vertices[polygonB.RightMostVertex.Data.Index].X));

                currentPolygon = group.OuterPolygon;
                currentVertices = new List<Vector2>(group.OuterPolygon.Vertices);
                // Connect the outer polygon with the inner polygons
                foreach (Polygon<T> innerPolygon in group.InnerPolygons)
                {
                    Polygon<T>? result = ConnectOuterAndInnerPolygon(currentPolygon, innerPolygon, currentVertices);
                    if (result != null)
                    {
                        currentPolygon = result;
                        // No need to do below as it should be done already in ConnectOuterAndInnerPolygon
                        // currentPolygon.Vertices = currentVertices;
                    }
                }
            }
            else
            {
                // Otherwise return null as there is no need 'combine' when there is not inner polygons 
                return null;
            }

            return currentPolygon;
        }

        /// <summary>
        /// Triangulate a group outer polygon and its inner polygons
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group">The group of polygons to triangulate</param>
        /// <param name="triangles">The list of triangles to add on to</param>
        /// <param name="vertices">The list of vertices to add on to</param>
        private static void TriangulateGroup<T>(PolygonGroup<T> group, List<int> triangles, List<Vector2> vertices) where T : PolygonVertex
        {
            if (group.OuterPolygon.Head == null || group.OuterPolygon.Count < 3 || group.OuterPolygon.Vertices == null)
            {
                return;
            }

            // Probably an unnecessary check here
            foreach (Polygon<T> innerPolygon in group.InnerPolygons)
            {
                if (innerPolygon.Head == null || innerPolygon.Count < 3 || innerPolygon.Vertices == null)
                {
                    return;
                }
            }

            // Combine the given group into one polygon
            Polygon<T>? currentPolygon = CombineGroupIntoOne(group);
            List<Vector2> currentVertices;
            if (currentPolygon == null)
            {
                // CombineGroupIntoOne failed probably because lack of inner polygons
                // Make a copy
                currentPolygon = new Polygon<T>(group.OuterPolygon);
                // No need to make a copy of currentPolygon.Vertices as it'll will be copied later
            }
            currentVertices = currentPolygon.Vertices;

            // Identify the eartips of currentPolygon
            List<VertexNode<T>>? earTips = FindEarTips(currentPolygon);
            // No ear tips was found, thus triangulation is not possible
            if (earTips == null)
            {
                return;
            }

            // Add on to the given list of vertices and triangles
            int verticesListOffset = vertices.Count;
            vertices.AddRange(currentVertices);

            while (earTips.Count > 0)
            {
                // Get the ear to clip
                VertexNode<T> earToClip = earTips[0];

                // Add the triangles
                triangles.Add(earToClip.Previous.Data.Index + verticesListOffset);
                triangles.Add(earToClip.Data.Index + verticesListOffset);
                triangles.Add(earToClip.Next.Data.Index + verticesListOffset);

                // Clip the ear and remove from the list
                currentPolygon.ClipVertex(earToClip);
                earTips.Remove(earToClip);

                // The neighbors may no longer be ears remove them
                earTips.Remove(earToClip.Previous);
                earTips.Remove(earToClip.Next);

                // Reprocess the neighbors
                if (IsAnEarTip(earToClip.Previous))
                    earTips.Add(earToClip.Previous);
                if (IsAnEarTip(earToClip.Next))
                    earTips.Add(earToClip.Next);
            }
        }

        private static void FindConvexVerticesOfGroup<T>(PolygonGroup<T> group, List<List<Vector2>> convexVerticesGroups) where T : PolygonVertex
        {
            if (group.OuterPolygon.Head == null || group.OuterPolygon.Count < 3 || group.OuterPolygon.Vertices == null)
            {
                return;
            }

            // Probably an unnecessary check here
            foreach (Polygon<T> innerPolygon in group.InnerPolygons)
            {
                if (innerPolygon.Head == null || innerPolygon.Count < 3 || innerPolygon.Vertices == null)
                {
                    return;
                }
            }

            // Combine the given group into one polygon
            Polygon<T>? currentPolygon = CombineGroupIntoOne(group);
            List<Vector2> currentVertices;
            if (currentPolygon == null)
            {
                // CombineGroupIntoOne failed probably because lack of inner polygons
                // Make a copy
                currentPolygon = new Polygon<T>(group.OuterPolygon);
                currentVertices = new List<Vector2>(currentPolygon.Vertices);
            }
            else
                currentVertices = currentPolygon.Vertices;

            // Identify the eartips of currentPolygon
            List<VertexNode<T>>? earTips = FindEarTips(currentPolygon, true);
            // No ear tips was found, thus no convex group of vertices exist
            if (earTips == null)
            {
                return;
            }

            while (earTips.Count > 0 && currentPolygon.Count > 2)
            {
                List<Vector2> convexVertices = new List<Vector2>();

                // Get the ear to clip
                VertexNode<T> earToClip = earTips[0];

                // Add the vertices
                convexVertices.Add(currentVertices[earToClip.Previous.Data.Index]);
                convexVertices.Add(currentVertices[earToClip.Data.Index]);
                convexVertices.Add(currentVertices[earToClip.Next.Data.Index]);

                // Clip the ear and remove from the list
                currentPolygon.ClipVertex(earToClip);
                earTips.Remove(earToClip);

                // Keep track if it is next and previous is still an eartip
                bool nextIsStillAnEarTip = earTips.Contains(earToClip.Next);
                bool previousIsStillAnEarTip = earTips.Contains(earToClip.Previous);

                // The neighbors may no longer be ears remove them
                earTips.Remove(earToClip.Previous);
                earTips.Remove(earToClip.Next);

                // Reprocess the neighbors
                if (IsAnEarTip(earToClip.Previous, true))
                    earTips.Add(earToClip.Previous);
                if (IsAnEarTip(earToClip.Next, true))
                    earTips.Add(earToClip.Next);

                // The values may become false now due to reprocessing
                nextIsStillAnEarTip = nextIsStillAnEarTip && earTips.Contains(earToClip.Next);
                previousIsStillAnEarTip = previousIsStillAnEarTip && earTips.Contains(earToClip.Previous);

                VertexNode<T> next = earToClip.Next;
                while(nextIsStillAnEarTip && earTips.Count > 0 && currentPolygon.Count > 2)
                {
                    // Add next.Next's vertex to the list
                    convexVertices.Add(currentVertices[next.Next.Data.Index]);

                    // Clip and remove from the list
                    currentPolygon.ClipVertex(next);
                    earTips.Remove(next);

                    // Keep track if next is still an ear tip
                    nextIsStillAnEarTip = earTips.Contains(next.Next);

                    // next.Next may no longer be an ear, remove and reprocess
                    earTips.Remove(next.Next);
                    if (IsAnEarTip(next.Next, true))
                        earTips.Add(next.Next);

                    // The value may become false now due to reprocessing
                    nextIsStillAnEarTip = nextIsStillAnEarTip && earTips.Contains(next.Next);

                    if (next.Next == earToClip.Previous)
                        // Update previous if next.Next happens to be earToClip.Previous
                        previousIsStillAnEarTip = nextIsStillAnEarTip;

                    next = next.Next;
                }

                VertexNode<T> prev = earToClip.Previous;
                while (previousIsStillAnEarTip && earTips.Count > 0 && currentPolygon.Count > 2)
                {
                    // Add prev.Previous's vertex to the list
                    convexVertices.Add(currentVertices[prev.Previous.Data.Index]);

                    // Clip and remove from the list
                    currentPolygon.ClipVertex(prev);
                    earTips.Remove(prev);

                    // Keep track if prev is still an ear tip
                    previousIsStillAnEarTip = earTips.Contains(prev.Previous);

                    // prev.Previous may no longer be an ear, remove and reprocess
                    earTips.Remove(prev.Previous);
                    if (IsAnEarTip(prev.Previous, true))
                        earTips.Add(prev.Previous);

                    prev = prev.Previous;
                }

                convexVerticesGroups.Add(convexVertices);
            }
        }

        /// <summary>
        /// Given an outer polygon and a polygon that is inside of the outer polygon, form a bridge between these two polygons
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outerPolygon">The polygon on the outside</param>
        /// <param name="innerPolygon">The polygon inside of the outerPolygon</param>
        /// <param name="vertices">The list vertices to modify and add to, should be the same as outerPolygon.Vertices</param>
        /// <returns>Polygon that was the result of combining the inner and outer polygon</returns>
        private static Polygon<T>? ConnectOuterAndInnerPolygon<T>(Polygon<T> outerPolygon, Polygon<T> innerPolygon, List<Vector2> vertices) where T : PolygonVertex
        {
            if (outerPolygon.Count < 3 || outerPolygon.Head == null || outerPolygon.Vertices == null ||
                innerPolygon.Count < 3 || innerPolygon.Head == null || innerPolygon.Vertices == null) return null;

            List<Vector2> outerVertices = outerPolygon.Vertices;
            List<Vector2> innerVertices = innerPolygon.Vertices;

            // Find the bridge
            #region BridgeFinding

            // Get the right-most vertex of the inner polygon
            if (innerPolygon.RightMostVertex == null) 
                return null;
            VertexNode<T> rightMostInnerVertex = innerPolygon.RightMostVertex;

            // Create a point directly to the right of the right-most hole vertex
            // in order to line intersections to find nearest edge
            Vector2 pointRightOfInnerVertex = new Vector2(innerVertices[rightMostInnerVertex.Data.Index].X + 1.0f,
                                innerVertices[rightMostInnerVertex.Data.Index].Y);

            // Find closest intersection point and line segment using partial line intersection test
            float shortestLength = float.MaxValue; // arbitrarily large value
            VertexNode<T>? closestLineSegment = null;
            Vector2? closestIntersectionPoint = null;
            VertexNode<T> now = outerPolygon.Head;
            do
            {
                int IndexA = now.Data.Index;
                int IndexB = now.Next.Data.Index;

                // Intersection should only be computed if
                // outerPolygonVertices[aIdx].y is below or on the ray and
                // outerPolygonVertices[bIdx].y is above or on the ray
                if (outerVertices[IndexA].Y < pointRightOfInnerVertex.Y ||
                    outerVertices[IndexB].Y > pointRightOfInnerVertex.Y)
                {
                    now = now.Next;
                    continue;
                }

                // Perform the intersection test
                int result = PartialLineIntersection(innerVertices[rightMostInnerVertex.Data.Index], pointRightOfInnerVertex,
                    outerVertices[IndexA], outerVertices[IndexB], out float t, out float u);

                if (result == 1 && t >= 0.0f && u >= 0.0f && u <= 1.0f)
                {
                    // There is a line intersection
                    Vector2 intersectionPoint = new Vector2(innerVertices[rightMostInnerVertex.Data.Index].X + t * (pointRightOfInnerVertex.X - innerVertices[rightMostInnerVertex.Data.Index].X),
                        innerVertices[rightMostInnerVertex.Data.Index].Y + t * (pointRightOfInnerVertex.Y - innerVertices[rightMostInnerVertex.Data.Index].Y));

                    float length = SegmentLengthSquared(innerVertices[rightMostInnerVertex.Data.Index], intersectionPoint);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        closestLineSegment = now;
                        closestIntersectionPoint = intersectionPoint;
                    }
                }

                now = now.Next;
            } while (now != outerPolygon.Head);

            // No intersection was found
            if (closestLineSegment == null || closestIntersectionPoint == null)
                return null;

            // Select point P to be the endpoint of the closest edge with the largest x-value
            VertexNode<T> vertexP = closestLineSegment;
            if (outerVertices[closestLineSegment.Next.Data.Index].X > outerVertices[vertexP.Data.Index].X)
            {
                vertexP = closestLineSegment.Next;
            }

            // Look at each reflex vertices of the outer polygon, except P
            // If all of these vertices are outside the triangle (M, I, P),
            // then M and P are mutually visible
            VertexNode<T> visibleOuterVertex = vertexP;

            float MIPArea = TriangleArea(innerVertices[rightMostInnerVertex.Data.Index], closestIntersectionPoint.Value, outerVertices[vertexP.Data.Index]);
            // If MIPArea == 0, then M and P are colinear, M and P must be mutually visible in this case
            if (!IsNearlyEqual(MIPArea, 0.0f))
            {
                bool MandPareVisible = true;
                List<VertexNode<T>> possibleRVertices = new List<VertexNode<T>>();
                now = outerPolygon.Head;
                do
                {
                    // vertex is the same as p or is not convex
                    if (now == vertexP || IsConvex(now))
                    {
                        now = now.Next;
                        continue;
                    }

                    // TODO: Optimization for determining if a vertex is inside a triangle can be done below

                    // Determine if the vertex is inside the triangle (M, I, P) using Barycentric coordinates
                    // u = CAP / ABC
                    float u = TriangleArea(outerVertices[vertexP.Data.Index], innerVertices[rightMostInnerVertex.Data.Index], outerVertices[now.Data.Index]) / MIPArea;
                    if (u < 0.0f || u > 1.0f)
                    {
                        now = now.Next;
                        continue;
                    }
                    // v = ABP / ABC
                    float v = TriangleArea(innerVertices[rightMostInnerVertex.Data.Index], closestIntersectionPoint.Value, outerVertices[now.Data.Index]) / MIPArea;
                    if (v < 0.0f || v > 1.0f)
                    {
                        now = now.Next;
                        continue;
                    }
                    // w = BCP / ABC
                    float w = TriangleArea(closestIntersectionPoint.Value, outerVertices[vertexP.Data.Index], outerVertices[now.Data.Index]) / MIPArea;
                    if (w < 0.0f || w > 1.0f)
                    {
                        now = now.Next;
                        continue;
                    }

                    if (IsNearlyEqual(u + v + w, 1.0f))
                    {
                        // M and P are not mutually visible, but an R candidate identified
                        MandPareVisible = false;
                        possibleRVertices.Add(now);
                    }

                    now = now.Next;
                } while (now != outerPolygon.Head);

                if (!MandPareVisible)
                {
                    // M and P are not mutually visible, Search for the reflex R with a minimum angle angle
                    // between ⟨M , I⟩ and ⟨M , R⟩; then M and R are mutually visible and the algorithm terminates.
                    float shortestAngle = DiamondAngleBetweenTwoVectors(closestIntersectionPoint.Value, innerVertices[rightMostInnerVertex.Data.Index], outerVertices[vertexP.Data.Index]);
                    foreach (VertexNode<T> vertexR in possibleRVertices)
                    {
                        float angle = DiamondAngleBetweenTwoVectors(closestIntersectionPoint.Value, innerVertices[rightMostInnerVertex.Data.Index], outerVertices[vertexR.Data.Index]);
                        if (angle < shortestAngle)
                        {
                            shortestAngle = angle;
                            visibleOuterVertex = vertexR;
                        }
                    }
                }
            }
            #endregion
            // visibleOuterVertex and rightMostInnerVertex are vertices that are mutually visible and a bridge can be form between these two vertices

            Polygon<T> output = new Polygon<T>();
            // We are assuming that the vertices list will represent output's list of vertices
            output.Vertices = vertices;
            // Start with the head of outer polygon
            VertexNode<T> outerNow = outerPolygon.Head;
            do
            {
                // Insert the index of the outerNow since it's 
                output.InsertVertexAtBack(outerNow.Data.Index);

                // Transition to inner polygon
                if (outerNow == visibleOuterVertex)
                {
                    // Insert the rightMostInnerVertex into the output polygon
                    int rightMostInnerVertexIndex = output.Vertices.Count;
                    output.Vertices.Add(innerVertices[rightMostInnerVertex.Data.Index]);
                    output.InsertVertexAtBack(rightMostInnerVertexIndex);
                    // Switch to the inner polygon
                    VertexNode<T> innerNow = rightMostInnerVertex.Next;
                    while (innerNow != rightMostInnerVertex)
                    {
                        int insertedVertexIndex = output.Vertices.Count;
                        output.Vertices.Add(innerVertices[innerNow.Data.Index]);
                        output.InsertVertexAtBack(insertedVertexIndex);

                        innerNow = innerNow.Next;
                    }
                    // Insert the rightMostInnerVertex again to complete the loop for inner polygon
                    output.InsertVertexAtBack(rightMostInnerVertexIndex);
                    // Insert visibleOuterVertex to return from inner polygon to outer polygon
                    output.InsertVertexAtBack(visibleOuterVertex.Data.Index);
                }

                outerNow = outerNow.Next;
            } while (outerNow != outerPolygon.Head);

            return output;
        }

        /// <summary>
        /// Check if the given node is an ear tip
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node to be checked</param>
        /// <returns>True if the node is an ear tip, false otherwise</returns>
        private static bool IsAnEarTip<T>(VertexNode<T> node, bool includeZeroAngles = false) where T : PolygonVertex
        {
            // It is an eartip if it is convex and there is no other vertices inside its triangle
            // It's reflex, then it is not an ear tip
            if (!IsConvex(node, includeZeroAngles)) return false;

            List<Vector2> vertices = node.Owner.Vertices;

            bool isEar = true;
            VertexNode<T> now = node.Owner.Head;
            // Check if there exist a vertex inside its triangle
            do
            {
                // Skip the vertices if it is one of the triangle
                if (now.Data.Index == node.Previous.Data.Index ||
                    now.Data.Index == node.Data.Index ||
                    now.Data.Index == node.Next.Data.Index)
                {
                    now = now.Next;
                    continue;
                }

                if (PointIsInsideTriangle(vertices[now.Data.Index],
                    vertices[node.Previous.Data.Index],
                    vertices[node.Data.Index],
                    vertices[node.Next.Data.Index]))
                {
                    isEar = false;
                    break;
                }

                now = now.Next;
            } while (now != node.Owner.Head);

            return isEar;
        }

        /// <summary>
        /// A function that finds all ear tips of the given polygon
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="polygon">The polygon to find the ear tips for</param>
        /// <returns>List of nodes that have been identified to be ear tips, null if no eartips was found, or is not possible</returns>
        private static List<VertexNode<T>>? FindEarTips<T>(Polygon<T> polygon, bool includeZeroAngles = false) where T : PolygonVertex
        {
            if (polygon.Head == null || polygon.Vertices == null) return null;

            List<VertexNode<T>> output = new List<VertexNode<T>>();

            // Add each node to output list if it is an ear tip
            VertexNode<T> now = polygon.Head;
            do
            {
                if (IsAnEarTip(now, includeZeroAngles))
                {
                    output.Add(now);
                }

                now = now.Next;
            } while (now != polygon.Head);

            // No eartips found, return null
            if (output.Count <= 0) return null;
            return output;
        }

        /// <summary>
        /// Convert the PolygonVertex of the given polygon into a BooleanVertex
        /// </summary>
        /// <typeparam name="T">The original PolygonVertex of the given polygon</typeparam>
        /// <typeparam name="U">The desired BooleanVertex class</typeparam>
        /// <param name="polygon">The polygon to convert</param>
        /// <returns>Copy of the given polygon but with BooleanVertex insteadd</returns>
        private static Polygon<U> ConvertPolygonToBooleanList<T, U>(Polygon<T> polygon) 
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            Polygon<U> toReturn = new Polygon<U>();
            // Should be a copy here
            toReturn.Vertices = polygon.Vertices;

            if (polygon.Head == null) return toReturn;

            VertexNode<T> now = polygon.Head;
            do
            {
                VertexNode<U> copy = toReturn.InsertVertexAtBack(now.Data.Index);
                copy.Data.CopyData(now.Data);
                now = now.Next;
            } while (now != polygon.Head);
            return toReturn;
        }

        /// <summary>
        /// Determine if the given point is inside triangle abc
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool PointIsInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float ABCarea = TriangleArea(a, b, c);
            if (IsNearlyEqual(ABCarea, 0.0f)) return false;

            // Determine if the vertex is inside the triangle using Barycentric coordinates
            // u = CAP / ABC
            float u = TriangleArea(c, a, point) / ABCarea;
            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }
            // v = ABP / ABC
            float v = TriangleArea(a, b, point) / ABCarea;
            if (v < 0.0f || v > 1.0f)
            {
                return false;
            }
            // w = BCP / ABC
            float w = TriangleArea(b, c, point) / ABCarea;
            if (w < 0.0f || w > 1.0f)
            {
                return false;
            }

            if (IsNearlyEqual(u + v + w, 1.0f))
            {
                return true;
            }

            return false;
        }

        private static bool  BarycentricCoordinates(Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float u, out float v, out float w)
        {
            float ABCarea = TriangleArea(a, b, c);
            if (IsNearlyEqual(ABCarea, 0.0f))
            {
                u = 0.0f;
                v = 0.0f;
                w = 0.0f;
                return false;
            }

            // u = CAP / ABC
            u = TriangleArea(c, a, point) / ABCarea;
            if (u < 0.0f || u > 1.0f)
            {
                v = 0.0f;
                w = 0.0f;
                return false;
            }
            // v = ABP / ABC
            v = TriangleArea(a, b, point) / ABCarea;
            if (v < 0.0f || v > 1.0f)
            {
                w = 0.0f;
                return false;
            }
            // w = BCP / ABC
            w = TriangleArea(b, c, point) / ABCarea;
            if (w < 0.0f || w > 1.0f)
            {
                return false;
            }

            if (IsNearlyEqual(u + v + w, 1.0f))
                return true;

            return false;
        }

        /// <summary>
        /// Determine if the given point is inside the given polygon
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns>Returns 1, if the given point is inside the given polygon.
        /// Returns 0, if the given point is on the edge or vertex of the given polygon.
        /// Returns -1, if the given point is outside the given polygon.</returns>
        public static int PointIsInsidePolygon<T>(Vector2 point, Polygon<T> polygon) where T : PolygonVertex
        {
            if (polygon.Head == null || polygon.Vertices == null || polygon.Count < 3) return -1;

            bool between(float p, float a, float b)
            {
                return (p >= a && p <= b) || (p <= a && p >= b);
            }

            List<Vector2> vertices = polygon.Vertices;
            bool inside = false;

            VertexNode<T> now = polygon.Head;
            do
            {
                Vector2 a = vertices[now.Data.Index];
                Vector2 b = vertices[now.Next.Data.Index];

                // Corner cases
                if (IsNearlyEqual(point.X, a.X) && IsNearlyEqual(point.Y, a.Y) ||
                    IsNearlyEqual(point.X, b.X) && IsNearlyEqual(point.Y, b.Y))
                    return 0;
                if (IsNearlyEqual(a.Y, b.Y) && IsNearlyEqual(point.Y, a.Y)
                    && between(point.X, a.X, b.X))
                    return 0;

                if (between(point.Y, a.Y, b.Y)) // If point is inside the vertical range
                {
                    // Below is extremely unlikely
                    if (IsNearlyEqual(point.Y, a.Y) && b.Y >= a.Y ||
                        IsNearlyEqual(point.Y, b.Y) && a.Y >= b.Y)
                    {
                        now = now.Next;
                        continue;
                    }

                    float c = (a.X - point.X) * (b.Y - point.Y) - (b.X - point.X) * (a.Y - point.Y);
                    if (IsNearlyEqual(c, 0.0f))
                        return 0;
                    if ((a.Y < b.Y) == (c > 0))
                        inside = !inside;
                }

                now = now.Next;

            } while (now != polygon.Head);

            return inside ? 1 : -1;
        }

        /// <summary>
        /// Compute the area of triangle abc
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathF.Abs(CrossProduct2D(b - a, c - a) / 2.0f);
        }

        /// <summary>
        /// Determine if the given node is convex
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsConvex<T>(VertexNode<T> node, bool includeZeroAngles = false) where T : PolygonVertex
        {
            if (node.Owner.Vertices == null) return false;
            List<Vector2> vertices = node.Owner.Vertices;
            // Takes advantage of the fact that triangulation is
            // done for 2D polygons. Thus the cross product
            // of two 2D vectors result in either a positive or
            // negative Z axis value
            Vector2 a = vertices[node.Previous.Data.Index];
            Vector2 b = vertices[node.Data.Index];
            Vector2 c = vertices[node.Next.Data.Index];

            float crossResult = CrossProduct2D(a - b, c - b);

            if (!includeZeroAngles)
                return crossResult > 0.0f;

            return crossResult >= 0.0f || IsNearlyEqual(crossResult, 0.0f);
        }

        /// <summary>
        /// Perform a partial line intersection computation between two lines
        /// </summary>
        /// <param name="a0">Starting point of Line A</param>
        /// <param name="a1">End point of Line A</param>
        /// <param name="b0">Starting point of Line B</param>
        /// <param name="b1">End point of Line B</param>
        /// <param name="tValue">The t value computed</param>
        /// <param name="uValue">The u value computed</param>
        /// <returns>Returns 1 if the given lines are not parallel,
        /// Returns 0 if the given lines are colinear,
        /// Returns -1 if the given lines are parallel</returns>
        private static int PartialLineIntersection(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out float tValue, out float uValue)
        {
            float determinant = (a0.X - a1.X) * (b0.Y - b1.Y) - (a0.Y - a1.Y) * (b0.X - b1.X);
            if (!IsNearlyEqual(determinant, 0.0f))
            {
                tValue = ((a0.X - b0.X) * (b0.Y - b1.Y) - (a0.Y - b0.Y) * (b0.X - b1.X)) / determinant;
                uValue = ((a1.X - a0.X) * (a0.Y - b0.Y) - (a1.Y - a0.Y) * (a0.X - b0.X)) / determinant;
                return 1;
            }
            else
            {
                float top = ((a1.X - a0.X) * (a0.Y - b0.Y) + (a1.Y - a0.Y) * (a0.X - b0.X));
                if (IsNearlyEqual(top, 0.0f)) // Lines are colinear
                {
                    tValue = ((a0.X - b0.X) * (b1.X - b0.X) + (a0.Y - b0.Y) * (b1.Y - b0.Y)) /
                        ((b1.X - b0.X) * (b1.X - b0.X) + (b1.Y - b0.Y) * (b1.Y - b0.Y));
                    uValue = ((b0.X - a0.X) * (a1.X - a0.X) + (b0.Y - a0.Y) * (a1.Y - a0.Y)) /
                        ((a1.X - a0.X) * (a1.X - a0.X) + (a1.Y - a0.Y) * (a1.Y - a0.Y));
                    return 0;
                }
            }
            tValue = uValue = 0.0f;
            return -1;
        }

        /// <summary>
        /// Get the angle diamond difference between vector ba and bc
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static float DiamondAngleBetweenTwoVectors(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ba = a - b;
            Vector2 bc = c - b;
            float result = MathF.Abs(VectorToDiamondAngle(ba) - VectorToDiamondAngle(bc));
            if (result > 2.0f)
                return 4.0f - result;
            return result;
        }

        /// <summary>
        /// Convert a 2D vector into diamond angle measurement
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static float VectorToDiamondAngle(Vector2 v)
        {
            if (IsNearlyEqual(v.LengthSquared(), 0.0f))
                return 0.0f;

            if (v.Y >= 0.0f)
            {
                if (v.X >= 0.0f)
                    return v.Y / (v.X + v.Y);
                return 1.0f - v.X / (-v.X + v.Y);
            }

            if (v.X < 0.0f)
            {
                return 2.0f - v.Y / (-v.X - v.Y);
            }

            return 3.0f + v.X / (v.X - v.Y);
        }

        /// <summary>
        /// Perform a cross product between two 2D vectors and return the Z value
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return (a.X * b.Y) - (a.Y * b.X);
        }

        /// <summary>
        /// Get squared length of a line segment
        /// </summary>
        /// <param name="a">First point of the line segment</param>
        /// <param name="b">Second point of the line segment</param>
        /// <returns>Squared length of a line segment</returns>
        private static float SegmentLengthSquared(Vector2 a, Vector2 b)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            return x * x + y * y;
        }

        /// <summary>
        /// Check if the given two float values are nearly equal to each other
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        /// <param name="epsilon">The threshold for comparison</param>
        /// <returns>True if they are nearly equal to each other, false otherwise</returns>
        private static bool IsNearlyEqual(float a, float b, float epsilon = 0.00001f)
        {
            return MathF.Abs(a - b) <= epsilon;
        }
    }


}
