
using System.Net.Http.Headers;
using System.Numerics;

namespace PSDSystem
{
    public static class PSD
    {
        public static int IntersectCutterAndPolygon<T>(Polygon<T> cutter, Polygon<T> polygon, 
            out Polygon<BooleanVertex>? booleanCutter, out Polygon<BooleanVertex>? booleanPolygon) where T : PolygonVertex
        {
            // Intersection cannot performed, return -1 for invalid operation
            if (cutter.Head == null || cutter.Vertices == null || polygon.Head == null || polygon.Vertices == null)
            {
                booleanCutter = null;
                booleanPolygon = null;
                return -1;
            }

            bool CutterAndPolygonIntersects = false;

            booleanCutter = new Polygon<BooleanVertex>();
            booleanPolygon = new Polygon<BooleanVertex>();

            List<Vector2> polygonVertices = polygon.Vertices;
            List<Vector2> cutterVertices = cutter.Vertices;

            VertexNode<T> polygonNow = polygon.Head;
            do
            {
                Vector2 a0 = polygonVertices[polygonNow.Data.Index];
                Vector2 a1 = polygonVertices[polygonNow.Next.Data.Index];

                VertexNode<BooleanVertex> polygonBoolVertex = booleanPolygon.InsertVertexAtBack(polygonNow.Data.Index);

                VertexNode<T> cutterNow = cutter.Head;
                do
                {
                    Vector2 b0 = cutterVertices[cutterNow.Data.Index];
                    Vector2 b1 = cutterVertices[cutterNow.Next.Data.Index];

                    VertexNode<BooleanVertex> cutterBoolVertex = booleanCutter.InsertVertexAtBack(cutterNow.Data.Index);

                    // Perform a line calculation
                    int result = PartialLineIntersection(a0, a1, b0, b1, out float t, out float u);

                    // Both line segments are non-colinear, thus t and u can be used to determine if they intersect
                    if (result == 1)
                    {
                        if (t >= 0.0f && t <= 1.0f && u >= 0.0f && u <= 1.0f)
                        {
                            CutterAndPolygonIntersects = true;
                            // do something here...
                        }

                        // ----------------------------------------------------------------------------
                        // This is very unlikely due floating point precision error,
                        // but just in case...
                        bool a0IsOnInfiniteRay = t == 0.0f; // a0 is intersecting with the cutter's infinite ray
                        bool a1IsOnInfiniteRay = t == 1.0f;
                        bool b0IsOnInfiniteRay = u == 0.0f;
                        bool b1IsOnInfiniteRay = u == 1.0f;

                        bool polygonVertexOnEdge = false;
                        bool cutterVertexOnEdge = false;

                        if (u >= 0.0f && u <= 1.0f && a0IsOnInfiniteRay)
                        {
                            polygonBoolVertex.Data.IsOutside = false;
                            polygonVertexOnEdge = true;
                        }

                        if (t >= 0.0f && u <= 1.0f && b0IsOnInfiniteRay)
                        {
                            cutterBoolVertex.Data.IsOutside = false;
                            cutterVertexOnEdge = true;
                        }

                        // Modify polygonBoolVertex.Data.IsOutside value
                        if (polygonVertexOnEdge == false)
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
                                polygonBoolVertex.Data.IsOutside = !polygonBoolVertex.Data.IsOutside;
                            }

                            if (polygonIntersectsAPoint == false && t >= 0.0f && u >= 0.0f && u <= 1.0f)
                            {
                                // infinite ray from polygon's a0 intersects cutter's line segment
                                polygonBoolVertex.Data.IsOutside = !polygonBoolVertex.Data.IsOutside;
                            }
                        }

                        // Modify cutterBoolVertex.Data.IsOutside value
                        if (cutterVertexOnEdge == false)
                        {
                            bool cutterIntersectsAPoint = a0IsOnInfiniteRay || a1IsOnInfiniteRay;
                            float cutterCrossProductToPolygonLine = 0.0f;
                            // Line intersection occured at polygon's a0
                            if (a0IsOnInfiniteRay) cutterCrossProductToPolygonLine = CrossProduct2D(a0 - b0, a1 - b0);
                            // Line intersection occured at polygon's a1
                            else if (a1IsOnInfiniteRay) cutterCrossProductToPolygonLine = CrossProduct2D(a1 - b0, a0 - b0);
                            
                            if (cutterIntersectsAPoint && cutterCrossProductToPolygonLine > 0.0f)
                            {
                                cutterBoolVertex.Data.IsOutside = !cutterBoolVertex.Data.IsOutside;
                            }

                            // infinite ray from cutter's b0 intersects cutter's line segment
                            if (cutterIntersectsAPoint == false && u >= 0.0f && t >= 0.0f && t <= 1.0f)
                            {
                                cutterBoolVertex.Data.IsOutside = !cutterBoolVertex.Data.IsOutside;
                            }
                        }
                        // ----------------------------------------------------------------------------
                    }
                    // Both line segments are colinear, (This is unlikely due to floating point error, just in case though...)
                    else if (result == 0)
                    {
                        // polygonNow is on the edge of cutter's line segment
                        if (t >= 0.0f && t <= 1.0f)
                        {
                            polygonBoolVertex.Data.IsOutside = false;
                        }
                        // cutterNow is on the edge of target's line segment
                        if (u >= 0.0f && u <= 0.0f)
                        {
                            cutterBoolVertex.Data.IsOutside = false;
                        }
                    }

                    cutterNow = cutterNow.Next;
                } while (cutterNow != cutter.Head);

                polygonNow = polygonNow.Next;
            } while (polygonNow != polygon.Head);


            booleanCutter = null;
            booleanPolygon = null;
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

        private static Polygon<BooleanVertex> ConvertPolygonToBooleanList<T>(Polygon<T> polygon) where T : PolygonVertex
        {
            Polygon<BooleanVertex> toReturn = new Polygon<BooleanVertex>();
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

        private static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return (a.X * b.Y) - (a.Y * b.X);
        }
    }
}
