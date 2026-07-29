using ElderWorld.Core.Climate;
using ElderWorld.Core.Time;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// Turns the world clock into light. Sun, moon, sky colour, fog and ambient level all
/// come from <see cref="SolarPosition"/> and <see cref="LunarPosition"/> — nothing
/// here is on a timer or a curve.
/// <para>
/// This is where the 23.4-hour day becomes something you can actually watch, and
/// where the Stage 1 gate gets decided. docs/09 §8: <i>"Gate: is the night
/// compelling? If two hours of real darkness with a fire is tedious, no amount of art
/// fixes it."</i> The honest version of that test needs the darkness to be real — so
/// a moonless night here is genuinely, uncomfortably dark, and a gibbous moon on snow
/// genuinely is not.
/// </para>
/// </summary>
public partial class SkyDriver : Node3D
{
    private WorldSimulation _simulation = null!;
    private DirectionalLight3D _sun = null!;
    private DirectionalLight3D _moon = null!;
    private WorldEnvironment _worldEnvironment = null!;
    private ProceduralSkyMaterial _skyMaterial = null!;

    /// <summary>Where to sample the weather from. Set by the bootstrap to follow the player.</summary>
    public Node3D? Observer { get; set; }

    /// <summary>The terrain, for working out the observer's site.</summary>
    public GreyboxTerrain? Terrain { get; set; }

    /// <summary>The environment sampled this frame, for anything else that wants it.</summary>
    public EnvironmentSample Current { get; private set; }

    public override void _Ready()
    {
        _simulation = GetNode<WorldSimulation>("/root/Greybox/WorldSimulation");

        _sun = new DirectionalLight3D
        {
            Name = "Sun",
            ShadowEnabled = true,
            LightAngularDistance = 0.5f,
            DirectionalShadowMaxDistance = 220.0f,
        };
        AddChild(_sun);

        _moon = new DirectionalLight3D
        {
            Name = "Moon",
            ShadowEnabled = false,
            LightColor = new Color(0.62f, 0.70f, 0.92f),
        };
        AddChild(_moon);

        _skyMaterial = new ProceduralSkyMaterial
        {
            SkyTopColor = new Color(0.16f, 0.26f, 0.42f),
            SkyHorizonColor = new Color(0.52f, 0.55f, 0.58f),
            GroundBottomColor = new Color(0.14f, 0.13f, 0.12f),
            GroundHorizonColor = new Color(0.35f, 0.34f, 0.33f),
        };

        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Sky,
            Sky = new Sky { SkyMaterial = _skyMaterial },
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            TonemapMode = Godot.Environment.ToneMapper.Filmic,
            TonemapExposure = 1.0f,
            FogEnabled = true,
            FogDensity = 0.004f,
            AdjustmentEnabled = true,
        };

