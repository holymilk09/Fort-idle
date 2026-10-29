using UnityEngine;

namespace FortIdle
{
    // Mesh triangles actually referenced by enabled renderers, split the way the night-5 budget is read.
    public static class TriReport
    {
        public static void Log(string name)
        {
            long trees = 0;
            long monsters = 0;
            long fort = 0;
            long terrain = 0;
            long other = 0;
            int renderers = 0;
            Renderer[] all = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Renderer renderer = all[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;
                int tris = Count(renderer);
                if (tris <= 0)
                    continue;
                renderers += 1;
                switch (Bucket(renderer.transform))
                {
                    case 0:
                        trees += tris;
                        break;
                    case 1:
                        monsters += tris;
                        break;
                    case 2:
                        fort += tris;
                        break;
                    case 3:
                        terrain += tris;
                        break;
                    default:
                        other += tris;
                        break;
                }
            }

            Debug.Log("FORTIDLE tris name=" + name
                + " trees=" + trees
                + " monsters=" + monsters
                + " fort=" + fort
                + " terrain=" + terrain
                + " other=" + other
                + " renderers=" + renderers);
        }

        static int Count(Renderer renderer)
        {
            Mesh mesh = null;
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null)
                mesh = filter.sharedMesh;
            SkinnedMeshRenderer skin = renderer as SkinnedMeshRenderer;
            if (skin != null)
                mesh = skin.sharedMesh;
            if (mesh == null)
                return 0;
            int sum = 0;
            int sub = mesh.subMeshCount;
            for (int i = 0; i < sub; i++)
                sum += (int)(mesh.GetIndexCount(i) / 3);
            if (!loggedLod && renderer.name == "Shambler")
            {
                loggedLod = true;
                Debug.Log("FORTIDLE lod mesh=" + mesh.name + " lods=" + mesh.lodCount + " tris=" + sum);
            }
            return sum;
        }

        static bool loggedLod;

        static int Bucket(Transform transform)
        {
            Transform cursor = transform;
            while (cursor != null)
            {
                string name = cursor.name;
                if (name == "Tree" || name == "BackdropTree" || name == "Canopy" || name == "Stump"
                    || name == "Bush" || name == "GrassTuft" || name == "Flowers" || name == "Pine"
                    || name == "Birch" || name == "CommonTree" || name == "CommonTreeB")
                    return 0;
                if (name == "Shambler" || name == "Brute" || name == "Treant" || name == "Boar")
                    return 1;
                if (name == "Wall" || name == "Gate" || name == "Turret" || name == "Watchtower"
                    || name == "Hut" || name == "Fire" || name == "Spikes" || name == "Hero" || name == "Worker")
                    return 2;
                if (name == "Ground" || name == "GroundScatter" || name == "Terrain")
                    return 3;
                cursor = cursor.parent;
            }

            return 4;
        }
    }
}
