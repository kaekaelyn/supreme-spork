using ElderWorld.Core.Fire;
using ElderWorld.Core.Thermal;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// Movement, looking, and the two or three physical things a naked person can do.
/// <para>
/// There is no HUD, no prompt, no crosshair and no tutorial (docs/00 §3). The keys
/// are documented in the repository README, out of the fiction, because the fiction
/// never explains itself. What the player has to work out for themselves is
/// everything that matters: that the trees are warmer than the open, that the hot
/// ground will give them fire, and that running to keep warm is a trap.
/// </para>
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    private const float WalkSpeed = 3.4f;
    private const float RunSpeed = 6.2f;
    private const float MouseSensitivity = 0.0022f;
    private const float Gravity = 9.8f;

    private Camera3D _camera = null!;
    private float _pitch;
    private bool _sitting;

    /// <summary>The head, which everything visual hangs off.</summary>
    public Camera3D Camera => _camera;

    /// <summary>Metabolic rate this frame, in met. Read by the thermal model.</summary>
    public double ActivityMet { get; private set; } = 1.2;

    /// <summary>What the body is doing, for ground contact and exposed area.</summary>
    public Posture Posture { get; private set; } = Posture.Standing;

    /// <summary>The ember being carried, if any.</summary>
    public Ember? CarriedEmber { get; private set; }

    /// <summary>The fire this player can reach.</summary>
    public Campfire? Hearth { get; set; }

    /// <summary>Where the hot volcanic ground is.</summary>
    public Node3D? Fumarole { get; set; }

    public override void _Ready()
    {
        AddChild(new CollisionShape3D
        {
            Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.75f },
            Position = new Vector3(0, 0.875f, 0),
        });

        _camera = new Camera3D
        {
            Name = "Head",
            Position = new Vector3(0, 1.65f, 0),
            Fov = 75.0f,
            Far = 600.0f,
        };
        AddChild(_camera);

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(-motion.Relative.X * MouseSensitivity);
            _pitch = Mathf.Clamp(_pitch - motion.Relative.Y * MouseSensitivity, -1.5f, 1.5f);
            _camera.Rotation = new Vector3(_pitch, 0, 0);
        }

        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

        switch (key.PhysicalKeycode)
        {
            case Key.Escape:
                Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                    ? Input.MouseModeEnum.Visible
                    : Input.MouseModeEnum.Captured;
                break;

            case Key.C:
                _sitting = !_sitting;
                break;

            case Key.E:
                Interact();
                break;

            case Key.Q:
                if (Hearth is not null && IsWithinReach(Hearth)) Hearth.AddDeadwood();
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor()) velocity.Y -= Gravity * (float)delta;
        else velocity.Y = 0.0f;

        var input = new Vector2(
            (Input.IsPhysicalKeyPressed(Key.D) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.A) ? 1 : 0),
            (Input.IsPhysicalKeyPressed(Key.S) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.W) ? 1 : 0));

        bool running = Input.IsPhysicalKeyPressed(Key.Shift);
        if (input != Vector2.Zero) _sitting = false;

        Vector3 direction = (Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
        float speed = _sitting ? 0.0f : running ? RunSpeed : WalkSpeed;

        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;

        Velocity = velocity;
        MoveAndSlide();

        UpdateExertion(input != Vector2.Zero, running);
    }

    /// <summary>
    /// Maps what the player is doing onto a metabolic rate.
    /// <para>
    /// Running produces a lot of heat, which is why it feels like the answer, and a
    /// lot of sweat, which is why it is not. docs/02 §1: <i>"Sprinting to stay warm
    /// and then stopping is how you die."</i> Nothing warns the player; the thermal
    /// model simply does the arithmetic.
    /// </para>
    /// </summary>
    private void UpdateExertion(bool moving, bool running)
    {
        if (_sitting)
        {
            Posture = Posture.Sitting;
            ActivityMet = 1.0;
            return;
        }

        Posture = Posture.Standing;
        ActivityMet = moving ? running ? 7.5 : 3.0 : 1.2;
    }

    private void Interact()
    {
        if (Fumarole is not null && GlobalPosition.DistanceTo(Fumarole.GlobalPosition) < 2.5f)
        {
            // docs/02 §4: before you can make fire, you can fetch it.
            CarriedEmber ??= Ember.FromVolcanicVent();
            return;
        }

        if (Hearth is null || !IsWithinReach(Hearth)) return;

        if (CarriedEmber is { IsAlive: true })
        {
            // Tinder first, then the ember on top of it — which is the only order
            // that works, and which the game will never say out loud.
            Hearth.AddTinder();
            Hearth.Hearth.LayEmber(CarriedEmber);
            CarriedEmber = null;
            return;
        }

        Hearth.AddTinder();
    }

    private bool IsWithinReach(Node3D target) => GlobalPosition.DistanceTo(target.GlobalPosition) < 2.5f;

    /// <summary>Advances the carried ember. Called by the body, which owns the timing.</summary>
    public void StepCarriedEmber(Core.Climate.EnvironmentSample sample, int steps)
    {
        if (CarriedEmber is null) return;

        for (int i = 0; i < steps; i++)
            CarriedEmber.Step(sample, sheltered: false);

        if (!CarriedEmber.IsAlive) CarriedEmber = null;
    }
}
