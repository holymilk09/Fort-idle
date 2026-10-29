using UnityEngine;
using UnityEngine.Rendering;

namespace FortIdle
{
    // One baked grass/dirt texture and a handful of instanced tufts. No terrain system.
    public static class FortGround
    {
        public static void Build(FortIdleConfig config)
        {
            Texture2D map = Paint(config);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null && FortWorld.Look != null && FortWorld.Look.Grass != null)
                shader = FortWorld.Look.Grass.shader;
            Material material = new Material(shader);
            material.name = "FortGround";
            material.enableInstancing = true;
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", map);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.05f);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);

            GameObject ground = new GameObject("GroundMesh");
            ground.transform.position = new Vector3(0f, 0.02f, 0f);
            MeshFilter filter = ground.AddComponent<MeshFilter>();
            MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
            filter.sharedMesh = Quad(config.layout.groundExtent);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            FortWorld.GroundRoot = ground.transform;
            Scatter(config, ground.transform);
        }

        static Mesh Quad(float extent)
        {
            Mesh mesh = new Mesh();
            mesh.name = "GroundQuad";
            float half = extent * 0.5f;
            mesh.vertices = new[]
            {
                new Vector3(-half, 0f, -half),
                new Vector3(-half, 0f, half),
                new Vector3(half, 0f, half),
                new Vector3(half, 0f, -half)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            return mesh;
        }

        static Texture2D Paint(FortIdleConfig config)
        {
            const int Size = 512;
            Color[] pixels = new Color[Size * Size];
            float extent = config.layout.groundExtent;
            Color grassA = config.presentation.grass;
            Color grassB = Color.Lerp(grassA, new Color(0.42f, 0.5f, 0.24f, 1f), 0.55f);
            Color dirt = new Color(0.58f, 0.4f, 0.2f, 1f);
            Color forest = Color.Lerp(grassA, new Color(0.16f, 0.2f, 0.12f, 1f), 0.72f);
            float clear = config.layout.clearingDiameter * 0.5f;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = (x + 0.5f) / Size;
                    float v = (y + 0.5f) / Size;
                    float wx = (u - 0.5f) * extent;
                    float wz = (v - 0.5f) * extent;
                    float noise = Mathf.PerlinNoise(wx * 0.11f + 3f, wz * 0.11f + 8f);
                    Color color = Color.Lerp(grassA, grassB, noise);
                    float path = Dirt(config, wx, wz);
                    color = Color.Lerp(color, dirt, path);
                    float radius = Mathf.Sqrt(wx * wx + wz * wz);
                    float edge = Mathf.InverseLerp(clear * 0.72f, clear * 1.35f, radius);
                    edge = edge * edge * (3f - 2f * edge);
                    color = Color.Lerp(color, forest, edge);
                    pixels[y * Size + x] = color;
                }
            }

            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, true);
            texture.name = "GroundAlbedo";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        static float Dirt(FortIdleConfig config, float x, float z)
        {
            float amount = 0f;
            Stamp(ref amount, x, z, config.fire.position, 4.2f);
            Stamp(ref amount, x, z, config.layout.wallPadPosition, 3.2f);
            Stamp(ref amount, x, z, config.layout.turretPadPosition, 2.2f);
            Stamp(ref amount, x, z, config.layout.hutPadPosition, 2.4f);
            Stamp(ref amount, x, z, config.defenses.gatePosition, 2.2f);
            Stamp(ref amount, x, z, config.defenses.towerPosition, 2.1f);
            Stamp(ref amount, x, z, config.defenses.spikePosition, 1.8f);
            Stamp(ref amount, x, z, config.player.spawnPosition, 2.6f);
            Lane(ref amount, x, z, config.player.spawnPosition, config.layout.wallPadPosition, 1.35f);
            Lane(ref amount, x, z, config.layout.wallPadPosition, config.layout.turretPadPosition, 1.15f);
            Lane(ref amount, x, z, config.fire.position, config.layout.hutPadPosition, 1.2f);
            Lane(ref amount, x, z, config.layout.wallPadPosition, config.defenses.gatePosition, 1.15f);
            return Mathf.Clamp01(amount);
        }

        static void Stamp(ref float amount, float x, float z, Vector3 point, float radius)
        {
            float dx = x - point.x;
            float dz = z - point.z;
            float d = Mathf.Sqrt(dx * dx + dz * dz);
            float band = 1f - Mathf.Clamp01(d / radius);
            band = band * band;
            if (band > amount)
                amount = band;
        }

        static void Lane(ref float amount, float x, float z, Vector3 a, Vector3 b, float radius)
        {
            Vector2 p = new Vector2(x, z);
            Vector2 start = new Vector2(a.x, a.z);
            Vector2 end = new Vector2(b.x, b.z);
            Vector2 span = end - start;
            float length = span.magnitude;
            if (length < 0.01f)
                return;
            float t = Mathf.Clamp01(Vector2.Dot(p - start, span) / (length * length));
            Vector2 closest = start + span * t;
            float d = Vector2.Distance(p, closest);
            float band = 1f - Mathf.Clamp01(d / radius);
            band *= 0.85f;
            if (band > amount)
                amount = band;
        }

        static void Scatter(FortIdleConfig config, Transform parent)
        {
            Shader shader = Shader.Find("FortIdle/Scatter");
            if (shader == null)
                return;
            Material material = new Material(shader);
            material.name = "FortScatter";
            material.enableInstancing = true;
            Mesh tuft = TuftMesh();
            Mesh rock = RockMesh();
            const int Tufts = 70;
            const int Rocks = 16;
            Matrix4x4[] tuftMatrices = new Matrix4x4[Tufts];
            Matrix4x4[] rockMatrices = new Matrix4x4[Rocks];
            float clear = config.layout.clearingDiameter * 0.5f;
            int tuftCount = 0;
            int rockCount = 0;
            Random.State state = Random.state;
            Random.InitState(19);
            for (int i = 0; i < 220 && (tuftCount < Tufts || rockCount < Rocks); i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(clear * 0.55f, clear * 1.15f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                if (Dirt(config, x, z) > 0.45f)
                    continue;
                Vector3 position = new Vector3(x, 0.02f, z);
                if (tuftCount < Tufts && i % 4 != 0)
                {
                    float yaw = Random.Range(0f, 360f);
                    float scale = Random.Range(0.35f, 0.7f);
                    tuftMatrices[tuftCount] = Matrix4x4.TRS(position, Quaternion.Euler(0f, yaw, 0f), new Vector3(scale, scale, scale));
                    tuftCount += 1;
                }
                else if (rockCount < Rocks)
                {
                    float yaw = Random.Range(0f, 360f);
                    float scale = Random.Range(0.18f, 0.42f);
                    rockMatrices[rockCount] = Matrix4x4.TRS(position, Quaternion.Euler(0f, yaw, Random.Range(-8f, 8f)), new Vector3(scale, scale * 0.6f, scale));
                    rockCount += 1;
                }
            }

            Random.state = state;
            GameObject host = new GameObject("GroundScatter");
            host.transform.SetParent(parent, false);
            GroundScatter scatter = host.AddComponent<GroundScatter>();
            scatter.Init(tuft, rock, material, tuftMatrices, tuftCount, rockMatrices, rockCount);
        }

        static Mesh TuftMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "GrassTuft";
            Vector3[] vertices =
            {
                new Vector3(-0.15f, 0f, 0f), new Vector3(0.15f, 0f, 0f), new Vector3(0f, 0.55f, 0f),
                new Vector3(0f, 0f, -0.15f), new Vector3(0f, 0f, 0.15f), new Vector3(0f, 0.5f, 0f)
            };
            Color[] colors =
            {
                new Color(0.25f, 0.38f, 0.14f), new Color(0.25f, 0.38f, 0.14f), new Color(0.55f, 0.68f, 0.28f),
                new Color(0.22f, 0.34f, 0.12f), new Color(0.22f, 0.34f, 0.12f), new Color(0.48f, 0.62f, 0.24f)
            };
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = new[] { 0, 2, 1, 3, 5, 4 };
            mesh.RecalculateNormals();
            return mesh;
        }

        static Mesh RockMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "Pebble";
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.4f),
                new Vector3(0.5f, 0f, -0.3f),
                new Vector3(0.4f, 0f, 0.5f),
                new Vector3(-0.4f, 0f, 0.4f),
                new Vector3(0f, 0.45f, 0f)
            };
            Color gray = new Color(0.45f, 0.43f, 0.38f);
            mesh.colors = new[] { gray, gray, gray, gray, Color.Lerp(gray, Color.white, 0.25f) };
            mesh.triangles = new[] { 0, 1, 4, 1, 2, 4, 2, 3, 4, 3, 0, 4, 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    public class GroundScatter : MonoBehaviour
    {
        Mesh tuft;
        Mesh rock;
        Material material;
        Matrix4x4[] tufts;
        Matrix4x4[] rocks;
        int tuftCount;
        int rockCount;

        public void Init(Mesh tuftMesh, Mesh rockMesh, Material shared, Matrix4x4[] tuftMatrices, int tufts, Matrix4x4[] rockMatrices, int rockTotal)
        {
            tuft = tuftMesh;
            rock = rockMesh;
            material = shared;
            this.tufts = tuftMatrices;
            tuftCount = tufts;
            rocks = rockMatrices;
            rockCount = rockTotal;
        }

        void Update()
        {
            if (material == null)
                return;
            if (tuft != null && tuftCount > 0)
                Graphics.DrawMeshInstanced(tuft, 0, material, tufts, tuftCount, null, ShadowCastingMode.Off, false);
            if (rock != null && rockCount > 0)
                Graphics.DrawMeshInstanced(rock, 0, material, rocks, rockCount, null, ShadowCastingMode.Off, false);
        }
    }
}
