using Raylib_cs;
using PSDSystem;
using System.Numerics;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Security.Cryptography.X509Certificates;

class Program
{
    const int HEIGHT = 720;

    private static Polygon<PolygonVertex> cutter = new Polygon<PolygonVertex>();

    // Initialize stuff....
    private static SurfaceShape<PolygonVertex> surface = new SurfaceShape<PolygonVertex>();

    public static void Main()
    {
        Raylib.InitWindow(1280, HEIGHT, "2D Surface Destruction Testing");
        Raylib.SetTargetFPS(60);

        InitShape();
        InitCutter();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mousePos = FlipY(Raylib.GetMousePosition());
                //int verticesIdxAdded = cutter.Vertices.Count;
                //cutter.Vertices.Add(mousePos);
                //cutter.InsertVertexAtBack(verticesIdxAdded);

                InsertCircle(mousePos, 5.0f);
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
                    string[] args = input.Split();

                    if (args.Length > 0)
                    {
                        if (args[0].Equals("test0"))
                        {
                            InitShape();
                            InitCutter();
                            Console.WriteLine("Loaded test0");
                        }
                        else if (args[0].Equals("test1")) LoadTest1();
                        else if (args[0].Equals("test2")) LoadTest2();
                        else if (args[0].Equals("test3")) LoadTest3();
                        else if (args[0].Equals("test4")) LoadTest4();
                        else if (args[0].Equals("test5")) LoadTest5();
                        else if (args[0].Equals("test6")) LoadTest6();
                    }
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                HandlePolygonProcessing();
            }

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            foreach (PolygonGroup<PolygonVertex> group in surface.Polygons)
            {
                DrawPolygon(group.OuterPolygon, Color.Black);
                foreach (Polygon<PolygonVertex> inner in group.InnerPolygons)
                {
                    DrawPolygon(inner, Color.Blue, false);
                }
            }
            DrawPolygon(cutter, Color.Red, true, true);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void DrawPolygon<T>(Polygon<T> polygon, Color color, bool labelVerts = true, bool labelOnLeft = false) where T : PolygonVertex
    {
        if (polygon.Head == null || polygon.Vertices == null) return;

        var now = polygon.Head;
        do
        {
            Vector2 toUse = FlipY(polygon.Vertices[now.Data.Index]);
            Vector2 toUseNext = FlipY(polygon.Vertices[now.Next.Data.Index]);

            Raylib.DrawLineEx(toUse, toUseNext, 1.0f, color);
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

    static void HandlePolygonProcessing()
    {
        // Perform intersection test with cutter against all outer polygons
        PolygonGroup<PolygonVertex>? groupCutterIsIn = null;

        List<Tuple<Polygon<BooleanVertex>,PolygonGroup<PolygonVertex>, IntersectionPoints<BooleanVertex>>> intersected = new List<Tuple<Polygon<BooleanVertex>, PolygonGroup<PolygonVertex>, IntersectionPoints<BooleanVertex>>>();
        List<PolygonGroup<PolygonVertex>> groupsToKeep = new List<PolygonGroup<PolygonVertex>>();
        foreach (PolygonGroup<PolygonVertex> polygonGroup in surface.Polygons)
        {
            Polygon<BooleanVertex> booleanCutter = PSD.ConvertPolygonToBooleanList<PolygonVertex, BooleanVertex>(cutter);
            Polygon<BooleanVertex> booleanPolygon = PSD.ConvertPolygonToBooleanList<PolygonVertex, BooleanVertex>(polygonGroup.OuterPolygon);
            var res = PSD.IntersectCutterAndPolygon(booleanCutter, booleanPolygon, out IntersectionPoints<BooleanVertex>? intersectionResults);

            if (res == PSD.IntersectionResult.CUTTER_IS_INSIDE)
            {
                // Cutter is completely inside of an outer polygon
                groupCutterIsIn = polygonGroup;
                break;
            }

            switch(res)
            {
                case PSD.IntersectionResult.POLYGON_IS_INSIDE:
                    continue;
                case PSD.IntersectionResult.BOTH_OUTSIDE:
                    // Keep the polygons
                    groupsToKeep.Add(polygonGroup);
                    break;
                case PSD.IntersectionResult.INTERSECTS:
                    intersected.Add(new Tuple<Polygon<BooleanVertex>, PolygonGroup<PolygonVertex>, IntersectionPoints<BooleanVertex>>(booleanCutter, polygonGroup, intersectionResults));
                    break;
                default:
                    break;
            }
        }

        if (groupCutterIsIn != null)
        {
            Console.WriteLine("Case 1 Detected");

            Polygon<BooleanVertex> booleanCutter = PSD.ConvertPolygonToBooleanList<PolygonVertex, BooleanVertex>(cutter);
            List<Polygon<PolygonVertex>> newInnerPolygonsList = new List<Polygon<PolygonVertex>>();
            List<Polygon<BooleanVertex>> polygonsToCombineWith = new List<Polygon<BooleanVertex>>();
            List<IntersectionPoints<BooleanVertex>> intersections = new List<IntersectionPoints<BooleanVertex>>();
            foreach (Polygon<PolygonVertex> inner in groupCutterIsIn.InnerPolygons)
            {
                Polygon<BooleanVertex> booleanPolygon = PSD.ConvertPolygonToBooleanList<PolygonVertex, BooleanVertex>(inner);
                var res = PSD.IntersectCutterAndPolygon(booleanCutter, booleanPolygon, out IntersectionPoints<BooleanVertex>? intersectionResults);

                if (res == PSD.IntersectionResult.CUTTER_IS_INSIDE)
                {
                    // Cutter is completely inside an inner polygon
                    // End the loop
                    return;
                }

                switch (res)
                {
                    case PSD.IntersectionResult.POLYGON_IS_INSIDE:
                        continue;
                    case PSD.IntersectionResult.BOTH_OUTSIDE:
                        // Keep the polygon
                        newInnerPolygonsList.Add(inner);
                        break;
                    case PSD.IntersectionResult.INTERSECTS:
                        polygonsToCombineWith.Add(booleanPolygon);
                        intersections.Add(intersectionResults);
                        break;
                    default:
                        break;
                }
            }

            if (intersections.Count == 0)
            {
                // No intersection was found, cutter is a new inner
                groupCutterIsIn.InnerPolygons.Add(cutter);
                Console.WriteLine("Case 1 End");
                return;
            }

            groupCutterIsIn.InnerPolygons = newInnerPolygonsList;

            // Perform polygon addition operations
            List<Polygon<PolygonVertex>> polygonsProduced = PSD.CombinePolygons<PolygonVertex, BooleanVertex>(booleanCutter, intersections);
            if (polygonsProduced.Count == 1)
            {
                // Only 1 polygon was produced
                groupCutterIsIn.InnerPolygons.Add(polygonsProduced[0]);
                Console.WriteLine("Case 1 Ended with 1 polygon produced");
            }
            else
            {
                // NOTE: Some optimization can be done in the AddPolygons() function side

                // Multiple polygons was produced
                // Find the polygon that is on the outside
                int outsidePolygonIndex = 0;
                bool outsidePolygonFound = false;
                while (!outsidePolygonFound)
                {
                    outsidePolygonFound = true;
                    foreach (Polygon<PolygonVertex> polygon in polygonsProduced)
                    {
                        if (polygon == polygonsProduced[outsidePolygonIndex]) continue;

                        Polygon<PolygonVertex> polygonBeingObserved = polygonsProduced[outsidePolygonIndex];

                        if (PSD.PointIsInsidePolygon(polygonBeingObserved.Vertices[polygonBeingObserved.Head.Data.Index], polygon) != -1)
                        {
                            outsidePolygonFound = false;
                            break;
                        }
                    }

                    if (!outsidePolygonFound)
                    {
                        outsidePolygonIndex++;
                    }
                }

                groupCutterIsIn.InnerPolygons.Add(polygonsProduced[outsidePolygonIndex]);

                Console.WriteLine("Case 1.1 Ended with {0} polygons produced from CombinePolygons()", polygonsProduced.Count);
            }

            return;
        }
        else if (intersected.Count == 0)
        {
            return;
        }

        Console.WriteLine("Case 2 Detected");

        foreach (Tuple<Polygon<BooleanVertex>, PolygonGroup<PolygonVertex>, IntersectionPoints<BooleanVertex>> tuple in intersected)
        {
            Polygon<BooleanVertex> booleanCutter = tuple.Item1;
            PolygonGroup<PolygonVertex> group = tuple.Item2;
            IntersectionPoints<BooleanVertex> outerIntersections = tuple.Item3;

            List<Polygon<PolygonVertex>> nonIntersectedInnerPolygonsList = new List<Polygon<PolygonVertex>>();
            List<Polygon<BooleanVertex>> polygonsToCombineWith = new List<Polygon<BooleanVertex>>();
            List<IntersectionPoints<BooleanVertex>> innerIntersections = new List<IntersectionPoints<BooleanVertex>>();
            foreach (Polygon<PolygonVertex> inner in group.InnerPolygons)
            {
                Polygon<BooleanVertex> booleanPolygon = PSD.ConvertPolygonToBooleanList<PolygonVertex, BooleanVertex>(inner);
                var res = PSD.IntersectCutterAndPolygon(booleanCutter, booleanPolygon, out IntersectionPoints<BooleanVertex>? intersectionResults);

                switch (res)
                {
                    case PSD.IntersectionResult.POLYGON_IS_INSIDE:
                        continue;
                    case PSD.IntersectionResult.BOTH_OUTSIDE:
                        // Keep the polygon
                        nonIntersectedInnerPolygonsList.Add(inner);
                        break;
                    case PSD.IntersectionResult.INTERSECTS:
                        polygonsToCombineWith.Add(booleanPolygon);
                        innerIntersections.Add(intersectionResults);
                        break;
                    default:
                        break;
                }
            }

            List<Polygon<PolygonVertex>> polygonsProduced = PSD.CombinePolygons<PolygonVertex, BooleanVertex>(booleanCutter, outerIntersections, innerIntersections);
            if (polygonsProduced.Count == 1)
            {
                groupsToKeep.Add(new PolygonGroup<PolygonVertex>(group.OuterPolygon, nonIntersectedInnerPolygonsList));

                PrintBooleanList(booleanCutter);
                Console.WriteLine();
                PrintBooleanList(outerIntersections.Polygon);
                Console.WriteLine();

                //groupsToKeep.Add(new PolygonGroup<PolygonVertex>(polygonsProduced[0], nonIntersectedInnerPolygonsList));
                Console.WriteLine("Case 2 has replaced an outer polygon", polygonsProduced.Count);
            }
            else
            {
                Console.WriteLine("Case 2 has CombinePolygons() produced {0} outer polygons", polygonsProduced.Count);
            }
        }

        surface = new SurfaceShape<PolygonVertex>(groupsToKeep);
    }

    static void InitCutter()
    {
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = new List<Vector2>();
    }

    static void InitShape()
    {
        surface = new SurfaceShape<PolygonVertex>();

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
    }

    static void LoadTest1()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(250.0f, 150.0f),
            new Vector2(250.0f, 50.0f),
            new Vector2(550.0f, 50.0f),
            new Vector2(550.0f, 250.0f),
            new Vector2(450.0f, 250.0f),
            new Vector2(450.0f, 150.0f)
            ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);
        cutter.InsertVertexAtBack(3);
        cutter.InsertVertexAtBack(4);
        cutter.InsertVertexAtBack(5);

        Console.WriteLine("Loaded test1");
    }

