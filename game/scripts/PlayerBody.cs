using ElderWorld.Core.Climate;
using ElderWorld.Core.Thermal;
using ElderWorld.Core.Time;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// The player's body, and the thing that is going to kill them.
/// <para>
/// Every frame this assembles a <see cref="ThermalExposure"/> from where the player
/// is standing, what they are doing, what they are wearing and what is burning nearby,
/// then steps the core's <see cref="ThermalModel"/> in fixed world-time steps.
/// </para>
/// <para>
/// It makes no decisions. All of the physics lives in the core, where it can be
/// soak-tested without an engine; this class only decides what to feed it.
/// </para>
/// </summary>
public partial class PlayerBody : Node
{
    private readonly FixedStepAccumulator _accumulator = new(ThermalModel.StepSeconds);
    private readonly ThermalModel _model = new();

    private WorldSimulation _simulation = null!;
    private GreyboxTerrain _terrain = null!;
    private PlayerController _controller = null!;

    /// <summary>The body's thermal condition.</summary>
    public ThermalState State { get; } = ThermalState.Fresh();

    /// <summary>
    /// What the player is wearing. You wake up with nothing (docs/02 §1), and
    /// clothing is a Stage 2 system — this is the seam it plugs into.
    /// </summary>
    public Insulation Clothing { get; private set; } = Insulation.Naked;

    /// <summary>The environment the body is currently in.</summary>
    public EnvironmentSample Environment { get; private set; }

    /// <summary>Radiant flux from fire reaching the body this frame, W/m².</summary>
    public double FireIrradiance { get; private set; }

    /// <summary>The fire the player can feel, if any.</summary>
    public Campfire? Hearth { get; set; }

    public override void _Ready()
    {
        _simulation = GetNode<WorldSimulation>("/root/Greybox/WorldSimulation");
        _terrain = GetNode<GreyboxTerrain>("/root/Greybox/Terrain");
        _controller = GetParent<PlayerController>();
    }

    public override void _Process(double delta)
    {
        SiteContext site = _terrain.SiteAt(_controller.GlobalPosition);
        Environment = _simulation.SampleAt(site);

        // Rain soaks what you are wearing; still air and a lack of anything to wear
        // is what dries it. Owned here rather than in the model because clothing
        // belongs to the equipment layer, not to the body.
        bool sheltered = site.SkyViewFactor < 0.15;
        Clothing = ThermalModel.UpdateClothingWetness(
            Clothing, Environment, sheltered, _simulation.LastWorldDelta);

        FireIrradiance = Hearth?.IrradianceAt(_controller.GlobalPosition) ?? 0.0;

        var exposure = new ThermalExposure
        {
            Environment = Environment,
            Clothing = Clothing,
            Posture = _controller.Posture,
            ActivityMet = _controller.ActivityMet,
            FireIrradianceWattsPerM2 = FireIrradiance,

            // Bedding is a Stage 2 craftable. Until then the ground takes what it wants.
            BeddingResistance = 0.0,
        };

        int steps = _accumulator.Pump(_simulation.LastWorldDelta);
        for (int i = 0; i < steps; i++)
            _model.Step(State, exposure);

        _controller.StepCarriedEmber(Environment, steps);
    }
}
