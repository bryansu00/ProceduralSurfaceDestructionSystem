using Godot;
using System.Collections.Generic;

public partial class ProceduralSurface : Node3D
{
    private MeshInstance3D _meshInstance;

    public override void _Ready()
    {
        base._Ready();

        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        GenerateMesh();
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
