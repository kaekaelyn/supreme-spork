using Godot;

namespace ElderWorld.Game;

/// <summary>
/// Every material in Stage 1, and there are five of them.
/// <para>
/// docs/09 §8: <i>"Stage 1 — Greybox, no art at all... The single most common solo
/// failure is making art before the game is fun. Resist it."</i> So these are flat
/// untextured greys, distinguishable enough to read the shapes and deliberately
/// ugly enough that nobody mistakes them for a direction.
/// </para>
/// </summary>
public static class GreyboxMaterials
{
    /// <summary>Bare volcanic ground.</summary>
    public static StandardMaterial3D Ground { get; } = Flat(new Color(0.34f, 0.33f, 0.31f), roughness: 0.95f);

    /// <summary>Conifer trunks.</summary>
    public static StandardMaterial3D Trunk { get; } = Flat(new Color(0.24f, 0.22f, 0.20f), roughness: 0.9f);

    /// <summary>Conifer crowns.</summary>
    public static StandardMaterial3D Canopy { get; } = Flat(new Color(0.19f, 0.24f, 0.20f), roughness: 0.95f);

    /// <summary>Hearth stones.</summary>
    public static StandardMaterial3D Stone { get; } = Flat(new Color(0.40f, 0.39f, 0.38f), roughness: 0.85f);

    /// <summary>
    /// Hot volcanic ground at a fumarole margin — the one thing in the greybox that
    /// is allowed to glow, because it is the only affordance a naked player has on
    /// day one and they have to be able to find it (docs/02 §4).
    /// </summary>
    public static StandardMaterial3D Fumarole { get; } = Emissive(new Color(0.35f, 0.15f, 0.10f), new Color(0.9f, 0.35f, 0.12f), 0.6f);

    private static StandardMaterial3D Flat(Color colour, float roughness) => new()
    {
        AlbedoColor = colour,
        Roughness = roughness,
        Metallic = 0.0f,
        SpecularMode = BaseMaterial3D.SpecularModeEnum.Disabled,
    };

    private static StandardMaterial3D Emissive(Color albedo, Color emission, float energy)
    {
        StandardMaterial3D material = Flat(albedo, roughness: 0.9f);
        material.EmissionEnabled = true;
        material.Emission = emission;
        material.EmissionEnergyMultiplier = energy;
        return material;
    }
}
