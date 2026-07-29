using ElderWorld.Core.Thermal;
using ElderWorld.Core.Time;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// The development time lever from docs/07 §2:
/// <i>"A time-scale debug lever (up to several thousand ×) available in dev builds and
/// never in shipping ones."</i>
/// <para>
/// At 1:1 nobody can playtest a year, or even a night, without one. The whole
/// simulation is written in fixed world-time steps precisely so that this changes how
/// fast you observe the world and nothing else — see <see cref="FixedStepAccumulator"/>
/// and the invariance tests.
/// </para>
/// <para>
/// <b>Never in a shipping build.</b> The guard is <see cref="OS.IsDebugBuild"/>, so an
/// exported release build has no lever at all; docs/06 §3 requires the clock be
/// authoritative and monotonic, and a player-facing accelerator would break that.
/// Readouts go to the console, never to the screen — docs/07 §1 says no UI whatsoever,
/// and that includes debug text.
/// </para>
/// </summary>
public partial class DevTimeControls : Node
{
    private static readonly double[] Scales = [1.0, 30.0, 300.0, 1500.0, 6000.0];

    private WorldSimulation _simulation = null!;
    private PlayerBody _body = null!;
    private int _scaleIndex;

    public override void _Ready()
    {
        if (!OS.IsDebugBuild())
        {
            SetProcessInput(false);
            SetProcess(false);
            return;
        }

        _simulation = GetNode<WorldSimulation>("/root/Greybox/WorldSimulation");
        _body = GetNode<PlayerBody>("/root/Greybox/Player/Body");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

        switch (key.PhysicalKeycode)
        {
            case >= Key.Key1 and <= Key.Key5:
                // Godot's Key enum is long-backed, so the offset needs narrowing.
                _scaleIndex = (int)(key.PhysicalKeycode - Key.Key1);
                _simulation.Clock.TimeScale = Scales[_scaleIndex];
                GD.Print($"[dev] time scale {Scales[_scaleIndex]:N0}×");
                break;

            case Key.F1:
                ReportWorld();
                break;

            case Key.F2:
                ReportBody();
                break;
        }
    }

    private void ReportWorld()
    {
        WorldClock clock = _simulation.Clock;
        var sample = _body.Environment;

        double nightHours = SolarPosition.NightLengthHours(clock.YearPhase);

        GD.Print(
            $"[world] year {clock.Year} day {clock.Day % (long)CretaceousCalendar.DaysPerYear} " +
            $"time {clock.TimeOfDay:F3} (noon = 0.5)  season {clock.YearPhase:F3}  moon {clock.LunarPhase:F2}\n" +
            $"        air {sample.AirTemperatureC,6:F1} °C   sky {sample.SkyTemperatureC,6:F1} °C   " +
            $"wind {sample.WindSpeedMetresPerSecond,4:F1} m/s   cloud {sample.CloudCover:P0}\n" +
            $"        night is {nightHours:F1} h long   light {sample.IlluminanceLux:F3} lux   " +
            $"snow {sample.SnowCoverFraction:P0}   {sample.Precipitation}");
    }

    private void ReportBody()
    {
        ThermalState state = _body.State;

        GD.Print(
            $"[body]  core {state.CoreTemperatureC:F2} °C  skin {state.MeanSkinTemperatureC:F1} °C  " +
            $"({state.Stage})\n" +
            $"        head {state.SkinTemperature(BodyZone.Head):F1}  " +
            $"hands {state.SkinTemperature(BodyZone.Hands):F1}  " +
            $"feet {state.SkinTemperature(BodyZone.Feet):F1}   dexterity {state.Dexterity:P0}\n" +
            $"        metabolic {state.MetabolicHeatWatts:F0} W (shivering {state.ShiveringHeatWatts:F0} W)  " +
            $"net {state.NetHeatBalanceWatts:+#;-#;0} W   fire {_body.FireIrradiance:F0} W/m²\n" +
            $"        reserve {state.EnergyReserveKcal:F0} kcal   clothing wet {_body.Clothing.Wetness:P0}   " +
            $"frostbite hands {state.Frostbite(BodyZone.Hands):P0}");
    }
}
