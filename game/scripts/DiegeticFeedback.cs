using ElderWorld.Core.Thermal;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// How the body tells the player what is happening to it, given that nothing else is
/// allowed to.
/// <para>
/// docs/02 opens with the rule this class exists to obey: <i>"no needs bars on the HUD
/// by default. The body reports its own state. You shiver, your breath fogs, your
/// hands get clumsy and drop things, your vision narrows, your character starts making
/// involuntary noises."</i>
/// </para>
/// <para>
/// docs/07 §6 names the risk this is guarding against: <i>"'Nothing is explained' reads
/// as unfinished rather than deliberate. Mitigation: the body has to communicate
/// flawlessly."</i> Every signal here is driven by a real number out of the thermal
/// model, so what the player sees is the actual state of the simulation and not a
/// scripted approximation of it. Playtest for "I didn't know what to do" versus "I knew
/// what to do and couldn't" — the first is a bug in this file.
/// </para>
/// </summary>
public partial class DiegeticFeedback : Node
{
    private PlayerBody _body = null!;
    private PlayerController _controller = null!;
    private SkyDriver _sky = null!;
    private Camera3D _camera = null!;
    private CpuParticles3D _breath = null!;

    private readonly RandomNumberGenerator _rng = new();
    private Vector3 _cameraRest;
    private float _shakePhase;
    private double _untilNextBreath;

    public override void _Ready()
    {
        _controller = GetParent<PlayerController>();
        _body = _controller.GetNode<PlayerBody>("Body");
        _sky = GetNode<SkyDriver>("/root/Greybox/Sky");
        _camera = _controller.Camera;
        _cameraRest = _camera.Position;
        _rng.Seed = 4711;

        _breath = BuildBreath();
        _camera.AddChild(_breath);
    }

    public override void _Process(double delta)
    {
        ThermalState state = _body.State;

        ApplyShivering(state, delta);
        ApplyBreath(state, delta);
        ApplyVision(state, delta);
    }

    /// <summary>
    /// Shivering, as camera shake. Amplitude tracks the actual watts of shivering
    /// thermogenesis, so it builds gradually and — importantly — <b>stops</b> as the
    /// core falls through the low thirties, because that is what real shivering does.
    /// The shaking ending feels like relief and is the opposite.
    /// </summary>
    private void ApplyShivering(ThermalState state, double delta)
    {
        float intensity = (float)state.ShiveringIntensity;
        _shakePhase += (float)delta * Mathf.Lerp(9.0f, 17.0f, intensity);

        float amplitude = intensity * 0.017f;
        var offset = new Vector3(
            Mathf.Sin(_shakePhase * 1.7f) * amplitude,
            Mathf.Sin(_shakePhase * 2.3f) * amplitude,
            0.0f);

        _camera.Position = _cameraRest + offset;
    }

    /// <summary>
    /// Fogged breath. It appears when the air is cold enough for exhaled vapour to
    /// condense, and its rate follows how hard the body is working — so a player who
    /// sprints can watch their own breathing rate climb, which is the only warning
    /// they will get before the sweat in their clothing turns on them.
    /// </summary>
    private void ApplyBreath(ThermalState state, double delta)
    {
        bool condensing = _body.Environment.AirTemperatureC < 9.0;
        if (!condensing)
        {
            _breath.Emitting = false;
            return;
        }

        // Breathing rate rises with metabolic rate and with shivering.
        double breathsPerMinute = 12.0
                                  + 26.0 * Mathf.Clamp((_controller.ActivityMet - 1.0) / 6.5, 0.0, 1.0)
                                  + 8.0 * state.ShiveringIntensity;

        _untilNextBreath -= delta;
        if (_untilNextBreath > 0.0) return;

        _untilNextBreath = 60.0 / breathsPerMinute;

        // Colder air makes a denser, longer-lived plume.
        float cold = Mathf.Clamp((float)((9.0 - _body.Environment.AirTemperatureC) / 25.0), 0.0f, 1.0f);
        _breath.Lifetime = Mathf.Lerp(0.7f, 1.9f, cold);
        _breath.ScaleAmountMax = Mathf.Lerp(0.10f, 0.24f, cold);
        _breath.Emitting = true;
        _breath.Restart();
    }

    /// <summary>
    /// Vision. Colour drains and the world dims as the core falls, and the view
    /// narrows toward tunnel vision at confusion.
    /// <para>
    /// Done with tonemapping and field of view rather than an overlay, because an
    /// overlay is a HUD element wearing a costume and docs/07 §1 says
    /// <i>"No UI whatsoever."</i>
    /// </para>
    /// </summary>
    private void ApplyVision(ThermalState state, double delta)
    {
        // Nothing at all until the body is genuinely in trouble.
        double impairment = Mathf.Clamp(
            (HypothermiaThresholds.ShiveringC - state.CoreTemperatureC) /
            (HypothermiaThresholds.ShiveringC - HypothermiaThresholds.UnconsciousC), 0.0, 1.0);

        float saturation = Mathf.Lerp(1.0f, 0.15f, (float)impairment);
        float brightness = Mathf.Lerp(1.0f, 0.55f, (float)impairment);
        _sky.ApplyVisionDegradation(saturation, brightness);

        float targetFov = Mathf.Lerp(75.0f, 58.0f, (float)impairment);
        _camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, (float)Mathf.Min(1.0, delta * 1.5));
    }

    private static CpuParticles3D BuildBreath()
    {
        var material = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Color(0.85f, 0.87f, 0.90f, 0.16f),
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Particles,
        };

        return new CpuParticles3D
        {
            Name = "Breath",
            Emitting = false,
            OneShot = true,
            Amount = 10,
            Lifetime = 1.2f,
            Explosiveness = 0.75f,
            Randomness = 0.5f,
            Position = new Vector3(0, -0.10f, -0.28f),
            Direction = new Vector3(0, -0.15f, -1.0f),
            Spread = 16.0f,
            InitialVelocityMin = 0.5f,
            InitialVelocityMax = 1.3f,
            Gravity = new Vector3(0, 0.22f, 0),
            ScaleAmountMin = 0.05f,
            ScaleAmountMax = 0.18f,
            Mesh = new SphereMesh { Radius = 0.5f, Height = 1.0f, RadialSegments = 6, Rings = 4 },
            MaterialOverride = material,
        };
    }
}