        _worldEnvironment = new WorldEnvironment { Name = "WorldEnvironment", Environment = environment };
        AddChild(_worldEnvironment);
    }

    public override void _Process(double delta)
    {
        SiteContext site = Observer is not null && Terrain is not null
            ? Terrain.SiteAt(Observer.GlobalPosition)
            : SiteContext.OpenGround;

        Current = _simulation.SampleAt(site);
        ApplySun(Current);
        ApplyMoon(Current);
        ApplyAtmosphere(Current);
    }

    /// <summary>
    /// Converts an altitude and azimuth into a unit vector, in Godot's axes:
    /// +X east, +Y up, −Z north.
    /// </summary>
    private static Vector3 SkyDirection(double altitudeRadians, double azimuthRadians)
    {
        double horizontal = Mathf.Cos(altitudeRadians);
        return new Vector3(
            (float)(Mathf.Sin(azimuthRadians) * horizontal),
            (float)Mathf.Sin(altitudeRadians),
            (float)(-Mathf.Cos(azimuthRadians) * horizontal)).Normalized();
    }

    /// <summary>Aims a directional light so that its rays travel away from the body.</summary>
    private static void AimLight(DirectionalLight3D light, Vector3 bodyDirection)
    {
        // A DirectionalLight3D emits along its local −Z, so pointing −Z at the
        // opposite of the body's direction makes the light fall from the body.
        Vector3 up = Mathf.Abs(bodyDirection.Y) > 0.98f ? Vector3.Forward : Vector3.Up;
        light.LookAt(light.GlobalPosition - bodyDirection, up);
    }

    private void ApplySun(EnvironmentSample sample)
    {
        SolarPosition sun = sample.Sun;
        _sun.Visible = sun.AltitudeRadians > -0.12;

        if (_sun.Visible)
            AimLight(_sun, SkyDirection(sun.AltitudeRadians, sun.AzimuthRadians));

        // Energy tracks the actual irradiance rather than a hand-drawn curve, so a
        // low winter sun through cloud is genuinely feeble.
        float energy = (float)(sample.SolarIrradianceWattsPerM2 / 900.0);
        _sun.LightEnergy = Mathf.Clamp(energy, 0.0f, 1.35f);

        // Low sun reddens as it burns through more atmosphere. docs/01 §8 notes that
        // higher Cretaceous CO₂ and volcanic aerosol justify a hazier sky and vivid
        // sunsets, which is free atmosphere for us.
        float lowness = 1.0f - Mathf.Clamp((float)(sun.AltitudeDegrees / 25.0), 0.0f, 1.0f);
        _sun.LightColor = new Color(1.0f, Mathf.Lerp(0.97f, 0.62f, lowness), Mathf.Lerp(0.92f, 0.36f, lowness));
    }

    private void ApplyMoon(EnvironmentSample sample)
    {
        LunarPosition moon = sample.Moon;

        // Moonlight is worth a fraction of a lux. It is only ever visible because the
        // alternative is near-total darkness — which is the point of the long night.
        double lux = moon.IlluminanceLux * (1.0 - 0.8 * sample.CloudCover);
        double withAlbedo = lux * (1.0 + 0.7 * sample.SnowCoverFraction);

        _moon.Visible = moon.IsUp && withAlbedo > 0.001 && !sample.Sun.IsUp;
        if (!_moon.Visible) return;

        AimLight(_moon, SkyDirection(moon.AltitudeRadians, moon.AzimuthRadians));
        _moon.LightEnergy = Mathf.Clamp((float)(withAlbedo * 0.85), 0.0f, 0.30f);
    }

    private void ApplyAtmosphere(EnvironmentSample sample)
    {
        Godot.Environment environment = _worldEnvironment.Environment;

        double daylight = Mathf.Clamp(sample.SolarIrradianceWattsPerM2 / 700.0, 0.0, 1.0);
        double twilight = sample.Sun.IsCivilTwilightOrBrighter && !sample.Sun.IsUp ? 0.25 : 0.0;
        double light = Math.Max(daylight, twilight);

        var day = new Color(0.52f, 0.55f, 0.58f);
        var night = new Color(0.045f, 0.055f, 0.085f);
        Color horizon = night.Lerp(day, (float)light);

        _skyMaterial.SkyHorizonColor = horizon;
        _skyMaterial.SkyTopColor = new Color(0.02f, 0.03f, 0.06f).Lerp(new Color(0.16f, 0.26f, 0.42f), (float)light);
        _skyMaterial.GroundHorizonColor = horizon * 0.6f;

        // Overcast flattens the sky toward uniform grey, which is most of why an
        // overcast day reads as oppressive.
        _skyMaterial.SkyCurve = Mathf.Lerp(0.15f, 0.5f, (float)sample.CloudCover);

        // docs/09 §1.3: "Fog is the cheapest immersion in the medium" — and here it
        // has a scientific justification rather than being an art choice.
        float humidity = (float)sample.RelativeHumidity;
        float precipitation = (float)Mathf.Clamp(sample.PrecipitationMmPerHour / 6.0, 0.0, 1.0);
        environment.FogDensity = Mathf.Lerp(0.0025f, 0.020f, Mathf.Max(humidity * humidity, precipitation));
        environment.FogLightColor = horizon;
        environment.FogLightEnergy = Mathf.Lerp(0.15f, 1.0f, (float)light);

        environment.AmbientLightEnergy = Mathf.Lerp(0.03f, 1.0f, (float)light);
    }

    /// <summary>Applies the vision degradation the body asks for. See <see cref="DiegeticFeedback"/>.</summary>
    public void ApplyVisionDegradation(float saturation, float brightness)
    {
        Godot.Environment environment = _worldEnvironment.Environment;
        environment.AdjustmentSaturation = saturation;
        environment.AdjustmentBrightness = brightness;
    }
}
