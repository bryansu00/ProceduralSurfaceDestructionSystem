
namespace PSDSystem
{
    public class SurfaceShape
    {
        public List<PolygonPair> Polygons { get; }

        public SurfaceShape()
        {
            Polygons = new List<PolygonPair>();
        }
    }

    public class PolygonPair
    {
        public Polygon<PolygonVertex> OuterPolygon { get; }

        public List<Polygon<PolygonVertex>> InnerPolygons { get; }

        public PolygonPair(Polygon<PolygonVertex>? outerPolygon, List<Polygon<PolygonVertex>>? innerPolygons = null)
        {
            if (outerPolygon != null) OuterPolygon = outerPolygon;
            else OuterPolygon = new Polygon<PolygonVertex>();

            if (innerPolygons != null) InnerPolygons = innerPolygons;
            else InnerPolygons = new List<Polygon<PolygonVertex>>();
        }
    }
}