    static void LoadTest2()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(250.0f, 150.0f),
            new Vector2(250.0f, 50.0f),
            new Vector2(550.0f, 50.0f),
            new Vector2(550.0f, 150.0f),
            ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);
        cutter.InsertVertexAtBack(3);

        Console.WriteLine("Loaded test2");
    }

    static void LoadTest3()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(550.0f, 150.0f),
            new Vector2(550.0f, 50.0f),
            new Vector2(250.0f, 50.0f),
        ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);

        Console.WriteLine("Loaded test3");
    }

    static void LoadTest4()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(550.0f, 150.0f),
            new Vector2(550.0f, 250.0f),
            new Vector2(250.0f, 250.0f),
        ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);

        Console.WriteLine("Loaded test4");
    }

    static void LoadTest5()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(150.0f, 150.0f),
            new Vector2(150.0f, 550.0f),
            new Vector2(550.0f, 550.0f),
            new Vector2(550.0f, 150.0f)
        ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);
        cutter.InsertVertexAtBack(3);

        Console.WriteLine("Loaded test5");
    }

    static void LoadTest6()
    {
        InitShape();
        cutter = new Polygon<PolygonVertex>();
        cutter.Vertices = [
            new Vector2(150.0f, 150.0f),
            new Vector2(150.0f, 250.0f),
            new Vector2(550.0f, 250.0f),
            new Vector2(550.0f, 150.0f)
        ];
        cutter.InsertVertexAtBack(0);
        cutter.InsertVertexAtBack(1);
        cutter.InsertVertexAtBack(2);
        cutter.InsertVertexAtBack(3);

        Console.WriteLine("Loaded test6");
    }

    static void InsertCircle(Vector2 center, float scale = 1.0f)
    {
        cutter = new Polygon<PolygonVertex>();
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

    static Vector2 FlipY(Vector2 vector)
    {
        return new Vector2(vector.X, HEIGHT - vector.Y);
    }
}