using Raylib_cs;
using PSDSystem;
using System.Numerics;
using static PSDSystem.PSD;
using static PSDSystem.TestCases;
using System.Diagnostics.Metrics;

class Program
{
    public enum ViewMode
    {
        Surface = 0,
        Triangles,
        ConvexGroups,
        MAX
    }

    const int HEIGHT = 720;
    private static List<Color> colors = new List<Color>();

    // Test Settings
    static List<TestCaseDel<PolygonVertex>> TestCaseDelegates = [
        SquareTestCase<PolygonVertex>,
        SquareTestCase1<PolygonVertex>,
        SquareTestCase2<PolygonVertex>,
        SquareTestCase3<PolygonVertex>,
        SquareTestCase4<PolygonVertex>,
        SquareTestCase5<PolygonVertex>,
        SquareTestCase6<PolygonVertex>,
        SquareTestCase7<PolygonVertex>,
        SquareTestCase8<PolygonVertex>,
        SquareTestCase9<PolygonVertex>,
        SquareTestCase10<PolygonVertex>,
        SquareTestCase11<PolygonVertex>,
        SquareTestCase12<PolygonVertex>,
        SquareTestCase13<PolygonVertex>,
        SquareTestCase14<PolygonVertex>,
        SquareAndOctagon1<PolygonVertex>,
        SquareAndOctagon2<PolygonVertex>,
        SquareAndOctagon3<PolygonVertex>,
        OctagonTestCase<PolygonVertex>,
        OctagonTestCase1<PolygonVertex>,
        GodotFoundCase1<PolygonVertex>,
        GodotFoundCase2<PolygonVertex>
        ];
    static int SelectedTestCase = 0;
    static ViewMode Mode = ViewMode.Surface;
    static bool FreeCutting = false;

