using Godot;
using System;

public partial class TestProjectile : RigidBody3D
{
	public Vector3 Direction
	{
		get;
		set;
	} = Vector3.Forward;

	private float _left = 2.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        ApplyImpulse(Direction);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_left -= (float)delta;

		if (_left <= 0.0f)
		{
			QueueFree();
		}
	}
}
