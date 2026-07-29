using Godot;

namespace ElderWorld.Game;

/// <summary>
/// The rough audio pass docs/09 §8 asks for in Stage 1, synthesised rather than
/// recorded because Stage 1 ships no assets.
/// <para>
/// docs/09 §6 argues this is worth doing before it feels natural to:
/// <i>"For an asset-constrained project, audio buys more immersion per hour than any
/// visual work... Do a rough audio pass earlier than feels natural — it transforms how
/// the greybox feels and will change design decisions while changing them is still
/// cheap."</i> Three sounds — wind, fire, and your own breathing — are most of what an
/// eleven-hour night actually sounds like, and whether that night is bearable is the
/// entire Stage 1 gate.
/// </para>
/// <para>
/// <b>All of this is placeholder.</b> The real thing is field recording: wind in
/// conifers, boots at three snow depths, fire, and breathing in cold air.
/// </para>
/// </summary>
public partial class ProceduralAudio : Node
{
    private const int MixRate = 22_050;

    private PlayerBody _body = null!;
    private PlayerController _controller = null!;

    private AudioStreamPlayer _wind = null!;
    private AudioStreamGeneratorPlayback _windPlayback = null!;
    private AudioStreamPlayer _fire = null!;
    private AudioStreamGeneratorPlayback _firePlayback = null!;

    private readonly RandomNumberGenerator _rng = new();

    // Filter state, carried between buffers so there are no clicks at the seams.
    private float _windLowPass;
    private float _windBandPass;
    private float _fireLowPass;
    private float _crackleEnvelope;

    private float _windLevel;
    private float _fireLevel;

    public override void _Ready()
    {
        _controller = GetParent<PlayerController>();
        _body = _controller.GetNode<PlayerBody>("Body");
        _rng.Seed = 8675309;

        (_wind, _windPlayback) = CreateGenerator("Wind", volumeDb: -12.0f);
        (_fire, _firePlayback) = CreateGenerator("Fire", volumeDb: -9.0f);
    }

    private (AudioStreamPlayer Player, AudioStreamGeneratorPlayback Playback) CreateGenerator(
        string name, float volumeDb)
    {
        var player = new AudioStreamPlayer
        {
            Name = name,
            Stream = new AudioStreamGenerator { MixRate = MixRate, BufferLength = 0.25f },
            VolumeDb = volumeDb,
            Autoplay = false,
        };
        AddChild(player);
        player.Play();

        return (player, (AudioStreamGeneratorPlayback)player.GetStreamPlayback());
    }

    public override void _Process(double delta)
    {
        // Wind you hear is the wind actually blowing where you are standing, so
        // stepping into the trees genuinely quietens the world.
        float wind = (float)_body.Environment.WindSpeedMetresPerSecond;
        _windLevel = Mathf.Lerp(_windLevel, Mathf.Clamp(wind / 9.0f, 0.02f, 1.0f), (float)delta * 2.0f);

        // Firelight and fire noise both come off the same hearth model.
        float fireKilowatts = (float)((_controller.Hearth?.Hearth.OutputWatts ?? 0.0) / 1000.0);
        float distance = _controller.Hearth is null
            ? 999.0f
            : _controller.GlobalPosition.DistanceTo(_controller.Hearth.GlobalPosition);
        float audible = Mathf.Clamp(fireKilowatts / 12.0f, 0.0f, 1.0f)
                        / Mathf.Max(1.0f, distance * distance * 0.12f);
        _fireLevel = Mathf.Lerp(_fireLevel, Mathf.Clamp(audible, 0.0f, 1.0f), (float)delta * 3.0f);

        FillWind();
        FillFire();
    }

    /// <summary>
    /// Wind: white noise through a low-pass for the body of it and a band-pass for
    /// the hiss through needles, with a slow gust envelope on top.
    /// </summary>
    private void FillWind()
    {
        int frames = _windPlayback.GetFramesAvailable();
        for (int i = 0; i < frames; i++)
        {
            float noise = _rng.RandfRange(-1.0f, 1.0f);

            _windLowPass += (noise - _windLowPass) * 0.010f;
            _windBandPass += (noise - _windBandPass) * 0.22f;

            float body = _windLowPass * 3.2f;
            float hiss = (_windBandPass - _windLowPass) * 0.30f;

            float sample = (body + hiss) * _windLevel * 0.55f;
            sample = Mathf.Clamp(sample, -1.0f, 1.0f);

            _windPlayback.PushFrame(new Vector2(sample, sample));
        }
    }

    /// <summary>
    /// Fire: a low rumble plus randomly triggered crackles with a fast decay. Crude,
    /// and it does the job of making a hearth feel occupied.
    /// </summary>
    private void FillFire()
    {
        int frames = _firePlayback.GetFramesAvailable();
        for (int i = 0; i < frames; i++)
        {
            float noise = _rng.RandfRange(-1.0f, 1.0f);
            _fireLowPass += (noise - _fireLowPass) * 0.020f;

            // Roughly a few dozen crackles a second at full burn.
            if (_rng.Randf() < 0.0016f * _fireLevel)
                _crackleEnvelope = _rng.RandfRange(0.3f, 1.0f);

            _crackleEnvelope *= 0.9986f;
            float crackle = noise * _crackleEnvelope * 0.5f;

            float sample = (_fireLowPass * 2.4f + crackle) * _fireLevel;
            sample = Mathf.Clamp(sample, -1.0f, 1.0f);

            _firePlayback.PushFrame(new Vector2(sample, sample));
        }
    }
}
