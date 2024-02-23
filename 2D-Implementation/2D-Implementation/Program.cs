using Raylib_cs;
using PSDSystem;
using System.Numerics;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 480, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}