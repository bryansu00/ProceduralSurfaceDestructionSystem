using Godot;
using PSDSystem;
using System;
using System.Collections.Generic;

public partial class ProceduralSurface : Node3D
{
    private MeshInstance3D _meshInstance;

    private StaticBody3D _staticBody3D;

    private SurfaceShape<PolygonVertex> _surface;

    private CoordinateConverter _coordinateConverter;

    public override void _Ready()
    {
        base._Ready();

        MeshInstance3D placeHolder = GetNode<MeshInstance3D>("Placeholder");
        placeHolder.Visible = false;

        _staticBody3D = GetNode<StaticBody3D>("StaticBody3D");
        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        _coordinateConverter = new CoordinateConverter(Vector3.Zero, Vector3.Right, Vector3.Up);

        InitSurface();
        GenerateMeshOfSurface();
        GenerateCollision();
    }

    public void DamageSurface(Vector3 globalCollisionPoint)
    {
        Vector3 localCollisionPoint = globalCollisionPoint - GlobalPosition;
        Vector2 collisionPointOnPlane = _coordinateConverter.ConvertTo2D(localCollisionPoint);
        
        Polygon<PolygonVertex> cutter = CreateCircle(collisionPointOnPlane, 0.05f);
        int result = PSD.CutSurface<PolygonVertex, BooleanVertex>(_surface, cutter);

        GenerateMeshOfSurface();

        GD.Print(result);
    }

    private Polygon<PolygonVertex> CreateCircle(Vector2 center, float scale = 1.0f)
    {
        Polygon<PolygonVertex> shape = new Polygon<PolygonVertex> { Vertices = new List<Vector2> {
                new Vector2(10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, 10.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, 7.07f * scale + center.Y),
                new Vector2(-10.0f * scale + center.X, 0.0f * scale + center.Y),
                new Vector2(-7.07f * scale + center.X, -7.07f * scale + center.Y),
                new Vector2(0.0f * scale + center.X, -10.0f * scale + center.Y),
                new Vector2(7.07f * scale + center.X, -7.07f * scale + center.Y)
            }
        };

        shape.InsertVertexAtBack(0);
        shape.InsertVertexAtBack(1);
        shape.InsertVertexAtBack(2);
        shape.InsertVertexAtBack(3);
        shape.InsertVertexAtBack(4);
        shape.InsertVertexAtBack(5);
        shape.InsertVertexAtBack(6);
        shape.InsertVertexAtBack(7);

        return shape;
    }

    private void DrawSphere(Vector3 pos)
    {
        var ins = new MeshInstance3D();
        AddChild(ins);
        ins.Position = pos;
        var sphere = new SphereMesh();
        sphere.Radius = 0.1f;
        sphere.Height = 0.1f;
        ins.Mesh = sphere;
    }


    private void InitSurface()
    {
        _surface = new SurfaceShape<PolygonVertex>();

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>() {
            Vertices = new List<Vector2> {
                new Vector2(-5.0f, -5.0f),
                new Vector2(-5.0f, 5.0f),
                new Vector2(5.0f, 5.0f),
                new Vector2(5.0f, -5.0f)
            }
        };

        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        _surface.AddOuterPolygon(polygon);
    }

