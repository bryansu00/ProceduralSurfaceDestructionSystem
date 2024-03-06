using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Text;
using static PSDSystem.PSD;

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
        static bool IsNearlyEqual(float a, float b, float epsilon = 0.00001f)
        {
            return MathF.Abs(a - b) <= epsilon;
        }

        /// <summary>
        /// Intersect a cutter polygon with another polygon and returns the findings between the two
        /// </summary>
        /// <typeparam name="T">A vertex class that has boolean vertex properties to represent each vertex of a polygon and store additional results from the intersection test</typeparam>
        /// <param name="cutter">The cutter polygon</param>
        /// <param name="polygon">The other polygon</param>
        /// <param name="intersectionPoints">List of intersection points to return</param>
        /// <returns>The result from intersecting the two given polygons</returns>
        public static IntersectionResult IntersectCutterAndPolygon<T>(Polygon<T> cutter, Polygon<T> polygon, out IntersectionPoints<T>? intersectionPoints) where T : PolygonVertex, IHasBooleanVertexProperties<T>
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
                        #region EdgeCase1
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
                bool cutterIsOutsidePolygon = originalCutterOutsideFlag ? cutter.Head.Data.IsOutside : !cutter.Head.Data.IsOutside;
                bool polygonIsOutsidePolygon = originalPolygonOutsideFlag ? polygon.Head.Data.IsOutside : !polygon.Head.Data.IsOutside;

                intersectionPoints = null;
                if (polygonIsOutsidePolygon && cutterIsOutsidePolygon) return IntersectionResult.BOTH_OUTSIDE;
                return cutterIsOutsidePolygon ? IntersectionResult.POLYGON_IS_INSIDE : IntersectionResult.CUTTER_IS_INSIDE;
            }

            intersectionPoints = new IntersectionPoints<T>(polygon, intersections);
            return 0;
        }

        private static bool CheckSpecialAdditionCase<T>(VertexNode<T> node) where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            if (node.Data.Cross == null) return false;
            if (!node.Data.Cross.Next.Data.IsOutside) return false;
            if (node.Data.Cross.Next.Data.Cross == null) return true;
            var crossback = node.Data.Cross.Next.Data.Cross;
            if (crossback == node.Next) return false;
            return true;
        }

        /// <summary>
        /// TO DO: FIX ISSUE AROUND THIS SPECIAL CASE
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="center"></param>
        /// <param name="outerPolygon"></param>
        /// <returns></returns>
        private static bool CheckSpecialSubtractionCase<T>(VertexNode<T> node, Polygon<T> center, Polygon<T> outerPolygon) where T : PolygonVertex, IHasBooleanVertexProperties<T>
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

        /// <summary>
        /// Perform a boolean addition operation using the given information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="center"></param>
        /// <param name="polygons"></param>
        /// <param name="intersections"></param>
        /// <returns></returns>
        public static List<Polygon<T>> CombinePolygons<T, U>(Polygon<U> center, List<IntersectionPoints<U>> intersections)
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            List<Polygon<T>> outputPolygons = new List<Polygon<T>>();

            if (center.Head == null || center.Count < 3 || intersections.Count == 0)
                return outputPolygons;

            // Insert Points
            InsertIntersectionPoints(center, intersections);

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

            return outputPolygons;
        }

        public static List<Polygon<T>> CombinePolygons<T, U>(Polygon<U> center, IntersectionPoints<U> outerIntersections, List<IntersectionPoints<U>> innerIntersections)
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            List<Polygon<T>> outputPolygons = new List<Polygon<T>>();

            if (center.Head == null || center.Count < 3 || outerIntersections.Polygon.Head == null)
                return outputPolygons;

            InsertIntersectionPoints(center, outerIntersections, innerIntersections);

            Polygon<U> outerPolygon = outerIntersections.Polygon;

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

                    bool pointIsCrossingPoint = CheckSpecialSubtractionCase(point, center, outerPolygon);

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

            return outputPolygons;
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

        public static void InsertIntersectionPoints<T>(Polygon<T> cutter, List<IntersectionPoints<T>> allIntersections) where T : PolygonVertex, IHasBooleanVertexProperties<T>
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
                    else if (IsNearlyEqual(t, 1.0f)) nodeAddedToCutter = cutterNode.Next;
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

        public static void InsertIntersectionPoints<T>(Polygon<T> cutter, IntersectionPoints<T> outerIntersections, List<IntersectionPoints<T>> innerIntersections) where T : PolygonVertex, IHasBooleanVertexProperties<T>
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
                    while (polygonNode.Next.Data.Cross != null && polygonNode.Next.Data.IsAnAddedVertex &&
                        SegmentLengthSquared(outerVertices[polygonNode.Data.Index], intersectionPoint) >
                        SegmentLengthSquared(outerVertices[polygonNode.Data.Index], outerVertices[polygonNode.Next.Data.Index]))
                    {
                        polygonNode = polygonNode.Next;
                    }
                    // Do the actual insertions
                    // NOTE: THIS FAILS TO HANDLE THE EDGE CASE WHERE VERTEX INTERSECTS THE EDGE
                    int insertedVertexLocation = outerVertices.Count;
                    outerVertices.Add(intersectionPoint);
                    nodeAddedToPolygon = polygonNode.Owner.InsertVertexAfter(polygonNode, insertedVertexLocation);
                    nodeAddedToPolygon.Data.IsAnAddedVertex = true;
                }

                // Do the same thing as above, but with cutter this time
                if (IsNearlyEqual(u, 0.0f)) nodeAddedToCutter = cutterNode;
                else if (IsNearlyEqual(t, 1.0f)) nodeAddedToCutter = cutterNode.Next;
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
                    else if (IsNearlyEqual(t, 1.0f)) nodeAddedToCutter = cutterNode.Next;
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

        /// <summary>
        /// Convert the PolygonVertex of the given polygon into a BooleanVertex
        /// </summary>
        /// <typeparam name="T">The original PolygonVertex of the given polygon</typeparam>
        /// <typeparam name="U">The desired BooleanVertex class</typeparam>
        /// <param name="polygon">The polygon to convert</param>
        /// <returns>Copy of the given polygon but with BooleanVertex insteadd</returns>
        public static Polygon<U> ConvertPolygonToBooleanList<T, U>(Polygon<T> polygon) 
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
                toReturn.InsertVertexAtBack(now.Data.Index);
                now = now.Next;
            } while (now != polygon.Head);
            return toReturn;
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
                    // Below is extremly unlikely
                    if (point.Y == a.Y && b.Y >= a.Y ||
                        point.Y == b.Y && a.Y >= b.Y)
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
        /// Debugging purpose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="polygon"></param>
        static void PrintBooleanList<T>(Polygon<T> polygon) where T : PolygonVertex, IHasBooleanVertexProperties<T>
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
    }
}
