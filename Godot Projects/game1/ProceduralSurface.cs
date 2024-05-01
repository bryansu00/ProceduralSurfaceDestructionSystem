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

    private MeshInstance3D? _meshInstance;

    private StaticBody3D? _staticBody3D;

    private SurfaceShape<PolygonVertex>? _surface;
    private Polygon<PolygonVertex>? _anchorPolygon;
    private List<Vector2>? _originalVertices;
    private List<Vector2>? _originalUvs;

    private CoordinateConverter? _coordinateConverter;

    private readonly float _depth = 1.0f;

    public override void _Ready()
    {
        base._Ready();

        // Turn off the visibility of the placeholder mesh
        MeshInstance3D placeHolder = GetNode<MeshInstance3D>("Placeholder");
        placeHolder.Visible = false;

        // Get and set the variables for the nodes
        _staticBody3D = GetNode<StaticBody3D>("StaticBody3D");
        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        // Create and Initialize the CoordinateConverter
        _coordinateConverter = new CoordinateConverter(new Vector3(0.0f, 0.0f, _depth / 2.0f), Vector3.Right, Vector3.Up);

        // Initialize the surface and generate the initial meshes and collisions
        InitSurface();
        GenerateMeshOfSurface();
        GenerateCollision();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // Below is for debugging purposes
        DebugDraw2D.SetText("SurfacePolygonCount", _surface.Polygons.Count);
        int i = 0;
        foreach (PolygonGroup<PolygonVertex> group in _surface.Polygons)
        {
            DebugDraw2D.SetText(string.Format("Group {0} Vertex Count", i), group.OuterPolygon.Count);
            i++;
        }
    }

    public void DamageSurface(Vector3 globalCollisionPoint)
    {
        // Convert global coordinate to local, and then onto 2D surface
        Vector3 localCollisionPoint = ToLocal(globalCollisionPoint); ;
        Vector2 collisionPointOnPlane = _coordinateConverter.ConvertTo2D(localCollisionPoint);
        
        // Process the Surface with a cutter
        Polygon<PolygonVertex> cutter = CreateCircle(collisionPointOnPlane, 0.05f);
        PSD.CutSurfaceResult result = PSD.CutSurface<PolygonVertex, BooleanVertex>(_surface, cutter, _anchorPolygon);

        // DEBUG Purpose
        GD.Print(string.Format("Distance From Top Left: {0}", 2.0f - cutter.Vertices[cutter.Head.Data.Index].Y));

        // Generate the surface and collision
        GenerateMeshOfSurface();
        GenerateCollision();

        // Print the result of the CutSurface function
        GD.Print(result);
    }

    /// <summary>
    /// Create a circule shaped polygon
    /// </summary>
    /// <param name="center"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Initialize the surface of this procedural mesh
    /// </summary>
    private void InitSurface()
    {
        _surface = new SurfaceShape<PolygonVertex>();

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>() {
            Vertices = new List<Vector2> {
                new Vector2(-5.0f, -2.0f), // bottom left
                new Vector2(-5.0f, 2.0f), // top left
                new Vector2(5.0f, 2.0f), // top right
                new Vector2(5.0f, -2.0f) // bottom right
            }
        };

        polygon.InsertVertexAtBack(0);
        polygon.InsertVertexAtBack(1);
        polygon.InsertVertexAtBack(2);
        polygon.InsertVertexAtBack(3);

        _anchorPolygon = new Polygon<PolygonVertex>(polygon);

        _surface.AddOuterPolygon(polygon);

        // Store the original initial surface needed for "anchors"
        _originalVertices = new List<Vector2>(polygon.Vertices);
        // Store the initial UV values for the surface
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

        // ------------------------------------------------------------------------------------------
        // Generate Front Face

        // Lists needed for the procedural mesh
        List<Vector3> frontVerts;
        List<Vector2>? frontUvs;
        List<Vector3> frontNormals = new List<Vector3>();
        List<int> frontIndices;

        // Triangulate the surface
        List<Vector2> verts2D;
        PSD.TriangulateSurface(_surface, out frontIndices, out verts2D);
        if (frontIndices == null || verts2D == null)
        {
            // Given surface cannot be triangulated,
            // thus no mesh will be generated
            _meshInstance.Mesh = arrayMesh;
            return;
        }

        // Convert triangulation into 3D
        frontVerts = _coordinateConverter.ConvertListTo3D(verts2D);
        // Add the normals for the front face, it is Vector3.Back by default
        for (int i = 0; i < frontVerts.Count; i++)
        {
            frontNormals.Add(Vector3.Back);
        }

        // Compute the UV coordinates of the front face
        frontUvs = PSD.ComputeUVCoordinates(_originalVertices, _originalUvs, verts2D);
        if (frontUvs == null) return;

        // Create the arrays for mesh
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

        // Reuse the triangulation from prior computation to generate the backface
        for (int i = 0; i < verts2D.Count; i++)
        {
            backVerts.Add(frontVerts[i] - new Vector3(0.0f, 0.0f, _depth / 2.0f));
            backNormals.Add(Vector3.Forward);
        }

        // Copy the front face UVs, (Should be recalculated using a different
        // originalUVs list for the back face, else the texture will be flipped horizontally)
        backUvs.AddRange(frontUvs);

        // Add the indices of triangulation for the back face
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

        // ------------------------------------------------------------------------------------------
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

        foreach (Node child in _staticBody3D.GetChildren())
        {
            _staticBody3D.RemoveChild(child);
            child.QueueFree();
        }

        if (convexVertices == null) return;

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
