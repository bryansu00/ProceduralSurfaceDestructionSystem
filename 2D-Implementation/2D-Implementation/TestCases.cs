
using System.Numerics;
using static PSDSystem.PSD;
using static System.Formats.Asn1.AsnWriter;

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

        public static void SquareTestCase1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Vertices and Edge Overlap";
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

        public static void SquareTestCase2<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            cutter = null;

            testName = "Surface with Single Hole";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            List<Vector2> innerVertices = [
                new Vector2(950.0f, 550.0f),
                new Vector2(650.0f, 550.0f),
                new Vector2(650.0f, 250.0f),
                new Vector2(950.0f, 250.0f)
            ];
            Polygon<T> innerPolygon = new Polygon<T>();
            innerPolygon.Vertices = innerVertices;
            innerPolygon.InsertVertexAtBack(0);
            innerPolygon.InsertVertexAtBack(1);
            innerPolygon.InsertVertexAtBack(2);
            innerPolygon.InsertVertexAtBack(3);

            surface.Polygons[0].InnerPolygons.Add(innerPolygon);
        }

        public static void SquareTestCase3<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
             where T : PolygonVertex
        {
            testName = "Surface with Single Hole and a Cutter";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            List<Vector2> innerVertices = [
                new Vector2(950.0f, 550.0f),
                new Vector2(650.0f, 550.0f),
                new Vector2(650.0f, 250.0f),
                new Vector2(950.0f, 250.0f)
            ];
            Polygon<T> innerPolygon = new Polygon<T>();
            innerPolygon.Vertices = innerVertices;
            innerPolygon.InsertVertexAtBack(0);
            innerPolygon.InsertVertexAtBack(1);
            innerPolygon.InsertVertexAtBack(2);
            innerPolygon.InsertVertexAtBack(3);

            surface.Polygons[0].InnerPolygons.Add(innerPolygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 350.0f),
                new Vector2(750.0f, 50.0f),
                new Vector2(850.0f, 50.0f),
                new Vector2(850.0f, 350.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void SquareTestCase4<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
             where T : PolygonVertex
        {
            testName = "Surface with Single Hole and Long Cutter";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            List<Vector2> innerVertices = [
                new Vector2(950.0f, 550.0f),
                new Vector2(650.0f, 550.0f),
                new Vector2(650.0f, 250.0f),
                new Vector2(950.0f, 250.0f)
            ];
            Polygon<T> innerPolygon = new Polygon<T>();
            innerPolygon.Vertices = innerVertices;
            innerPolygon.InsertVertexAtBack(0);
            innerPolygon.InsertVertexAtBack(1);
            innerPolygon.InsertVertexAtBack(2);
            innerPolygon.InsertVertexAtBack(3);

            surface.Polygons[0].InnerPolygons.Add(innerPolygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 570.0f),
                new Vector2(750.0f, 50.0f),
                new Vector2(850.0f, 50.0f),
                new Vector2(850.0f, 570.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void SquareTestCase5<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Surface with Single Hole and Very Long Cutter";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            List<Vector2> innerVertices = [
                new Vector2(950.0f, 550.0f),
                new Vector2(650.0f, 550.0f),
                new Vector2(650.0f, 250.0f),
                new Vector2(950.0f, 250.0f)
            ];
            Polygon<T> innerPolygon = new Polygon<T>();
            innerPolygon.Vertices = innerVertices;
            innerPolygon.InsertVertexAtBack(0);
            innerPolygon.InsertVertexAtBack(1);
            innerPolygon.InsertVertexAtBack(2);
            innerPolygon.InsertVertexAtBack(3);

            surface.Polygons[0].InnerPolygons.Add(innerPolygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 700.0f),
                new Vector2(750.0f, 50.0f),
                new Vector2(850.0f, 50.0f),
                new Vector2(850.0f, 700.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void SquareTestCase6<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Surface with Square Cutter Inside Overlapping Edge";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 450.0f),
                new Vector2(750.0f, 150.0f),
                new Vector2(850.0f, 150.0f),
                new Vector2(850.0f, 450.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void SquareTestCase7<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Surface with Triangle Cutter Inside - Vertex Touches Edge";
            surface = new SurfaceShape<T>();
            List<Vector2> outerVertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 650.0f),
                new Vector2(1050.0f, 650.0f),
                new Vector2(1050.0f, 150.0f)
            ];
            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);

            surface.AddOuterPolygon(polygon);

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(750.0f, 450.0f),
                new Vector2(750.0f, 150.0f),
                new Vector2(850.0f, 450.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void OctagonTestCase<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Octagon Test";
            surface = new SurfaceShape<T>();
            cutter = null;

            float scale = 20.0f;
            Vector2 center = new Vector2(775.0f, 400.0f);
            List<Vector2> Vertices = [
                new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y)
            ];

            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = Vertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);
            polygon.InsertVertexAtBack(4);
            polygon.InsertVertexAtBack(5);
            polygon.InsertVertexAtBack(6);
            polygon.InsertVertexAtBack(7);

            surface.AddOuterPolygon(polygon);

            return;
        }

        public static void OctagonTestCase1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter)
            where T : PolygonVertex
        {
            testName = "Octagon Test With Hole";
            surface = new SurfaceShape<T>();

            float scale = 20.0f;
            Vector2 center = new Vector2(775.0f, 400.0f);
            List<Vector2> outerVertices = [
                new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y)
            ];

            Polygon<T> polygon = new Polygon<T>();
            polygon.Vertices = outerVertices;
            polygon.InsertVertexAtBack(0);
            polygon.InsertVertexAtBack(1);
            polygon.InsertVertexAtBack(2);
            polygon.InsertVertexAtBack(3);
            polygon.InsertVertexAtBack(4);
            polygon.InsertVertexAtBack(5);
            polygon.InsertVertexAtBack(6);
            polygon.InsertVertexAtBack(7);

            surface.AddOuterPolygon(polygon);

            scale = 10.0f;
            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
            cutter.InsertVertexAtBack(4);
            cutter.InsertVertexAtBack(5);
            cutter.InsertVertexAtBack(6);
            cutter.InsertVertexAtBack(7);

            return;
        }
    }
}
