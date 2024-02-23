
namespace PSDSystem
{
    public class PolygonVertex
    {
        public int Center { get; }

        public Polygon Owner { get; }

        public PolygonVertex Next { get; set; }

        public PolygonVertex Previous { get; set; }

        public PolygonVertex(Polygon owner, int center)
        {
            Owner = owner;
            Center = center;
            Next = this;
            Previous = this;
        }
    }
}
