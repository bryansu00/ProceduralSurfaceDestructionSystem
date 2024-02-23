
using System.Numerics;

namespace PSDSystem
{
    public class Polygon
    {
        public PolygonVertex? HeadVertex { get; private set; }

        public uint Size { get; private set; }

        public List<Vector2>? Vertices { get; set; }

        public Polygon()
        {
            HeadVertex = null;
            Size = 0;
            Vertices = null;
        }
    }
}