    private void GenerateMeshOfSurface()
    {
        var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // Stuff needed for the procedural mesh
        List<Vector3> verts;
        List<Vector2> uvs;
        List<Vector3> normals;
        List<int> indices;

        // Generate the mesh here

        // Generate Front Face
        List<Vector2> verts2D;
        PSD.TriangulateSurface(_surface, out indices, out verts2D);
        if (indices == null || verts2D == null) return;

        verts = _coordinateConverter.ConvertListTo3D(verts2D);

        uvs = new List<Vector2>();
        normals = new List<Vector3>();
        for (int i = 0; i < verts.Count; i++)
        {
            uvs.Add(Vector2.Zero);
            normals.Add(Vector3.Back);
        }

        // Generate Back Face
        for (int i = 0; i < verts2D.Count; i++)
        {
            verts.Add(verts[i] - new Vector3(0.0f, 0.0f, 0.1f));
            uvs.Add(Vector2.Zero);
            normals.Add(Vector3.Forward);
        }

        for (int i = indices.Count - 1; i >= 0; i--)
        {
            indices.Add(indices[i] + verts2D.Count);
        }

        // Generate Side cap
        PSD.CreateSideCapOfSurface(_surface, _coordinateConverter, Vector3.Back, 0.1f,
            verts, normals, indices, uvs);

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        ArrayMesh arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        _meshInstance.Mesh = arrayMesh;
    }

    private void GenerateCollision()
    {
        List<Vector3> vertices = _coordinateConverter.ConvertListTo3D(_surface.Polygons[0].OuterPolygon.Vertices);

        ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
        shape.Points = vertices.ToArray();
        
        CollisionShape3D collisionShape3D = new CollisionShape3D();
        collisionShape3D.Shape = shape;

        _staticBody3D.AddChild(collisionShape3D);
    }

    private void GenerateMesh()
    {
        var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var indices = new List<int>();

        /***********************************
        * Insert code here to generate mesh.
        ************************************/
        
        // Front Face
        verts.Add(new Vector3(0.0f, 0.0f, 0.0f));
        verts.Add(new Vector3(0.0f, 10.0f, 0.0f));
        verts.Add(new Vector3(10.0f, 10.0f, 0.0f));
        verts.Add(new Vector3(10.0f, 0.0f, 0.0f));

        normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
        normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
        normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
        normals.Add(new Vector3(0.0f, 0.0f, 1.0f));

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));

        indices.Add(0);
        indices.Add(1);
        indices.Add(2);

        indices.Add(0);
        indices.Add(2);
        indices.Add(3);

        // Back Face - Basically duplicate of the front face...
        verts.Add(new Vector3(10.0f, 0.0f, -0.1f));
        verts.Add(new Vector3(10.0f, 10.0f, -0.1f));
        verts.Add(new Vector3(0.0f, 10.0f, -0.1f));
        verts.Add(new Vector3(0.0f, 0.0f, -0.1f));

        normals.Add(new Vector3(0.0f, 0.0f, -0.1f));
        normals.Add(new Vector3(0.0f, 0.0f, -0.1f));
        normals.Add(new Vector3(0.0f, 0.0f, -0.1f));
        normals.Add(new Vector3(0.0f, 0.0f, -0.1f));

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));

        indices.Add(4);
        indices.Add(5);
        indices.Add(6);

        indices.Add(4);
        indices.Add(6);
        indices.Add(7);

        // Side cap 1
        verts.Add(new Vector3(0.0f, 0.0f, -0.1f));
        verts.Add(new Vector3(0.0f, 10.0f, -0.1f));
        verts.Add(new Vector3(0.0f, 10.0f, 0.0f));
        verts.Add(new Vector3(0.0f, 0.0f, 0.0f));

        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));

        indices.Add(8);
        indices.Add(9);
        indices.Add(10);

        indices.Add(8);
        indices.Add(10);
        indices.Add(11);

        // Side cap 2
        verts.Add(new Vector3(10.0f, 0.0f, 0.0f));
        verts.Add(new Vector3(10.0f, 10.0f, 0.0f));
        verts.Add(new Vector3(10.0f, 10.0f, -0.1f));
        verts.Add(new Vector3(10.0f, 0.0f, -0.1f));

        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));

        indices.Add(12);
        indices.Add(13);
        indices.Add(14);

        indices.Add(12);
        indices.Add(14);
        indices.Add(15);

        // Should add top and bottom cap

        /**********************************/

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        ArrayMesh arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        _meshInstance.Mesh = arrayMesh;
    }
}
