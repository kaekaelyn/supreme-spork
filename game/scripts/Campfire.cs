using ElderWorld.Core.Climate;
using ElderWorld.Core.Fire;
using ElderWorld.Core.Time;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// A hearth in the world. Owns a <see cref="Hearth"/> from the core and renders it:
/// light, glow and a place to stand near.
/// <para>
/// It has no UI and reports nothing. How the fire is doing is legible only from how
/// much light it throws and how warm you are, which is the whole point of docs/00 §3.
/// </para>
/// </summary>
public partial class Campfire : Node3D
{
    private readonly FixedStepAccumulator _accumulator = new(Hearth.StepSeconds);
    private readonly RandomNumberGenerator _flicker = new();

    private WorldSimulation _simulation = null!;
    private GreyboxTerrain _terrain = null!;
    private OmniLight3D _light = null!;
    private MeshInstance3D _glow = null!;
    private StandardMaterial3D _glowMaterial = null!;

    /// <summary>The simulated fire.</summary>
    public Hearth Hearth { get; } = new(thermalMassJPerK: 22_000.0, airflow: 0.75);

    /// <summary>How much of the rain the surrounding shelter keeps off, 0–1.</summary>
    [Export] public float RainShelter { get; set; }

    public override void _Ready()
    {
        _simulation = GetNode<WorldSimulation>("/root/Greybox/WorldSimulation");
        _terrain = GetNode<GreyboxTerrain>("/root/Greybox/Terrain");
        _flicker.Seed = 90210;

        Hearth.RainShelter = RainShelter;

        // A ring of hearth stones, which is where the thermal mass comes from.
        for (int i = 0; i < 9; i++)
        {
            float angle = Mathf.Tau * i / 9.0f;
            AddChild(new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(0.28f, 0.20f, 0.28f) },
                MaterialOverride = GreyboxMaterials.Stone,
                Position = new Vector3(Mathf.Cos(angle) * 0.62f, 0.10f, Mathf.Sin(angle) * 0.62f),
                RotationDegrees = new Vector3(0, Mathf.RadToDeg(angle), 0),
            });
        }

        _glowMaterial = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.12f, 0.10f, 0.09f),
            EmissionEnabled = true,
            Emission = new Color(1.0f, 0.48f, 0.14f),
            EmissionEnergyMultiplier = 0.0f,
            SpecularMode = BaseMaterial3D.SpecularModeEnum.Disabled,
        };

        _glow = new MeshInstance3D
        {
            Name = "Bed",
            Mesh = new CylinderMesh { TopRadius = 0.34f, BottomRadius = 0.44f, Height = 0.18f, RadialSegments = 8 },
            MaterialOverride = _glowMaterial,
            Position = new Vector3(0, 0.09f, 0),
        };
        AddChild(_glow);

        _light = new OmniLight3D
        {
            Name = "Firelight",
            LightColor = new Color(1.0f, 0.55f, 0.22f),
            LightEnergy = 0.0f,
            OmniRange = 14.0f,
            ShadowEnabled = true,
            Position = new Vector3(0, 0.5f, 0),
        };
        AddChild(_light);
    }

    public override void _Process(double delta)
    {
        EnvironmentSample sample = _simulation.SampleAt(_terrain.SiteAt(GlobalPosition));

        int steps = _accumulator.Pump(_simulation.LastWorldDelta);
        for (int i = 0; i < steps; i++)
            Hearth.Step(sample);

        Render(delta);
    }

    private void Render(double delta)
    {
        // Light energy from the actual illuminance the model produces. A campfire is
        // a poor lamp — a couple of lux at two metres — and that is exactly why an
        // eleven-hour night is a design brief rather than dead time (docs/02 §6).
        float illuminance = (float)Hearth.IlluminanceAt(2.0);
        float target = Mathf.Clamp(illuminance / 12.0f, 0.0f, 3.2f);

        float flicker = 1.0f + _flicker.RandfRange(-0.10f, 0.10f) * Mathf.Clamp(target, 0.0f, 1.0f);
        _light.LightEnergy = Mathf.Lerp(_light.LightEnergy, target * flicker, (float)Mathf.Min(1.0, delta * 12.0));
        _light.Visible = _light.LightEnergy > 0.001f;

        // The bed glows from its own temperature, so banked coals stay visible after
        // the flames have gone.
        float heat = Mathf.Clamp((float)((Hearth.BedTemperatureC - 100.0) / 750.0), 0.0f, 1.0f);
        _glowMaterial.EmissionEnergyMultiplier = heat * 2.2f;
        _glowMaterial.Emission = new Color(1.0f, Mathf.Lerp(0.20f, 0.55f, heat), Mathf.Lerp(0.03f, 0.16f, heat));
    }

    /// <summary>Radiant flux reaching a point, W/m². Fed straight into the thermal model.</summary>
    public double IrradianceAt(Vector3 position)
        => Hearth.IrradianceAt(GlobalPosition.DistanceTo(position));

    /// <summary>Adds a piece of seasoned conifer deadwood.</summary>
    public void AddDeadwood()
        => Hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.18, 0.045));

    /// <summary>Adds a handful of tinder — what an ember can actually catch.</summary>
    public void AddTinder()
        => Hearth.AddFuel(new FuelPiece(FuelSpecies.Tinder, 0.05, 0.08, 0.003));
}
