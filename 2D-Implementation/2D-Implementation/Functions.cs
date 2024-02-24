
using System.Net.Http.Headers;
using System.Numerics;

namespace PSDSystem
{
    public static class Functions
    {
        public static List<IntersectionResult<T>>? IntersectCutterAndPolygon<T>(Polygon<T> cutterPolygon, Polygon<T> otherPolygon) where T : PolygonVertex
        {
            if (cutterPolygon.Head == null || cutterPolygon.Vertices == null || otherPolygon.Head == null || otherPolygon.Vertices == null) return null;

            List<IntersectionResult<T>> results = new List<IntersectionResult<T>>();

            VertexNode<T> cutterNow = cutterPolygon.Head;
            do
            {
                VertexNode<T> otherNow = otherPolygon.Head;
                do
                {
                    IntersectionResult<T>? result = LineIntersection(cutterNow, otherNow);

                    if (result != null)
                    {
                        results.Add(result);
                    }

                    otherNow = otherNow.Next;

                } while (otherNow != otherPolygon.Head);

                cutterNow = cutterNow.Next;

            } while (cutterNow != cutterPolygon.Head);

            if (results.Count > 0) return results;

            return null;
        }

        private static IntersectionResult<T>? LineIntersection<T>(VertexNode<T> cutterVertex, VertexNode<T> otherVertex) where T : PolygonVertex
        {
            if (cutterVertex.Owner.Vertices == null || otherVertex.Owner.Vertices == null) return null;

            float tValue, uValue;

            int result = PartialLineIntersection(otherVertex.Owner.Vertices[otherVertex.Data.Index], otherVertex.Owner.Vertices[otherVertex.Next.Data.Index],
                cutterVertex.Owner.Vertices[cutterVertex.Data.Index], cutterVertex.Owner.Vertices[cutterVertex.Next.Data.Index], out tValue, out uValue);

            if (result == -1) return null;
            if (result == 0) return new IntersectionResult<T>(cutterVertex, otherVertex, tValue, uValue, true);

            return new IntersectionResult<T>(cutterVertex, otherVertex, tValue, uValue, false);
        }

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
                if (top == 0.0) // Lines are collinear
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
    }

    public class IntersectionResult<T> where T : PolygonVertex
    {
        public VertexNode<T> CutterVertex { get; }

        public VertexNode<T> OtherVertex { get; }

        public float TValue { get; }

        public float UValue { get; }

        public bool IsColinear { get; }

        public IntersectionResult(VertexNode<T> cutterVertex, VertexNode<T> otherVertex, float tValue, float uValue, bool isColinear)
        {
            CutterVertex = cutterVertex;
            OtherVertex = otherVertex;
            TValue = tValue;
            UValue = uValue;
            IsColinear = isColinear;
        }
    }
}
