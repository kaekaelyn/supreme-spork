using Godot;

namespace ElderWorld.Game;

/// <summary>
/// Builds the whole greybox in code.
/// <para>
/// Everything is assembled here rather than saved into a scene file because Stage 1 is
/// pure untextured geometry with no authored assets (docs/09 §8), and because a scene
/// built from code is one a person can read, diff and review — which matters more at
/// this stage than editor convenience. When there is real art, this becomes a proper
/// scene and this file goes away.
/// </para>
/// </summary>
public partial class Bootstrap : Node3D
{
    public override void _Ready()
    {
        Name = "Greybox";

        var simulation = new WorldSimulation { Name = "WorldSimulation" };
        AddChild(simulation);

        var terrain = new GreyboxTerrain { Name = "Terrain" };
        AddChild(terrain);

        var sky = new SkyDriver { Name = "Sky" };
        AddChild(sky);

        // The fumarole: hot volcanic ground, the day-one source of fire, and the only
        // thing in the greybox allowed to glow (docs/02 §4).
        Node3D fumarole = BuildFumarole(terrain, new Vector3(-26.0f, 0.0f, -14.0f));
        AddChild(fumarole);

        var campfire = new Campfire { Name = "Campfire" };
        AddChild(campfire);
        campfire.Position = GroundedAt(terrain, new Vector3(6.0f, 0.0f, 4.0f));

        var player = new PlayerController { Name = "Player" };
        AddChild(player);
        player.Position = GroundedAt(terrain, Vector3.Zero) + new Vector3(0, 0.2f, 0);
        player.Hearth = campfire;
        player.Fumarole = fumarole;

        // Children of the player, added after it is in the tree so their _Ready can
        // resolve absolute paths.
        player.AddChild(new PlayerBody { Name = "Body", Hearth = campfire });
        player.AddChild(new DiegeticFeedback { Name = "Feedback" });
        player.AddChild(new ProceduralAudio { Name = "Audio" });

        sky.Observer = player;
        sky.Terrain = terrain;

        AddChild(new DevTimeControls { Name = "DevTimeControls" });
    }

    private static Vector3 GroundedAt(GreyboxTerrain terrain, Vector3 position)
        => new(position.X, terrain.HeightAt(position), position.Z);

    private static Node3D BuildFumarole(GreyboxTerrain terrain, Vector3 position)
    {
        var vent = new Node3D { Name = "Fumarole", Position = GroundedAt(terrain, position) };

        vent.AddChild(new MeshInstance3D
        {
            Mesh = new CylinderMesh { TopRadius = 1.6f, BottomRadius = 1.9f, Height = 0.24f, RadialSegments = 10 },
            MaterialOverride = GreyboxMaterials.Fumarole,
            Position = new Vector3(0, 0.12f, 0),
        });

        vent.AddChild(new OmniLight3D
        {
            LightColor = new Color(1.0f, 0.42f, 0.16f),
            LightEnergy = 0.55f,
            OmniRange = 7.0f,
            Position = new Vector3(0, 0.4f, 0),
        });

        return vent;
    }
}
