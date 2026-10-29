using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FortIdle
{
    // Runtime loader for the CC0 meshes in Resources/Art. Gameplay colliders stay primitive.
    public static class FortArt
    {
        const string Root = "Art/";
        public static readonly Color IronTint = new Color(0.18f, 0.22f, 0.28f, 1f);
        public static readonly Color TreantTint = new Color(0.62f, 0.78f, 0.55f, 1f);

        static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        static readonly Dictionary<string, bool> missing = new Dictionary<string, bool>();
        static readonly Dictionary<int, Material> shared = new Dictionary<int, Material>();
        static Shader lit;
        static Texture2D colormap;

        public static bool Has(string id)
        {
            return Load(id) != null;
        }

        public static GameObject Load(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            GameObject found;
            if (prefabs.TryGetValue(id, out found))
                return found;
            if (missing.ContainsKey(id))
                return null;
            found = Resources.Load<GameObject>(Root + id);
            if (found == null)
            {
                missing[id] = true;
                return null;
            }

            prefabs[id] = found;
            return found;
        }

        public static string TreeId(int index)
        {
            int pick = Mathf.Abs(index) % 3;
            if (pick == 1 && Has("CommonTreeB"))
                return "CommonTreeB";
            if (pick == 2 && Has("Birch"))
                return "Birch";
            return "CommonTree";
        }

        public static GameObject Stand(Transform parent, string id, float height)
        {
            return Stand(parent, id, height, Color.white, 0f, false, false);
        }

        public static GameObject Stand(Transform parent, string id, float height, Color tint, float metallic, bool replaceColor, bool useAtlas)
        {
            GameObject piece = Create(id);
            if (piece == null)
                return null;
            piece.transform.SetParent(parent, false);
            piece.transform.localScale = Vector3.one;
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localRotation = Quaternion.identity;
            Paint(piece, tint, metallic, replaceColor, useAtlas);
            if (IsPlant(id))
                AlignTrunk(piece, parent);
            if (id == "GrassTuft" || id == "Flowers" || id == "Bush" || id == "Stump" || id == "Rock" || id == "RockMoss")
                CastShadows(piece, false);
            Bounds sized = RenderBounds(piece);
            float span = Mathf.Max(sized.size.y, 0.0001f);
            float scale = height / span;
            piece.transform.localScale = Vector3.one * scale;
            Bounds placed = RenderBounds(piece);
            Vector3 origin = parent != null ? parent.position : Vector3.zero;
            piece.transform.position += new Vector3(origin.x - placed.center.x, origin.y - placed.min.y, origin.z - placed.center.z);
            if (id == "Hero" || id == "Worker")
            {
                FortAnim.Attach(piece);
                PoseWatch.Attach(piece, id);
            }
            return piece;
        }

        public static bool IsPlant(string id)
        {
            return id == "CommonTree" || id == "CommonTreeB" || id == "Birch" || id == "Pine"
                || id == "Stump" || id == "Bush" || id == "GrassTuft" || id == "Flowers";
        }

        // The Blender mesh's trunk is the axis that starts at the origin and runs one way.
        // Canopy width can be larger than height, so "longest bounds axis" is the wrong test.
        static void AlignTrunk(GameObject piece, Transform parent)
        {
            Vector3 trunk = TrunkWorld(piece);
            Vector3 want = parent != null ? parent.up : Vector3.up;
            if (trunk.sqrMagnitude < 0.0001f)
                return;
            float error = Vector3.Angle(trunk, want);
            if (error > 90f)
                error = 180f - error;
            if (error <= 1f)
                return;
            piece.transform.rotation = Quaternion.FromToRotation(trunk, want) * piece.transform.rotation;
            Debug.Log("FORTIDLE tree-align name=" + piece.name + " error=" + error.ToString("0.0"));
        }

        public static void CastShadows(GameObject piece, bool cast)
        {
            if (piece == null)
                return;
            Renderer[] renderers = piece.GetComponentsInChildren<Renderer>(true);
            ShadowCastingMode mode = cast ? ShadowCastingMode.On : ShadowCastingMode.Off;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;
                renderers[i].shadowCastingMode = mode;
            }
        }

        public static float TrunkDegrees(GameObject piece)
        {
            Vector3 trunk = TrunkWorld(piece);
            if (trunk.sqrMagnitude < 0.0001f)
                return 90f;
            float angle = Vector3.Angle(trunk, Vector3.up);
            if (angle > 90f)
                angle = 180f - angle;
            return angle;
        }

        static Vector3 TrunkWorld(GameObject piece)
        {
            MeshFilter[] filters = piece.GetComponentsInChildren<MeshFilter>(true);
            Vector3 best = Vector3.up;
            float bestScore = -1f;
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh == null)
                    continue;
                Vector3 local = GroundAxis(mesh.bounds);
                float score = GroundScore(mesh.bounds);
                if (score <= bestScore)
                    continue;
                bestScore = score;
                best = filters[i].transform.TransformDirection(local);
            }

            return best.sqrMagnitude < 0.0001f ? Vector3.up : best.normalized;
        }

        static Vector3 GroundAxis(Bounds bounds)
        {
            float x = GroundScore(bounds.min.x, bounds.max.x);
            float y = GroundScore(bounds.min.y, bounds.max.y);
            float z = GroundScore(bounds.min.z, bounds.max.z);
            if (y >= z && y >= x)
                return bounds.max.y >= -bounds.min.y ? Vector3.up : Vector3.down;
            if (z >= x)
                return bounds.max.z >= -bounds.min.z ? Vector3.forward : Vector3.back;
            return bounds.max.x >= -bounds.min.x ? Vector3.right : Vector3.left;
        }

        static float GroundScore(Bounds bounds)
        {
            return Mathf.Max(
                GroundScore(bounds.min.x, bounds.max.x),
                Mathf.Max(GroundScore(bounds.min.y, bounds.max.y), GroundScore(bounds.min.z, bounds.max.z)));
        }

        static float GroundScore(float min, float max)
        {
            float size = max - min;
            if (size <= 0.001f)
                return 0f;
            float nearOrigin = Mathf.Min(Mathf.Abs(min), Mathf.Abs(max));
            if (nearOrigin > size * 0.35f)
                return 0f;
            return size;
        }

        public static GameObject Box(Transform parent, string id, Vector3 size, Color tint, float metallic, bool replaceColor, bool useAtlas)
        {
            if (Load(id) == null)
                return null;
            GameObject box = new GameObject(id);
            if (parent != null)
                box.transform.SetParent(parent, false);
            box.transform.localRotation = Quaternion.identity;
            box.transform.localPosition = Vector3.zero;
            box.transform.localScale = size;
            if (!Fill(box.transform, id, 1, tint, metallic, replaceColor, useAtlas))
            {
                Object.Destroy(box);
                return null;
            }

            return box;
        }

        public static bool Fill(Transform box, string id, int tiles, bool useAtlas)
        {
            return Fill(box, id, tiles, Color.white, 0f, false, useAtlas);
        }

        public static bool Fill(Transform box, string id, int tiles, Color tint, float metallic, bool replaceColor, bool useAtlas)
        {
            if (box == null || Load(id) == null)
                return false;
            if (tiles < 1)
                tiles = 1;
            for (int i = box.childCount - 1; i >= 0; i--)
            {
                Transform child = box.GetChild(i);
                if (child.name == "Dress")
                    Object.DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < tiles; i++)
            {
                GameObject piece = Create(id);
                piece.name = "Dress";
                piece.transform.SetParent(box, false);
                piece.transform.localRotation = Quaternion.identity;
                piece.transform.localScale = Vector3.one;
                piece.transform.localPosition = Vector3.zero;
                Paint(piece, tint, metallic, replaceColor, useAtlas);
                Bounds mesh = LocalBounds(piece);
                float slice = 1f / tiles;
                Vector3 size = new Vector3(slice, 1f, 1f);
                Vector3 center = new Vector3(-0.5f + slice * (i + 0.5f), 0f, 0f);
                Vector3 safe = new Vector3(
                    Mathf.Max(mesh.size.x, 0.0001f),
                    Mathf.Max(mesh.size.y, 0.0001f),
                    Mathf.Max(mesh.size.z, 0.0001f));
                Vector3 scale = new Vector3(size.x / safe.x, size.y / safe.y, size.z / safe.z);
                piece.transform.localScale = scale;
                piece.transform.localPosition = center - Vector3.Scale(mesh.center, scale);
            }

            return true;
        }

        static GameObject Create(string id)
        {
            GameObject prefab = Load(id);
            if (prefab == null)
                return null;
            GameObject piece = Object.Instantiate(prefab);
            piece.name = id;
            return piece;
        }

        static void Paint(GameObject piece, Color tint, float metallic, bool replaceColor, bool useAtlas)
        {
            Collider[] colliders = piece.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                Object.DestroyImmediate(colliders[i]);

            Texture2D atlas = useAtlas ? Atlas() : null;
            Renderer[] renderers = piece.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                Material[] sources = renderers[r].sharedMaterials;
                Material[] painted = new Material[sources.Length];
                for (int i = 0; i < sources.Length; i++)
                    painted[i] = Convert(sources[i], tint, metallic, replaceColor, atlas);
                renderers[r].sharedMaterials = painted;
                Prop.ApplyShadows(renderers[r]);
            }
        }

        static Material Convert(Material source, Color tint, float metallic, bool replaceColor, Texture2D atlas)
        {
            Color baseColor = Color.white;
            Texture texture = atlas;
            if (source != null)
            {
                if (!replaceColor)
                {
                    if (source.HasProperty("_BaseColor"))
                        baseColor = source.GetColor("_BaseColor");
                    else if (source.HasProperty("_Color"))
                        baseColor = source.GetColor("_Color");
                    baseColor = Lift(baseColor);
                }

                if (texture == null)
                {
                    if (source.HasProperty("_BaseMap"))
                        texture = source.GetTexture("_BaseMap");
                    if (texture == null && source.HasProperty("_MainTex"))
                        texture = source.GetTexture("_MainTex");
                }
            }

            Color final = new Color(
                Mathf.Clamp01(baseColor.r * tint.r),
                Mathf.Clamp01(baseColor.g * tint.g),
                Mathf.Clamp01(baseColor.b * tint.b),
                1f);
            int key = final.GetHashCode();
            key = (key * 397) ^ metallic.GetHashCode();
            if (texture != null)
                key = (key * 397) ^ texture.GetInstanceID();
            Material cached;
            if (shared.TryGetValue(key, out cached) && cached != null)
                return cached;

            Shader shader = Lit();
            Material material = new Material(shader);
            material.name = "FortArt";
            material.enableInstancing = true;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", final);
            if (texture != null && material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.12f);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            shared[key] = material;
            return material;
        }

        static Color Lift(Color color)
        {
            float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            if (max < 0.02f || max > 0.55f)
                return color;
            const float Power = 0.5f;
            return new Color(Mathf.Pow(color.r, Power), Mathf.Pow(color.g, Power), Mathf.Pow(color.b, Power), 1f);
        }

        static Shader Lit()
        {
            if (lit == null)
                lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null && FortWorld.Look != null && FortWorld.Look.Grass != null)
                lit = FortWorld.Look.Grass.shader;
            return lit;
        }

        static Texture2D Atlas()
        {
            if (colormap == null)
                colormap = Resources.Load<Texture2D>(Root + "Textures/colormap");
            return colormap;
        }

        static Bounds RenderBounds(GameObject piece)
        {
            Renderer[] renderers = piece.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            Bounds bounds = new Bounds(piece.transform.position, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;
                if (!any)
                {
                    bounds = renderers[i].bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            if (!any)
                bounds = LocalBounds(piece);
            return bounds;
        }

        static Bounds LocalBounds(GameObject piece)
        {
            MeshFilter[] filters = piece.GetComponentsInChildren<MeshFilter>(true);
            bool any = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh == null)
                    continue;
                Bounds local = mesh.bounds;
                Vector3 center = local.center;
                Vector3 extents = local.extents;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 corner = center + new Vector3(
                        (c & 1) == 0 ? -extents.x : extents.x,
                        (c & 2) == 0 ? -extents.y : extents.y,
                        (c & 4) == 0 ? -extents.z : extents.z);
                    Vector3 inPiece = piece.transform.InverseTransformPoint(filters[i].transform.TransformPoint(corner));
                    if (!any)
                    {
                        bounds = new Bounds(inPiece, Vector3.zero);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(inPiece);
                    }
                }
            }

            if (!any)
                bounds = new Bounds(Vector3.zero, Vector3.one);
            return bounds;
        }
    }
}
