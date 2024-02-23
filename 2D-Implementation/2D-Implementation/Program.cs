using Raylib_cs;
using PSDSystem;
using System.Numerics;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 480, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        // Initialize stuff....

        List<Vector2> Vertices = new List<Vector2>();
        Vertices.Add(new Vector2(1050.0f, 150.0f));
        Vertices.Add(new Vector2(150.0f, 150.0f));
        Vertices.Add(new Vector2(150.0f, 550.0f));
        Vertices.Add(new Vector2(1050.0f, 550.0f));

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>();
        polygon.Vertices = Vertices;
        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        Console.WriteLine(String.Join(", ", polygon.ToList()));

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}