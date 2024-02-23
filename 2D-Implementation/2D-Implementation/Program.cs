using Raylib_cs;
using PSDSystem;
using System.Numerics;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(1280, 720, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        // Initialize stuff....

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

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            DrawPolygon(polygon, Color.Black);

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
            Raylib.DrawLineEx(polygon.Vertices[now.Data.Index], polygon.Vertices[now.Next.Data.Index], 1.0f, color);
            Raylib.DrawCircleV(polygon.Vertices[now.Data.Index], 5.0f, color);
            now = now.Next;
        } while (now != polygon.Head);

        if (labelVerts)
        {
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Raylib.DrawText(i.ToString(), (int)polygon.Vertices[i].X + 5, (int)polygon.Vertices[i].Y + 5, 12, color);
            }
        }
    }
}