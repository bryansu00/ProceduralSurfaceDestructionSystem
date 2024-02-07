using Godot;

public partial class Player : CharacterBody3D
{
    // How fast the player moves in meters per second.
	[Export]
    public int Speed { get; set; } = 14;

    // The downwatd acceleration when in the air, in meters per second squared.
    [Export]
    public int FallAcceleration { get; set; } = 75;

    private Vector3 _targetVelocity = Vector3.Zero;

    private SpringArm3D _springArm;

    private float _sensitivity = 0.1f;

    private float _totalPitch = 0.0f;

    public override void _Ready()
    {
        base._Ready();

        _springArm = GetNode<SpringArm3D>("SpringArm");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Local variable to store input direction
        Vector3 direction = Vector3.Zero;

        // Check input and update direction accordingly
        if (Input.IsActionPressed("move_right"))
            direction.X += 1.0f;
        if (Input.IsActionPressed("move_left"))
            direction.X -= 1.0f;
        if (Input.IsActionPressed("move_back"))
            direction.Z += 1.0f;
        if (Input.IsActionPressed("move_forward"))
            direction.Z -= 1.0f;

        if (!direction.IsZeroApprox())
        {
            // Rotate camera based on the direction of the camera and then normalize
            direction = direction.Rotated(Vector3.Up, _springArm.Rotation.Y).Normalized();
        }

        // Ground velocity
        _targetVelocity.X = direction.X * Speed;
        _targetVelocity.Z = direction.Z * Speed;

        // Vertical velocity
        if (!IsOnFloor())
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }

        // Move the character
        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        // Process Mouse Input
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            float yaw = eventMouseMotion.Relative.X * _sensitivity;
            float pitch = eventMouseMotion.Relative.Y * _sensitivity;

            // Clamp pitch
            pitch = Mathf.Clamp(pitch, -90 - _totalPitch, 90 - _totalPitch);
            _totalPitch += pitch;

            // Perform the rotation to the spring arm
            _springArm.RotateY(Mathf.DegToRad(-yaw));
            _springArm.RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));

            Vector3 direction = Vector3.Forward.Rotated(Vector3.Up, _springArm.Rotation.Y).Normalized();
            GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
        }
    }
}
