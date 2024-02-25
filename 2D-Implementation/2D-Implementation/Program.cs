using Raylib_cs;
using PSDSystem;
using System.Numerics;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text;

class Program
{
    const int HEIGHT = 720;

    public static void Main()
    {
        Raylib.InitWindow(1280, HEIGHT, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        // Initialize stuff....
        SurfaceShape<PolygonVertex> surface = InitShape();

        Polygon<PolygonVertex> cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = new List<Vector2>();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                int verticesIdxAdded = cutter.Vertices.Count;
                Vector2 mousePos = FlipY(Raylib.GetMousePosition());
                cutter.Vertices.Add(mousePos);
                cutter.InsertVertexAtBack(verticesIdxAdded);
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                if (cutter.Vertices.Count > 0)
                {
                    cutter.Vertices.RemoveAt(cutter.Vertices.Count - 1);
                    int verticesIdxRemoved = cutter.Vertices.Count;
                    cutter.RemoveVerticesWithIndex(verticesIdxRemoved);
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Grave))
            {
                Console.Write("~");
                string? input = Console.ReadLine();
                if (input != null)
                {

                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                var res = PSD.IntersectCutterAndPolygon(cutter, surface.Polygons[0].OuterPolygon, out Polygon<BooleanVertex>? a, out Polygon<BooleanVertex>? b);
                if (a != null)
                {
                    Console.WriteLine("Cutter:");
                    PrintBooleanList(a);
                }
                if (b != null)
                {
                    Console.WriteLine("Other:");
                    PrintBooleanList(b);
                }
            }

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            foreach (PolygonGroup<PolygonVertex> group in surface.Polygons)
            {
                DrawPolygon(group.OuterPolygon, Color.Black);
            }
            DrawPolygon(cutter, Color.Red);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    public static void DrawPolygon<T>(Polygon<T> polygon, Color color, bool labelVerts = true) where T : PolygonVertex
    {
        if (polygon.Head == null || polygon.Vertices == null) return;

        var now = polygon.Head;
        do
        {
            Vector2 toUse = FlipY(polygon.Vertices[now.Data.Index]);
            Vector2 toUseNext = FlipY(polygon.Vertices[now.Next.Data.Index]);

            Raylib.DrawLineEx(toUse, toUseNext, 1.0f, color);
            Raylib.DrawCircleV(toUse, 5.0f, color);
            now = now.Next;
        } while (now != polygon.Head);

        if (labelVerts)
        {
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 toUse = FlipY(polygon.Vertices[i]);
                Raylib.DrawText(i.ToString(), (int)toUse.X + 5, (int)toUse.Y + 5, 12, color);
            }
        }
    }

    public static void PrintBooleanList(Polygon<BooleanVertex> polygon)
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

    public static SurfaceShape<PolygonVertex> InitShape()
    {
        SurfaceShape<PolygonVertex> surface = new SurfaceShape<PolygonVertex>();

        List<Vector2> Vertices = [
            new Vector2(150.0f, 150.0f),
            new Vector2(150.0f, 550.0f),
            new Vector2(1050.0f, 550.0f),
            new Vector2(1050.0f, 150.0f)
        ];

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>();
        polygon.Vertices = Vertices;
        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        //Console.WriteLine(String.Join(", ", polygon.ToList()));

        surface.AddOuterPolygon(polygon);

        return surface;
    }

    public static Vector2 FlipY(Vector2 vector)
    {
        return new Vector2(vector.X, HEIGHT - vector.Y);
    }
}