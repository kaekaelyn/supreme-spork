using ElderWorld.Core.Climate;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// The 500 m × 500 m greybox patch of docs/07 §1, in untextured geometry.
/// <para>
/// The forest is not decoration here. Tree density feeds
/// <see cref="SiteAt"/>, which is what tells the thermal model how much wind reaches
/// you and how much cold sky you can see — so walking into the trees genuinely
/// changes how fast you are dying. That is the mechanic docs/02 §1 calls "a legible,
/// learnable map feature", and it has to be testable before any of this has bark on it.
/// </para>
/// </summary>
public partial class GreyboxTerrain : Node3D
{
    /// <summary>Side length of the patch, metres. docs/07 §1 asks for 500 m.</summary>
    public const float PatchSize = 500.0f;

    /// <summary>Grid cells per side. 100 gives a 5 m triangle, plenty for greybox.</summary>
    private const int Resolution = 100;

    /// <summary>Basin floor elevation, metres above sea level (docs/01 §2).</summary>
    private const double BasinFloorElevation = 2000.0;

    private const int TreeCount = 900;

    private readonly FastNoiseLite _heightNoise = new();
    private readonly FastNoiseLite _forestNoise = new();

    private float _maxHeight = 1.0f;

    /// <summary>Terrain height at a world position, metres above the patch datum.</summary>
    public float HeightAt(Vector3 position) => SampleHeight(position.X, position.Z);

    /// <summary>Canopy density at a world position, 0–1.</summary>
    public float ForestDensityAt(Vector3 position) => SampleForest(position.X, position.Z);

    public override void _Ready()
    {
        _heightNoise.Seed = 1;
        _heightNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        _heightNoise.Frequency = 0.0035f;
        _heightNoise.FractalOctaves = 4;

        _forestNoise.Seed = 2;
        _forestNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        _forestNoise.Frequency = 0.006f;
        _forestNoise.FractalOctaves = 2;

        BuildGround();
        ScatterForest();
    }

    private float SampleHeight(float x, float z)
    {
        // A shallow basin: low in the middle, rising toward the rim, with a ridge on
        // one side to give somewhere genuinely exposed to stand.
        float distance = new Vector2(x, z).Length() / (PatchSize * 0.5f);
        float basin = Mathf.Pow(Mathf.Clamp(distance, 0.0f, 1.4f), 2.0f) * 34.0f;
        float relief = _heightNoise.GetNoise2D(x, z) * 12.0f;
        float ridge = Mathf.Max(0.0f, (x - PatchSize * 0.30f) / 40.0f) * 8.0f;

        return basin + relief + ridge;
    }

    private float SampleForest(float x, float z)
    {
        float raw = (_forestNoise.GetNoise2D(x, z) + 1.0f) * 0.5f;

        // Treeline: the ridges are above it, which is why they are cold and why you
        // still have to go up there for stone (docs/06 §4).
        float height = SampleHeight(x, z);
        float treeline = Mathf.Clamp(1.0f - (height - 28.0f) / 20.0f, 0.0f, 1.0f);

        return Mathf.Clamp(raw * 1.35f, 0.0f, 1.0f) * treeline;
    }

    /// <summary>
    /// The site context at a world position — elevation, wind exposure and sky view.
    /// <para>
    /// Everything the thermal model needs to know about <i>where</i> you are.
    /// Under closed canopy the wind mostly stops and most of the cold night sky is
    /// hidden; on the open ridge neither is true.
    /// </para>
    /// </summary>
    public SiteContext SiteAt(Vector3 position)
    {
        float height = SampleHeight(position.X, position.Z);
        float forest = SampleForest(position.X, position.Z);

        // Higher ground catches more wind even before the trees thin out.
        float exposure = Mathf.Clamp(height / Mathf.Max(1.0f, _maxHeight), 0.0f, 1.0f);
        double windExposure = (0.15 + 0.85 * (1.0 - forest)) * (0.55 + 0.45 * exposure);

        return new SiteContext
        {
            ElevationMetres = BasinFloorElevation + height,
            WindExposure = windExposure,
            SkyViewFactor = 1.0 - 0.80 * forest,
        }.Normalised();
    }

    private void BuildGround()
    {
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);

        float step = PatchSize / Resolution;
        float origin = -PatchSize * 0.5f;

