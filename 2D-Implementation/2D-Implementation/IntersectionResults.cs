
using System.Numerics;

namespace PSDSystem
{
    public class IntersectionResults<T> where T : PolygonVertex, IHasBooleanVertexProperties<T>
    {
        public Polygon<T> PolygonList { get; }

        public Polygon<T> CutterList { get; }

        public List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> Intersections { get; }

        public IntersectionResults(Polygon<T> polygonList, Polygon<T> cutterList, List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> intersections)
        {
            PolygonList = polygonList;
            CutterList = cutterList;
            Intersections = intersections;
        }
    }
}