    // Test Results
    static string TestName = "";
    static SurfaceShape<PolygonVertex>? Surface = null;
    static Polygon<PolygonVertex>? Cutter = null;
    static Polygon<PolygonVertex>? AnchorPolygon = null;
    static CutSurfaceResult CutResult = CutSurfaceResult.UNKNOWN_ERROR;
    static List<int>? Triangles = null;
    static List<Vector2>? TrianglesVertices = null;
    static List<List<Vector2>>? ConvexGroups = null;
    static int TriangleCount = 0;
    static int SurfaceVerticesCount = 0;
    static int ConvexGroupCount = 0;
    static int PolygonGroupCount = 0;

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
            // Change View
            if (Raylib.IsKeyPressed(KeyboardKey.V))
            {
                Mode = (ViewMode)(((int)Mode + 1) % (int)ViewMode.MAX);
            }
            // Reload
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                LoadTestCase();
            }
            // Run Test
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Surface != null && Cutter != null)
                {
                     CutResult = CutSurface<PolygonVertex, BooleanVertex>(Surface, Cutter, AnchorPolygon);
                }
                if (Surface != null)
                {
                    PolygonGroupCount = Surface.Polygons.Count;

                    TriangulateSurface(Surface, out Triangles, out TrianglesVertices);
                    if (Triangles != null)
                        TriangleCount = Triangles.Count / 3;

                    ConvexGroups = FindConvexVerticesOfSurface(Surface);
                    if (ConvexGroups != null)
                        ConvexGroupCount = ConvexGroups.Count;

                    SurfaceVerticesCount = 0;
                    foreach (PolygonGroup<PolygonVertex> group in Surface.Polygons)
                    {
                        SurfaceVerticesCount += (int)group.OuterPolygon.Count;
                        //Console.Write("\nOuter Polygon Indices: ");
                        //Console.WriteLine(string.Join(", ", group.OuterPolygon.ToVerticesList()));
                        int innerPolygonCount = 0;
                        foreach (Polygon<PolygonVertex> polygon in group.InnerPolygons)
                        {
                            SurfaceVerticesCount += (int)polygon.Count;
                            //Console.Write("Inner Polygon {0} Indices: ", innerPolygonCount);
                            //Console.WriteLine(string.Join(", ", polygon.ToList()));
                            innerPolygonCount++;
                        }
                    }
                }
                
                Cutter = null;
            }

            // Toggle free circle mode
            if (Raylib.IsKeyPressed(KeyboardKey.F))
            {
                FreeCutting = !FreeCutting;
            }
            // Free cut circle
            if (FreeCutting && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                float scale = 5.0f;
                Vector2 center = FlipY(Raylib.GetMousePosition());
                Cutter = new Polygon<PolygonVertex>();
                Cutter.Vertices = [
                    new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
                    new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
                    new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
                    new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
                    new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
                    new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
                    new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
                    new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
                    ];
                Cutter.InsertVertexAtBack(0);
                Cutter.InsertVertexAtBack(1);
                Cutter.InsertVertexAtBack(2);
                Cutter.InsertVertexAtBack(3);
                Cutter.InsertVertexAtBack(4);
                Cutter.InsertVertexAtBack(5);
                Cutter.InsertVertexAtBack(6);
                Cutter.InsertVertexAtBack(7);

                if (Surface != null)
                {
                    CutResult = CutSurface<PolygonVertex, BooleanVertex>(Surface, Cutter, AnchorPolygon);

                    PolygonGroupCount = Surface.Polygons.Count;

                    TriangulateSurface(Surface, out Triangles, out TrianglesVertices);
                    if (Triangles != null)
                        TriangleCount = Triangles.Count / 3;

                    ConvexGroups = FindConvexVerticesOfSurface(Surface);
                    if (ConvexGroups != null)
                        ConvexGroupCount = ConvexGroups.Count;

                    SurfaceVerticesCount = 0;
                    foreach (PolygonGroup<PolygonVertex> group in Surface.Polygons)
                    {
                        SurfaceVerticesCount += (int)group.OuterPolygon.Count;
                        int innerPolygonCount = 0;
                        foreach (Polygon<PolygonVertex> polygon in group.InnerPolygons)
                        {
                            SurfaceVerticesCount += (int)polygon.Count;
                            innerPolygonCount++;
                        }
                    }
                }

                Cutter = null;
            }

            // Change Test
            if (Raylib.IsKeyPressed(KeyboardKey.Right))
            {
                SelectedTestCase = (SelectedTestCase + 1) % TestCaseDelegates.Count;
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                SelectedTestCase = SelectedTestCase - 1;
                if (SelectedTestCase == -1) SelectedTestCase = TestCaseDelegates.Count - 1;
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
                case ViewMode.Triangles:
                    if (Triangles != null && TrianglesVertices != null)
                    {
                        DrawTriangulation(Triangles, TrianglesVertices);
                    }
                    break;
                case ViewMode.ConvexGroups:
                    if (Surface != null)
                    {
                        DrawSurface(Surface, false);
                    }
                    if (ConvexGroups != null)
                    {
                        DrawConvexVertices(ConvexGroups);
                    }
                    break;
                default:
                    break;
            }
            if (Cutter != null)
            {
                DrawPolygon(Cutter, Color.Red, true, true);
            }

            // Draw Info Container
            Raylib.DrawRectangle(20, 20, 400, 300, Color.Gray);

            Raylib.DrawText(TestName, 30, 30, 18, Color.Black);
            Raylib.DrawText(string.Format("CutResult: {0}", CutResult), 30, 50, 18, Color.Black);
            Raylib.DrawText(string.Format("TriangleCount: {0}", TriangleCount), 30, 70, 18, Color.Black);
            Raylib.DrawText(string.Format("SurfaceVerticesCount: {0}", SurfaceVerticesCount), 30, 90, 18, Color.Black);
            Raylib.DrawText(string.Format("ConvexGroupCount: {0}", ConvexGroupCount), 30, 110, 18, Color.Black);
            Raylib.DrawText(string.Format("PolygonGroupCount: {0}", PolygonGroupCount), 30, 130, 18, Color.Black);

            Raylib.DrawText(string.Format("Free Cutting: {0}", FreeCutting), 30, 220, 18, Color.Black);
            Raylib.DrawText(string.Format("Current View: {0}", Mode), 30, 240, 18, Color.Black);
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
        Cutter = null;
        CutResult = CutSurfaceResult.UNKNOWN_ERROR;
        Triangles = null;
        AnchorPolygon = null;
        TrianglesVertices = null;
        ConvexGroups = null;
        TriangleCount = 0;
        SurfaceVerticesCount = 0;
        ConvexGroupCount = 0;
        PolygonGroupCount = 0;

        if (SelectedTestCase < 0 || SelectedTestCase >= TestCaseDelegates.Count)
        {
            return;
        }

        TestCaseDelegates[SelectedTestCase](out TestName, out Surface, out Cutter, out AnchorPolygon);
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

            Raylib.DrawLineEx(toUse, toUseNext, 3.0f, color);
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
                DrawPolygon(polygon, Color.Blue, labelVerts, false);
            }
        }
    }

    static void DrawConvexVertices(List<List<Vector2>> verticesGroup)
    {
        for (int i = 0; i < verticesGroup.Count; i++)
        {
            foreach (Vector2 v in verticesGroup[i])
            {
                Vector2 flippedV = FlipY(v);
                Raylib.DrawCircleV(flippedV, 5.0f, colors[i]);
                foreach (Vector2 v2 in verticesGroup[i])
                {
                    if (v == v2) continue;
                    Vector2 flippedV2 = FlipY(v2);
                    Raylib.DrawLineEx(flippedV, flippedV2, 5.0f, colors[i]);
                }
            }
        }
    }

    static Vector2 FlipY(Vector2 vector)
    {
        return new Vector2(vector.X, HEIGHT - vector.Y);
    }
}