
using System.Numerics;
using static PSDSystem.PSD;

namespace PSDSystem
{
    public static class TestCases
    {
        public delegate void TestCaseDel<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex;

        public static void SquareTestCase<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Square Test";
            surface = new SurfaceShape<T>();
            cutter = null;

            List<Vector2> Vertices = [
                new Vector2(150.0f, 150.0f),
                new Vector2(150.0f, 550.0f),
                new Vector2(550.0f, 550.0f),
                new Vector2(550.0f, 150.0f)
            ];

            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = Vertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            return;
        }
    }
}
