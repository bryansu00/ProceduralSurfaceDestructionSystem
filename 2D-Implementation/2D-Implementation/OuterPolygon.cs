
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

        public void AddInnerPolygon(Polygon<PolygonVertex> polygon)
        {
            InnerPolygons.Add(polygon);
        }

        public void RemoveInnerPolygon(Polygon<PolygonVertex> polygon)
        {
            InnerPolygons.Remove(polygon);
        }
    }
}
