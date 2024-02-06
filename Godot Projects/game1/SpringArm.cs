using Godot;

public partial class SpringArm : SpringArm3D
{
    private float _sensitivity = 0.1f;
    private float _totalPitch = 0.0f;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
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
            RotateY(Mathf.DegToRad(-yaw));
            RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));
        }
    }
}
