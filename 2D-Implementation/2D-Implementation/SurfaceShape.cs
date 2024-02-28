
namespace PSDSystem
{
    public class SurfaceShape<T> where T : PolygonVertex
    {
        public List<PolygonGroup<T>> Polygons { get; }

        public SurfaceShape()
        {
            Polygons = new List<PolygonGroup<T>>();
        }

        public void AddOuterPolygon(Polygon<T> polygon)
        {
            PolygonGroup<T> pair = new PolygonGroup<T>(polygon);

            Polygons.Add(pair);
        }

        public void AddPair(Polygon<T> outerPolygon, List<Polygon<T>> innerPolygons)
        {
            PolygonGroup<T> pair = new PolygonGroup<T>(outerPolygon, innerPolygons);

            Polygons.Add(pair);
        }

        public void RemoveGroup(PolygonGroup<T> group)
        {
            Polygons.Remove(group);
        }
    }

    public class PolygonGroup<T> where T : PolygonVertex
    {
        public Polygon<T> OuterPolygon { get; }

        public List<Polygon<T>> InnerPolygons { get; }

        public PolygonGroup(Polygon<T>? outerPolygon, List<Polygon<T>>? innerPolygons = null)
        {
            if (outerPolygon != null) OuterPolygon = outerPolygon;
            else OuterPolygon = new Polygon<T>();

            if (innerPolygons != null) InnerPolygons = innerPolygons;
            else InnerPolygons = new List<Polygon<T>>();
        }
    }
}
