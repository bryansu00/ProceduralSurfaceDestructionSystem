
namespace PSDSystem
{
    public class OuterPolygon : Polygon<PolygonVertex>
    {
        public List<Polygon<PolygonVertex>> InnerPolygons { get; private set; }

        public OuterPolygon(List<Polygon<PolygonVertex>>? innerPolygons = null)
        {
            if (innerPolygons != null) InnerPolygons = innerPolygons;
            else InnerPolygons = new List<Polygon<PolygonVertex>>();
        }
    }
}
