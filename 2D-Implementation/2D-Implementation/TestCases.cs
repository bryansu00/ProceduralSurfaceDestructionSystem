
using static PSDSystem.PSD;

namespace PSDSystem
{
    public static class TestCases
    {
        public static void SquareTestCase<T>(out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            surface = new SurfaceShape<T>();
            cutter = null;
        }
    }
}
