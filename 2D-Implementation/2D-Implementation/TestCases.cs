
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
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
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

        public static void TestCase1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "I don' know";
            surface = new SurfaceShape<T>();
            List<Vector2> Vertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = Vertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 150.0f),
                new Vector2(750.0f, 50.0f),
                new Vector2(950.0f, 50.0f),
                new Vector2(950.0f, 250.0f),
                new Vector2(850.0f, 250.0f),
                new Vector2(850.0f, 150.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
            cutter.InsertVertexAtBack(4);
            cutter.InsertVertexAtBack(5);
        }
    }
}
