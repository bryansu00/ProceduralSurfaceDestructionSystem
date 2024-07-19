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

        if (Material1 != null)
            _meshInstance.SetSurfaceOverrideMaterial(0, Material1);
        if (Material2 != null)
            _meshInstance.SetSurfaceOverrideMaterial(1, Material2);
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
        GD.Print(string.Format("Distance From Top Left: {0}, Position: {1}", 2.0f - cutter.Vertices[cutter.Head.Data.Index].Y, collisionPointOnPlane));

        // Generate the surface and collision
        GPSD.ExtrudeSurface(_meshInstance, _surface, _coordinateConverter, _originalVertices, _originalUvs, _depth);
        GPSD.GenerateCollisionShape(_staticBody3D, _surface, _coordinateConverter, _depth);

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

        GPSD.ExtrudeSurface(_meshInstance, _surface, _coordinateConverter, _originalVertices, _originalUvs, _depth);
        GPSD.GenerateCollisionShape(_staticBody3D, _surface, _coordinateConverter, _depth);
    }
}
