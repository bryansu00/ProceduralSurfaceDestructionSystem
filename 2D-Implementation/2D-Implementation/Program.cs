using Raylib_cs;
using PSDSystem;
using System.Numerics;
using System.Text;
using static PSDSystem.PSD;
using static PSDSystem.TestCases;

class Program
{
    public enum ViewMode
    {
        Surface = 0,
        Triangle
    }

    const int HEIGHT = 720;
    private static List<Color> colors = new List<Color>();

    // Test Settings
    static List<TestCaseDel<PolygonVertex>> TestCaseDelegates = [
        SquareTestCase<PolygonVertex>
        ];
    static int SelectedTestCase = 0;
    static ViewMode Mode = ViewMode.Surface;

    // Test Results
    static string TestName = "";
    static SurfaceShape<PolygonVertex>? Surface = null;
    static List<Polygon<PolygonVertex>>? Cutters = null;
    static CutSurfaceResult CutResult = CutSurfaceResult.UNKNOWN_ERROR;
    static List<int>? Triangles = null;
    static List<Vector2>? TrianglesVertices = null;
    static List<List<Vector2>>? ConvexGroups = null;
    static int TriangleCount = 0;
    static int ConvexGroupCount = 0;

    public static void Main()
    {
        Random rnd = new Random();
        for (int i = 0; i < 1000; i++)
        {
            colors.Add(new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255), 255));
        }

        LoadTestCase();

        Raylib.InitWindow(1280, HEIGHT, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {

            }

            // Reload
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                LoadTestCase();
            }
            // Run Test
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Surface != null && Cutters != null)
                {
                    foreach (Polygon<PolygonVertex> cutter in Cutters)
                        CutSurface<PolygonVertex, BooleanVertex>(Surface, cutter);
                }
                if (Surface != null)
                {
                    TriangulateSurface(Surface, out Triangles, out TrianglesVertices);
                    if (Triangles != null)
                        TriangleCount = Triangles.Count / 3;

                    ConvexGroups = FindConvexVerticesOfSurface(Surface);
                    if (ConvexGroups != null)
                        ConvexGroupCount = ConvexGroups.Count;
                }
            }

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            // Draw Test Case
            switch (Mode)
            {
                case ViewMode.Surface:
                    if (Surface != null)
                    {
                        DrawSurface(Surface, true);
                    }
                    break;
                default:
                    break;
            }

            // Draw Info Container
            Raylib.DrawRectangle(20, 20, 400, 300, Color.Gray);

            Raylib.DrawText(string.Format("CurrentTest: {0}", TestName), 30, 30, 18, Color.Black);
            Raylib.DrawText(string.Format("CutResult: {0}", CutResult), 30, 50, 18, Color.Black);
            Raylib.DrawText(string.Format("TriangleCount: {0}", TriangleCount), 30, 70, 18, Color.Black);
            Raylib.DrawText(string.Format("ConvexGroupCount: {0}", ConvexGroupCount), 30, 90, 18, Color.Black);

            Raylib.DrawText(string.Format("Selected Test Case: {0}", SelectedTestCase), 30, 260, 18, Color.Black);
            Raylib.DrawText("V - Change View | R - Reload", 30, 280, 18, Color.Black);
            Raylib.DrawText("Space - Run Test", 30, 300, 18, Color.Black);
            // End Info Container

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void LoadTestCase()
    {
        TestName = "None";
        Surface = null;
        Cutters = null;
        CutResult = CutSurfaceResult.UNKNOWN_ERROR;
        TriangleCount = 0;
        ConvexGroupCount = 0;

        if (SelectedTestCase < 0 || SelectedTestCase >= TestCaseDelegates.Count)
        {
            return;
        }

        TestCaseDelegates[SelectedTestCase](out TestName, out Surface, out Cutters);
    }

    static void DrawTriangulation(List<int> triangulation, List<Vector2> vertices)
    {
        for (int i = 0; i < triangulation.Count; i += 3)
        {
            Vector2 a = FlipY(vertices[triangulation[i+2]]);
            Vector2 b = FlipY(vertices[triangulation[i+1]]);
            Vector2 c = FlipY(vertices[triangulation[i]]);
            Raylib.DrawTriangle(a, b, c, colors[i]);
        }
    }

    static void DrawPolygon<T>(Polygon<T> polygon, Color color, bool labelVerts = true, bool labelOnLeft = false) where T : PolygonVertex
    {
        if (polygon.Head == null || polygon.Vertices == null) return;

        var now = polygon.Head;
        do
        {
            Vector2 toUse = FlipY(polygon.Vertices[now.Data.Index]);
            Vector2 toUseNext = FlipY(polygon.Vertices[now.Next.Data.Index]);

            Raylib.DrawLineEx(toUse, toUseNext, 2.0f, color);
            now = now.Next;
        } while (now != polygon.Head);

        if (labelVerts)
        {
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 toUse = FlipY(polygon.Vertices[i]);
                Raylib.DrawCircleV(toUse, 5.0f, color);
                if (labelOnLeft) Raylib.DrawText(i.ToString(), (int)toUse.X - 6, (int)toUse.Y + 6, 12, color);
                else Raylib.DrawText(i.ToString(), (int)toUse.X + 5, (int)toUse.Y + 5, 12, color);
            }
        }
    }

    static void DrawSurface<T>(SurfaceShape<T> surface, bool labelVerts = true) where T : PolygonVertex
    {
        foreach (PolygonGroup<T> group in surface.Polygons)
        {
            DrawPolygon(group.OuterPolygon, Color.Blue, labelVerts, false);
            
            foreach (Polygon<T> polygon in group.InnerPolygons)
            {
                DrawPolygon(polygon, Color.Red, labelVerts, true);
            }
        }
    }

    static void PrintBooleanList(Polygon<BooleanVertex> polygon)
    {
        if (polygon.Head == null) return;

        StringBuilder sb = new StringBuilder();
        VertexNode<BooleanVertex> now = polygon.Head;
        do
        {
            sb.Append(now.Data.Index);

            sb.Append(", Outside: ");
            sb.Append(now.Data.IsOutside);

            sb.Append(", Cross: ");
            if (now.Data.Cross != null) sb.Append(now.Data.Cross.Data.Index);
            else sb.Append("None");

            sb.Append("\n");

            now = now.Next;
        } while (now != polygon.Head);

        Console.Write(sb.ToString());
    }

    //static void InitShape()
    //{
    //    surface = new SurfaceShape<PolygonVertex>();

    //    List<Vector2> Vertices = [
    //        new Vector2(150.0f, 150.0f),
    //        new Vector2(150.0f, 550.0f),
    //        new Vector2(1050.0f, 550.0f),
    //        new Vector2(1050.0f, 150.0f)
    //    ];

    //    //List<Vector2> Vertices = [
    //    //    new Vector2(130.0f, 250.0f),
    //    //    new Vector2(0.0f, 250.0f),
    //    //    new Vector2(65.0f, 350.0f),
    //    //    new Vector2(0.0f, 450.0f),
    //    //    new Vector2(130.0f, 450.0f),
    //    //    new Vector2(200.0f, 550.0f),
    //    //    new Vector2(270.0f, 450.0f),
    //    //    new Vector2(400.0f, 450.0f),
    //    //    new Vector2(335.0f, 350.0f),
    //    //    new Vector2(400.0f, 250.0f),
    //    //    new Vector2(270.0f, 250.0f),
    //    //    new Vector2(200.0f, 150.0f),
    //    //];

    //    Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>();
    //    polygon.Vertices = Vertices;
    //    polygon.InsertVertexAtBack(0);
    //    polygon.InsertVertexAtBack(1);
    //    polygon.InsertVertexAtBack(2);
    //    polygon.InsertVertexAtBack(3);
    //    //polygon.InsertVertexAtBack(4);
    //    //polygon.InsertVertexAtBack(5);
    //    //polygon.InsertVertexAtBack(6);
    //    //polygon.InsertVertexAtBack(7);
    //    //polygon.InsertVertexAtBack(8);
    //    //polygon.InsertVertexAtBack(9);
    //    //polygon.InsertVertexAtBack(10);
    //    //polygon.InsertVertexAtBack(11);

    //    surface.AddOuterPolygon(polygon);
    //}

    //static void LoadTest1()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(250.0f, 150.0f),
    //        new Vector2(250.0f, 50.0f),
    //        new Vector2(550.0f, 50.0f),
    //        new Vector2(550.0f, 250.0f),
    //        new Vector2(450.0f, 250.0f),
    //        new Vector2(450.0f, 150.0f)
    //        ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);
    //    cutter.InsertVertexAtBack(3);
    //    cutter.InsertVertexAtBack(4);
    //    cutter.InsertVertexAtBack(5);

    //    Console.WriteLine("Loaded test1");
    //}

    //static void LoadTest2()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(250.0f, 150.0f),
    //        new Vector2(250.0f, 50.0f),
    //        new Vector2(550.0f, 50.0f),
    //        new Vector2(550.0f, 150.0f),
    //        ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);
    //    cutter.InsertVertexAtBack(3);

    //    Console.WriteLine("Loaded test2");
    //}

    //static void LoadTest3()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(550.0f, 150.0f),
    //        new Vector2(550.0f, 50.0f),
    //        new Vector2(250.0f, 50.0f),
    //    ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);

    //    Console.WriteLine("Loaded test3");
    //}

    //static void LoadTest4()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(550.0f, 150.0f),
    //        new Vector2(550.0f, 250.0f),
    //        new Vector2(250.0f, 250.0f),
    //    ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);

    //    Console.WriteLine("Loaded test4");
    //}

    //static void LoadTest5()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(150.0f, 150.0f),
    //        new Vector2(150.0f, 550.0f),
    //        new Vector2(550.0f, 550.0f),
    //        new Vector2(550.0f, 150.0f)
    //    ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);
    //    cutter.InsertVertexAtBack(3);

    //    Console.WriteLine("Loaded test5");
    //}

    //static void LoadTest6()
    //{
    //    InitShape();
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(150.0f, 150.0f),
    //        new Vector2(150.0f, 250.0f),
    //        new Vector2(550.0f, 250.0f),
    //        new Vector2(550.0f, 150.0f)
    //    ];
    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);
    //    cutter.InsertVertexAtBack(3);

    //    Console.WriteLine("Loaded test6");
    //}

    //static void InsertCircle(Vector2 center, float scale = 1.0f)
    //{
    //    cutter = new Polygon<PolygonVertex>();
    //    cutter.Vertices = [
    //        new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
    //        new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
    //        new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
    //        new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
    //        new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
    //        new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
    //        new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
    //        new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
    //    ];

    //    cutter.InsertVertexAtBack(0);
    //    cutter.InsertVertexAtBack(1);
    //    cutter.InsertVertexAtBack(2);
    //    cutter.InsertVertexAtBack(3);
    //    cutter.InsertVertexAtBack(4);
    //    cutter.InsertVertexAtBack(5);
    //    cutter.InsertVertexAtBack(6);
    //    cutter.InsertVertexAtBack(7);
    //}

    static Vector2 FlipY(Vector2 vector)
    {
        return new Vector2(vector.X, HEIGHT - vector.Y);
    }
}