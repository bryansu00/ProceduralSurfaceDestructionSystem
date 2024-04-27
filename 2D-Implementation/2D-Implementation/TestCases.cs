
using System.Numerics;
using static PSDSystem.PSD;
using static System.Formats.Asn1.AsnWriter;

namespace PSDSystem
{
    public static class TestCases
    {
        public delegate void TestCaseDel<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex;

        public static void SquareTestCase<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Square Test";
            surface = new SurfaceShape<T>();
            cutter = null;
            anchorVertices = null;

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

        public static void SquareTestCase1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Vertices and Edge Overlap";
            anchorVertices = null;
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

        public static void SquareTestCase2<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            cutter = null;
            anchorVertices = null;
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

        public static void SquareTestCase3<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
             where T : PolygonVertex
        {
            testName = "Surface with Single Hole and a Cutter";
            surface = new SurfaceShape<T>();
            anchorVertices = null;
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

        public static void SquareTestCase4<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
             where T : PolygonVertex
        {
            testName = "Surface with Single Hole and Long Cutter";
            anchorVertices = null;
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

        public static void SquareTestCase5<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Surface with Single Hole and Very Long Cutter";
            anchorVertices = null;
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

        public static void SquareTestCase6<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Surface with Square Cutter Inside Overlapping Edge";
            anchorVertices = null;
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

        public static void SquareTestCase7<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Surface with Triangle Cutter Inside - Vertex Touches Edge";
            anchorVertices = null;
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

        public static void SquareTestCase8<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Inside Triangle Vertex-Vertex Intersect";
            anchorVertices = null;
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
                new Vector2(550.0f, 150.0f),
                new Vector2(850.0f, 200.0f),
                new Vector2(600.0f, 450.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void SquareTestCase9<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Surface with Triangle Cutter Outside - Vertex Touches Edge";
            anchorVertices = null;
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
                new Vector2(850.0f, 50.0f),
                new Vector2(750.0f, 150.0f),
                new Vector2(750.0f, 50.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void SquareTestCase10<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Vertices Overlap with Edges Intersecting";
            anchorVertices = null;
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
                new Vector2(550.0f, 150.0f),
                new Vector2(850.0f, 140.0f),
                new Vector2(540.0f, 450.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void SquareTestCase11<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Vertices Overlap (From Outside)";
            anchorVertices = null;
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
                new Vector2(550.0f, 50.0f),
                new Vector2(550.0f, 150.0f),
                new Vector2(450.0f, 150.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void SquareTestCase12<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Corner Vertices and Edges Overlap";
            anchorVertices = null;
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
                new Vector2(550.0f, 150.0f),
                new Vector2(850.0f, 150.0f),
                new Vector2(550.0f, 450.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
        }

        public static void SquareTestCase13<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
             where T : PolygonVertex
        {
            testName = "Surface with Single Hole and a Cutter's Edge Overlaps";
            anchorVertices = null;
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
                new Vector2(750.0f, 250.0f),
                new Vector2(750.0f, 150.0f),
                new Vector2(850.0f, 150.0f),
                new Vector2(850.0f, 250.0f)
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void SquareTestCase14<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Vertices and Edge Overlap (From Inside)";
            anchorVertices = null;
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
                new Vector2(750.0f, 250.0f),
                new Vector2(750.0f, 150.0f),
                new Vector2(850.0f, 150.0f),
                new Vector2(850.0f, 50.0f),
                new Vector2(950.0f, 50.0f),
                new Vector2(950.0f, 250.0f),
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
            cutter.InsertVertexAtBack(4);
            cutter.InsertVertexAtBack(5);
        }

        public static void SquareAndOctagon1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Square With Octagon Cutter";
            anchorVertices = null;
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

            float scale = 10.0f;
            Vector2 center = new Vector2(550.0f, 400.0f);
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
        }

        public static void SquareAndOctagon2<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Square With Octagon Cutter (Bottom)";
            anchorVertices = null;
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

            float scale = 10.0f;
            Vector2 center = new Vector2(550.0f, 200.0f);
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
        }

        public static void SquareAndOctagon3<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Square With Octagon Cutter (Bottom)";
            anchorVertices = null;
            surface = new SurfaceShape<T>();
            List<Vector2> Vertices = [
                new Vector2(550.0f, 150.0f),
                new Vector2(550.0f, 400.0f),
                new Vector2(620.7f, 429.3f),
                new Vector2(650.0f, 500.0f),
                new Vector2(620.7f, 570.7f),
                new Vector2(550.0f, 600.0f),
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
            polygon.InsertVertexAtBack(4);
            polygon.InsertVertexAtBack(5);
            polygon.InsertVertexAtBack(6);
            polygon.InsertVertexAtBack(7);
            polygon.InsertVertexAtBack(8);

            surface.AddOuterPolygon(polygon);

            float scale = 10.0f;
            Vector2 center = new Vector2(550.0f, 270.0f);
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
        }

        public static void GodotFoundCase1<T>(out string testName, out SurfaceShape<T> ? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Infinite Loop From Godot";
            anchorVertices = null;
            surface = new SurfaceShape<T>();
            List<Vector2> Vertices = [
                new Vector2(550, 150),
                new Vector2(550, 250),
                new Vector2(585.35f, 264.65f),
                new Vector2(600, 300),
                new Vector2(585.35f, 335.35f),
                new Vector2(550, 350),
                new Vector2(550, 420),
                new Vector2(585.35f, 434.65f),
                new Vector2(600, 470),
                new Vector2(585.35f, 505.35f),
                new Vector2(550, 520),
                new Vector2(550, 650),
                new Vector2(1050, 650),
                new Vector2(1050, 150)
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
            polygon.InsertVertexAtBack(8);
            polygon.InsertVertexAtBack(9);
            polygon.InsertVertexAtBack(10);
            polygon.InsertVertexAtBack(11);
            polygon.InsertVertexAtBack(12);
            polygon.InsertVertexAtBack(13);

            surface.AddOuterPolygon(polygon);

            //float scale = 5.0f;
            //Vector2 center = new Vector2(550.0f, 300.0f);
            //cutter = new Polygon<T>();
            //cutter.Vertices = [
            //    new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
            //    new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
            //    new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
            //    new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
            //    new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
            //    new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
            //    new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
            //    new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
            //    ];
            //cutter.InsertVertexAtBack(0);
            //cutter.InsertVertexAtBack(1);
            //cutter.InsertVertexAtBack(2);
            //cutter.InsertVertexAtBack(3);
            //cutter.InsertVertexAtBack(4);
            //cutter.InsertVertexAtBack(5);
            //cutter.InsertVertexAtBack(6);
            //cutter.InsertVertexAtBack(7);

            //float scale = 5.0f;
            //Vector2 center = new Vector2(550.0f, 450.0f);
            //cutter = new Polygon<T>();
            //cutter.Vertices = [
            //    new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
            //    new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
            //    new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
            //    new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
            //    new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
            //    new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
            //    new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
            //    new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
            //    ];
            //cutter.InsertVertexAtBack(0);
            //cutter.InsertVertexAtBack(1);
            //cutter.InsertVertexAtBack(2);
            //cutter.InsertVertexAtBack(3);
            //cutter.InsertVertexAtBack(4);
            //cutter.InsertVertexAtBack(5);
            //cutter.InsertVertexAtBack(6);
            //cutter.InsertVertexAtBack(7);

            float scale = 5.0f;
            Vector2 center = new Vector2(550.0f, 380.0f);
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
        }

        public static void GodotFoundCase2<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Simplified Infinite Loop From Godot";
            anchorVertices = null;
            surface = new SurfaceShape<T>();
            List<Vector2> Vertices = [
                new Vector2(550, 150),
                new Vector2(585.35f, 335.35f),
                new Vector2(550, 350),
                new Vector2(550, 420),
                new Vector2(585.35f, 434.65f),
                new Vector2(550, 650),
                new Vector2(1050, 650),
                new Vector2(1050, 150)
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

            cutter = new Polygon<T>();
            cutter.Vertices = [
                new Vector2(600, 380),
                new Vector2(550, 430),
                new Vector2(500, 380),
                new Vector2(550, 330),
                ];
            cutter.InsertVertexAtBack(0);
            cutter.InsertVertexAtBack(1);
            cutter.InsertVertexAtBack(2);
            cutter.InsertVertexAtBack(3);
        }

        public static void OctagonTestCase<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Octagon Test";
            surface = new SurfaceShape<T>();
            cutter = null;
            anchorVertices = null;
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

        public static void OctagonTestCase1<T>(out string testName, out SurfaceShape<T>? surface, out Polygon<T>? cutter, out Polygon<T>? anchorVertices)
            where T : PolygonVertex
        {
            testName = "Octagon Test With Hole";
            surface = new SurfaceShape<T>();
            anchorVertices = null;
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