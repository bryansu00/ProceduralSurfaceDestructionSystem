using Raylib_cs;
using PSDSystem;
using System.Numerics;
using System.IO;

class Program
{
    const int HEIGHT = 720;

    public static void Main()
    {
        Raylib.InitWindow(1280, HEIGHT, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        // Initialize stuff....
        SurfaceShape<PolygonVertex> surface = InitShape();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Grave))
            {
                Console.Write("~");
                string? input = Console.ReadLine();
                if (input != null)
                {

                }
            }

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            //DrawPolygon(polygon, Color.Black);

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

    public static SurfaceShape<PolygonVertex> InitShape()
    {
        SurfaceShape<PolygonVertex> surface = new SurfaceShape<PolygonVertex>();

        List<Vector2> Vertices = new List<Vector2>();
        Vertices.Add(new Vector2(150.0f, 150.0f));
        Vertices.Add(new Vector2(150.0f, 550.0f));
        Vertices.Add(new Vector2(1050.0f, 550.0f));
        Vertices.Add(new Vector2(1050.0f, 150.0f));

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>();
        polygon.Vertices = Vertices;
        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        //Console.WriteLine(String.Join(", ", polygon.ToList()));

        return surface;
    }

    public static Vector2 FlipY(Vector2 vector)
    {
        return new Vector2(vector.X, HEIGHT - vector.Y);
    }
}