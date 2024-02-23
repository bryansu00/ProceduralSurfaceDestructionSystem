
namespace PSDSystem
{
    public class SurfaceShape<T> where T : PolygonVertex
    {
        public List<PolygonPair<T>> Polygons { get; }

        public SurfaceShape()
        {
            Polygons = new List<PolygonPair<T>>();
        }

        
    }

    public class PolygonPair<T> where T : PolygonVertex
    {
        public Polygon<T> OuterPolygon { get; }

        public List<Polygon<T>> InnerPolygons { get; }

        public PolygonPair(Polygon<T>? outerPolygon, List<Polygon<T>>? innerPolygons = null)
        {
            if (outerPolygon != null) OuterPolygon = outerPolygon;
            else OuterPolygon = new Polygon<T>();

            if (innerPolygons != null) InnerPolygons = innerPolygons;
            else InnerPolygons = new List<Polygon<T>>();
        }
    }
}
