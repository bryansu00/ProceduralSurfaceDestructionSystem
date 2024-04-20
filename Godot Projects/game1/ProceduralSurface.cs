#nullable enable

using Godot;
using PSDSystem;
using System;
using System.Collections.Generic;

public partial class ProceduralSurface : Node3D
{
    [Export]
    private Material? Material1 = null;
    [Export]
    private Material? Material2 = null;
    [Export]
    private Material? MaterialSide = null;

    private MeshInstance3D _meshInstance;

    private StaticBody3D _staticBody3D;

    private SurfaceShape<PolygonVertex> _surface;
    private List<Vector2> _originalVertices;
    private List<Vector2> _originalUvs;

    private CoordinateConverter _coordinateConverter;

    private readonly float _depth = 1.0f;

    public override void _Ready()
    {
        base._Ready();

        MeshInstance3D placeHolder = GetNode<MeshInstance3D>("Placeholder");
        placeHolder.Visible = false;

        _staticBody3D = GetNode<StaticBody3D>("StaticBody3D");
        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        _coordinateConverter = new CoordinateConverter(new Vector3(0.0f, 0.0f, _depth / 2.0f), Vector3.Right, Vector3.Up);

        InitSurface();
        GenerateMeshOfSurface();
        GenerateCollision();
    }

    public void DamageSurface(Vector3 globalCollisionPoint)
    {
        Vector3 localCollisionPoint = globalCollisionPoint - GlobalPosition;
        Vector2 collisionPointOnPlane = _coordinateConverter.ConvertTo2D(localCollisionPoint);
        
        Polygon<PolygonVertex> cutter = CreateCircle(collisionPointOnPlane, 0.05f);
        PSD.CutSurfaceResult result = PSD.CutSurface<PolygonVertex, BooleanVertex>(_surface, cutter);

        GenerateMeshOfSurface();
        GenerateCollision();

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

    private void InitSurface()
    {
        _surface = new SurfaceShape<PolygonVertex>();

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>() {
            Vertices = new List<Vector2> {
                new Vector2(-5.0f, -5.0f), // bottom left
                new Vector2(-5.0f, 5.0f), // top left
                new Vector2(5.0f, 5.0f), // top right
                new Vector2(5.0f, -5.0f) // bottom right
            }
        };

        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        _surface.AddOuterPolygon(polygon);

        _originalVertices = new List<Vector2>(polygon.Vertices);
        _originalUvs = new List<Vector2>
        {
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
        };
    }

    private void GenerateMeshOfSurface()
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        // Generate Front Face

        // Objects needed for the procedural mesh
        List<Vector3> frontVerts;
        List<Vector2>? frontUvs = new List<Vector2>();
        List<Vector3> frontNormals = new List<Vector3>();
        List<int> frontIndices;

        // Begine generating the mesh here

        List<Vector2> verts2D;
        PSD.TriangulateSurface(_surface, out frontIndices, out verts2D);
        if (frontIndices == null || verts2D == null) return;

        frontVerts = _coordinateConverter.ConvertListTo3D(verts2D);

        for (int i = 0; i < frontVerts.Count; i++)
        {
            frontNormals.Add(Vector3.Back);
        }

        frontUvs = PSD.ComputeUVCoordinates(_originalVertices, _originalUvs, verts2D);
        if (frontUvs == null) return;

        var frontSurfaceArray = new Godot.Collections.Array();
        frontSurfaceArray.Resize((int)Mesh.ArrayType.Max);

        // Convert Lists to arrays and assign to surface array
        frontSurfaceArray[(int)Mesh.ArrayType.Vertex] = frontVerts.ToArray();
        frontSurfaceArray[(int)Mesh.ArrayType.TexUV] = frontUvs.ToArray();
        frontSurfaceArray[(int)Mesh.ArrayType.Normal] = frontNormals.ToArray();
        frontSurfaceArray[(int)Mesh.ArrayType.Index] = frontIndices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontSurfaceArray);

        // ------------------------------------------------------------------------------------------

        // Generate Back Face

        List<Vector3> backVerts = new List<Vector3>();
        List<Vector2> backUvs = new List<Vector2>();
        List<Vector3> backNormals = new List<Vector3>();
        List<int> backIndices = new List<int>();

        for (int i = 0; i < verts2D.Count; i++)
        {
            backVerts.Add(frontVerts[i] - new Vector3(0.0f, 0.0f, _depth / 2.0f));
            backNormals.Add(Vector3.Forward);
        }

        backUvs.AddRange(frontUvs);

        for (int i = frontIndices.Count - 1; i >= 0; i--)
        {
            backIndices.Add(frontIndices[i]);
        }

        var backSurfaceArray = new Godot.Collections.Array();
        backSurfaceArray.Resize((int)Mesh.ArrayType.Max);

        backSurfaceArray[(int)Mesh.ArrayType.Vertex] = backVerts.ToArray();
        backSurfaceArray[(int)Mesh.ArrayType.TexUV] = backUvs.ToArray();
        backSurfaceArray[(int)Mesh.ArrayType.Normal] = backNormals.ToArray();
        backSurfaceArray[(int)Mesh.ArrayType.Index] = backIndices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backSurfaceArray);

        // Generate Side cap
        List<Vector3> sideVerts = new List<Vector3>();
        List<Vector2> sideUvs = new List<Vector2>();
        List<Vector3> sideNormals = new List<Vector3>();
        List<int> sideIndices = new List<int>();

        PSD.CreateSideCapOfSurface(_surface, _coordinateConverter, Vector3.Back, _depth / 2.0f,
            sideVerts, sideNormals, sideIndices, sideUvs);

        var sideSurfaceArray = new Godot.Collections.Array();
        sideSurfaceArray.Resize((int)Mesh.ArrayType.Max);

        sideSurfaceArray[(int)Mesh.ArrayType.Vertex] = sideVerts.ToArray();
        sideSurfaceArray[(int)Mesh.ArrayType.TexUV] = sideUvs.ToArray();
        sideSurfaceArray[(int)Mesh.ArrayType.Normal] = sideNormals.ToArray();
        sideSurfaceArray[(int)Mesh.ArrayType.Index] = sideIndices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, sideSurfaceArray);

        _meshInstance.Mesh = arrayMesh;

        if (Material1 != null)
            _meshInstance.SetSurfaceOverrideMaterial(0, Material1);
        if (Material2 != null)
            _meshInstance.SetSurfaceOverrideMaterial(1, Material2);
    }

    private void GenerateCollision()
    {
        List<List<Vector2>>? convexVertices = PSD.FindConvexVerticesOfSurface(_surface);
        if (convexVertices == null) return;

        foreach (Node child in _staticBody3D.GetChildren())
        {
            _staticBody3D.RemoveChild(child);
            child.QueueFree();
        }

        foreach (List<Vector2> vertexGroup in convexVertices)
        {
            List<Vector3> vertices = _coordinateConverter.ConvertListTo3D(vertexGroup);
            for (int i = vertices.Count - 1; i >= 0; i--)
            {
                vertices.Add(vertices[i] - new Vector3(0.0f, 0.0f, _depth / 2.0f));
            }

            ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
            shape.Points = vertices.ToArray();

            CollisionShape3D collisionShape3D = new CollisionShape3D();
            collisionShape3D.Shape = shape;

            _staticBody3D.AddChild(collisionShape3D);
        }
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
