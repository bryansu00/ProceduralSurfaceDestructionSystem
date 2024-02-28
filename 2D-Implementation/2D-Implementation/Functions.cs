using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace PSDSystem
{
    public static class PSD
    {
        public static int IntersectCutterAndPolygon<T, U>(Polygon<T> cutter, Polygon<T> polygon, out IntersectionResults<U>? intersectionResults) 
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            // Intersection cannot performed, return -1 for invalid operation
            intersectionResults = null;
            if (cutter.Count < 3 || cutter.Vertices == null || polygon.Count < 3 || polygon.Vertices == null)
            {
                return -1;
            }

            Polygon<U> booleanCutter = ConvertPolygonToBooleanList<T, U>(cutter);
            Polygon<U> booleanPolygon = ConvertPolygonToBooleanList<T, U>(polygon);
            if (booleanCutter.Head == null || booleanPolygon.Head == null)
            {
                return -1;
            }

            // SHOULD MAKE A COPY HERE...
            List<Vector2> polygonVertices = polygon.Vertices;
            List<Vector2> cutterVertices = cutter.Vertices;

            #region IntersectionTest

            List<Tuple<Vector2, VertexNode<U>, float, VertexNode<U>, float>> intersections = new List<Tuple<Vector2, VertexNode<U>, float, VertexNode<U>, float>>();

            VertexNode<U> polygonNow = booleanPolygon.Head;
            do
            {
                Vector2 a0 = polygonVertices[polygonNow.Data.Index];
                Vector2 a1 = polygonVertices[polygonNow.Next.Data.Index];

                VertexNode<U> cutterNow = booleanCutter.Head;
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
                        bool a0IsOnInfiniteRay = t == 0.0f; // a0 is intersecting with the cutter's infinite ray
                        bool a1IsOnInfiniteRay = t == 1.0f;
                        bool b0IsOnInfiniteRay = u == 0.0f;
                        bool b1IsOnInfiniteRay = u == 1.0f;

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
                                new Tuple<Vector2, VertexNode<U>, float, VertexNode<U>, float>(intersectionPoint, polygonNow, t, cutterNow, u)
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
                } while (cutterNow != booleanCutter.Head);

                polygonNow = polygonNow.Next;
            } while (polygonNow != booleanPolygon.Head);
            #endregion

            if (intersections.Count == 0)
            { 
                // No intersection between the two given polygons, thus end function
                bool cutterIsOutsidePolygon = booleanCutter.Head.Data.IsOutside;

                // Return 1 if cutter is outside, 2 if it is inside
                return cutterIsOutsidePolygon ? 1 : 2;
            }

            intersectionResults = new IntersectionResults<U>(booleanPolygon, booleanCutter, intersections);
            return 0;
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
            if (determinant != 0.0f)
            {
                tValue = ((a0.X - b0.X) * (b0.Y - b1.Y) - (a0.Y - b0.Y) * (b0.X - b1.X)) / determinant;
                uValue = ((a1.X - a0.X) * (a0.Y - b0.Y) - (a1.Y - a0.Y) * (a0.X - b0.X)) / determinant;
                return 1;
            }
            else
            {
                float top = ((a1.X - a0.X) * (a0.Y - b0.Y) + (a1.Y - a0.Y) * (a0.X - b0.X));
                if (top == 0.0) // Lines are colinear
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

        public static void InsertIntersectionPoints<T>(IntersectionResults<T> intersectionResults) where T : PolygonVertex, IHasBooleanVertexProperties<T>
        {
            if (intersectionResults.PolygonList.Vertices == null || intersectionResults.CutterList.Vertices == null) return;

            List<Vector2> polygonVertices = intersectionResults.PolygonList.Vertices;
            List<Vector2> cutterVertices = intersectionResults.CutterList.Vertices;

            // Insert the intersection points
            List<VertexNode<T>> insertedIntersection = new List<VertexNode<T>>();
            foreach (var result in intersectionResults.Intersections)
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
                if (t == 0.0f) nodeAddedToPolygon = polygonNode;
                else if (t == 1.0f) nodeAddedToPolygon = polygonNode.Next;
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
                if (u == 0.0f) nodeAddedToCutter = cutterNode;
                else if (u == 1.0f) nodeAddedToCutter = cutterNode.Next;
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

                insertedIntersection.Add(nodeAddedToPolygon);
            }

            // Add additional vertices, needed for boolean-subtraction operations
            foreach (VertexNode<T> node in insertedIntersection)
            {
                if (node.Next.Data.IsOutside) continue;

                Vector2 extraInsertionPoint = new Vector2(
                    (polygonVertices[node.Next.Data.Index].X - polygonVertices[node.Data.Index].X) / 2.0f + polygonVertices[node.Data.Index].X,
                    (polygonVertices[node.Next.Data.Index].Y - polygonVertices[node.Data.Index].Y) / 2.0f + polygonVertices[node.Data.Index].Y
                );

                if (PointIsInsidePolygon(extraInsertionPoint, intersectionResults.CutterList) == -1)
                {
                    int insertedVertexLocation = polygonVertices.Count;
                    polygonVertices.Add(extraInsertionPoint);
                    VertexNode<T> addedNode = intersectionResults.PolygonList.InsertVertexAfter(node, insertedVertexLocation);
                    addedNode.Data.IsAnAddedVertex = true;
                }
            }
        }

        private static Polygon<U> ConvertPolygonToBooleanList<T, U>(Polygon<T> polygon) 
            where T : PolygonVertex
            where U : PolygonVertex, IHasBooleanVertexProperties<U>
        {
            Polygon<U> toReturn = new Polygon<U>();
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
        private static int PointIsInsidePolygon<T>(Vector2 point, Polygon<T> polygon) where T : PolygonVertex
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

                // Corner cases (extremely unlikely due to floating point error)
                if (point.X == a.X && point.Y == a.Y ||
                    point.X == b.X && point.Y == b.Y)
                    return 0;
                if (a.Y == b.Y && point.Y == a.Y
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
                    if (c == 0)
                        return 0;
                    if ((a.Y < b.Y) == (c > 0))
                        inside = !inside;
                }

                now = now.Next;

            } while (now != polygon.Head);

            return inside ? 1 : -1;
        }

        private static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return (a.X * b.Y) - (a.Y * b.X);
        }

        private static float SegmentLengthSquared(Vector2 a, Vector2 b)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            return x * x + y * y;
        }
    }
}
