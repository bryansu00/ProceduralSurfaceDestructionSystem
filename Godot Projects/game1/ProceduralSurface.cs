using Godot;
using PSDSystem;
using System.Collections.Generic;

public partial class ProceduralSurface : Node3D
{
    private MeshInstance3D _meshInstance;

    private SurfaceShape<PolygonVertex> _surface;

    public override void _Ready()
    {
        base._Ready();

        MeshInstance3D placeHolder = GetNode<MeshInstance3D>("Placeholder");
        //placeHolder.Visible = false;

        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
        
        InitSurface();
    }

    private void InitSurface()
    {
        _surface = new SurfaceShape<PolygonVertex>();

        Polygon<PolygonVertex> polygon = new Polygon<PolygonVertex>();
        polygon.Vertices = new List<Vector2>();

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
