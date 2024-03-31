using System.Numerics;

namespace PSDSystem
{
    public class IntersectionPoints<T> where T : PolygonVertex, IHasBooleanVertexProperties<T>
    {
        public Polygon<T> Polygon { get; }

        public List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> Intersections { get; }

        public IntersectionPoints(Polygon<T> polygon, List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> intersections)
        {
            Polygon = polygon;
            Intersections = intersections;
        }
    }
}
