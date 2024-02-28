
using System.Numerics;

namespace PSDSystem
{
    public class IntersectionResults<T> where T : PolygonVertex, IHasBooleanVertexProperties<T>
    {
        public Polygon<T> Polygon { get; }

        public Polygon<T> Cutter { get; }

        public List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> Intersections { get; }

        public IntersectionResults(Polygon<T> polygon, Polygon<T> cutter, List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> intersections)
        {
            Polygon = polygon;
            Cutter = cutter;
            Intersections = intersections;
        }
    }
}