        for (int gz = 0; gz < Resolution; gz++)
        {
            for (int gx = 0; gx < Resolution; gx++)
            {
                float x0 = origin + gx * step;
                float z0 = origin + gz * step;
                float x1 = x0 + step;
                float z1 = z0 + step;

                Vector3 a = new(x0, SampleHeight(x0, z0), z0);
                Vector3 b = new(x1, SampleHeight(x1, z0), z0);
                Vector3 c = new(x1, SampleHeight(x1, z1), z1);
                Vector3 d = new(x0, SampleHeight(x0, z1), z1);

                _maxHeight = Mathf.Max(_maxHeight, Mathf.Max(a.Y, c.Y));

                surface.AddVertex(a);
                surface.AddVertex(c);
                surface.AddVertex(b);

                surface.AddVertex(a);
                surface.AddVertex(d);
                surface.AddVertex(c);
            }
        }

        surface.GenerateNormals();
        ArrayMesh mesh = surface.Commit();

        var ground = new MeshInstance3D
        {
            Name = "Ground",
            Mesh = mesh,
            MaterialOverride = GreyboxMaterials.Ground,
        };
        AddChild(ground);

        var body = new StaticBody3D { Name = "GroundCollision" };
        body.AddChild(new CollisionShape3D { Shape = mesh.CreateTrimeshShape() });
        AddChild(body);
    }

    /// <summary>
    /// Scatters greybox conifers: a trunk cylinder and a crown cone, instanced.
    /// <para>
    /// Cones and cylinders, and that is deliberate. docs/09 §8 is explicit that
    /// Stage 1 is "pure untextured geometry" and that the temptation to make art
    /// before the game is fun is the commonest way a solo project dies. What these
    /// need to get right is the <i>silhouette and the density</i>, because that is
    /// what the wind and sky-view model reads.
    /// </para>
    /// </summary>
    private void ScatterForest()
    {
        var rng = new RandomNumberGenerator { Seed = 1337 };

        var trunks = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = new CylinderMesh { TopRadius = 0.18f, BottomRadius = 0.34f, Height = 1.0f, RadialSegments = 6 },
        };
        var crowns = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = new CylinderMesh { TopRadius = 0.02f, BottomRadius = 1.0f, Height = 1.0f, RadialSegments = 7 },
        };

        var placements = new List<(Transform3D Trunk, Transform3D Crown)>(TreeCount);
        var collision = new StaticBody3D { Name = "TreeCollision" };

        float half = PatchSize * 0.5f;
        for (int attempt = 0; attempt < TreeCount * 4 && placements.Count < TreeCount; attempt++)
        {
            float x = rng.RandfRange(-half, half);
            float z = rng.RandfRange(-half, half);

            if (rng.Randf() > SampleForest(x, z)) continue;

            float y = SampleHeight(x, z);
            var basePosition = new Vector3(x, y, z);

            float height = rng.RandfRange(9.0f, 19.0f);
            float trunkHeight = height * 0.34f;
            float crownHeight = height * 0.78f;
            float crownRadius = rng.RandfRange(1.5f, 2.7f);

            var trunk = new Transform3D(
                Basis.Identity.Scaled(new Vector3(1, trunkHeight, 1)),
                basePosition + new Vector3(0, trunkHeight * 0.5f, 0));

            var crown = new Transform3D(
                Basis.Identity.Scaled(new Vector3(crownRadius, crownHeight, crownRadius)),
                basePosition + new Vector3(0, trunkHeight + crownHeight * 0.5f, 0));

            placements.Add((trunk, crown));

            collision.AddChild(new CollisionShape3D
            {
                Shape = new CylinderShape3D { Radius = 0.4f, Height = height },
                Position = basePosition + new Vector3(0, height * 0.5f, 0),
            });
        }

        trunks.InstanceCount = placements.Count;
        crowns.InstanceCount = placements.Count;
        for (int i = 0; i < placements.Count; i++)
        {
            trunks.SetInstanceTransform(i, placements[i].Trunk);
            crowns.SetInstanceTransform(i, placements[i].Crown);
        }

        AddChild(new MultiMeshInstance3D
        {
            Name = "Trunks",
            Multimesh = trunks,
            MaterialOverride = GreyboxMaterials.Trunk,
        });
        AddChild(new MultiMeshInstance3D
        {
            Name = "Crowns",
            Multimesh = crowns,
            MaterialOverride = GreyboxMaterials.Canopy,
        });
        AddChild(collision);
    }
}
