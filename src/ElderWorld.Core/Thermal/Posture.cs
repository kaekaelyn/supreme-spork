namespace ElderWorld.Core.Thermal;

/// <summary>
/// What the body is doing, which decides both how much heat it makes and how much
/// of it is pressed against cold ground.
/// </summary>
public enum Posture
{
    /// <summary>Upright. Minimal ground contact, maximum exposure to wind and sky.</summary>
    Standing = 0,

    /// <summary>Sitting, as at a fire through a long night. Some ground contact.</summary>
    Sitting = 1,

    /// <summary>
    /// Lying down. The most ground contact and the most conductive loss.
    /// <para>
    /// Sleeping on bare ground is a way to die that no other survival game models,
    /// and it is why a bough bed is a real piece of technology rather than décor.
    /// </para>
    /// </summary>
    Lying = 2,

    /// <summary>
    /// Curled tight, limbs tucked. Cuts exposed surface area substantially — the
    /// posture every cold animal adopts, and it is available to the player for free
    /// if they think of it.
    /// </summary>
    Curled = 3,
}

/// <summary>Geometry constants per posture.</summary>
public static class Postures
{
    /// <summary>Fraction of body surface in contact with the ground.</summary>
    public static double GroundContactFraction(Posture posture) => posture switch
    {
        Posture.Standing => 0.02,
        Posture.Sitting => 0.14,
        Posture.Lying => 0.32,
        Posture.Curled => 0.24,
        _ => throw new ArgumentOutOfRangeException(nameof(posture)),
    };

    /// <summary>
    /// Fraction of body surface still exposed to air and sky. Curling up hides a
    /// real amount of skin from the sky, which is why animals do it.
    /// </summary>
    public static double ExposedFraction(Posture posture) => posture switch
    {
        Posture.Standing => 0.98,
        Posture.Sitting => 0.86,
        Posture.Lying => 0.68,
        Posture.Curled => 0.62,
        _ => throw new ArgumentOutOfRangeException(nameof(posture)),
    };

    /// <summary>Metabolic rate floor for the posture, in met (1 met ≈ 58.2 W/m²).</summary>
    public static double BaseActivityMet(Posture posture) => posture switch
    {
        Posture.Standing => 1.2,
        Posture.Sitting => 1.0,
        Posture.Lying => 0.8,
        Posture.Curled => 0.8,
        _ => throw new ArgumentOutOfRangeException(nameof(posture)),
    };
}
