using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace FortIdle.Editor
{
    public class FortArtImport : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (assetPath.IndexOf("/FortIdle/Resources/Art/") < 0)
                return;

            ModelImporter importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            bool actor = IsActor(assetPath);
            importer.importAnimation = actor;
            importer.animationType = actor ? ModelImporterAnimationType.Legacy : ModelImporterAnimationType.None;
            if (actor)
            {
                importer.generateMeshLods = false;
                NameClips(importer);
            }
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            // Blender nature files are Z-up with a single -90 X on the model node.
            // Bake that once here. Spawn used to zero the node and then guess again.
            if (BakesPlantAxis(assetPath))
                importer.bakeAxisConversion = true;
            if (WantsLod(assetPath))
                importer.generateMeshLods = true;
        }

        static bool WantsLod(string path)
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            return file == "Shambler" || file == "Brute"
                || file == "CommonTree" || file == "CommonTreeB" || file == "Birch" || file == "Pine";
        }

        static bool IsActor(string path)
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            return file == "Hero" || file == "Worker";
        }

        static void NameClips(ModelImporter importer)
        {
            ModelImporterClipAnimation[] takes = importer.defaultClipAnimations;
            if (takes == null || takes.Length == 0)
                return;
            for (int i = 0; i < takes.Length; i++)
            {
                string name = takes[i].name;
                int bar = name.LastIndexOf('|');
                if (bar >= 0 && bar < name.Length - 1)
                    name = name.Substring(bar + 1);
                takes[i].name = name;
                bool loop = name == "Idle" || name == "Walk" || name == "Walk_Carry";
                takes[i].loopTime = loop;
                takes[i].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            }

            importer.clipAnimations = takes;
        }

        static bool BakesPlantAxis(string path)
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            return file == "CommonTree" || file == "CommonTreeB" || file == "Birch" || file == "Pine"
                || file == "Stump" || file == "Bush" || file == "GrassTuft" || file == "Flowers";
        }

        void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] clips)
        {
            if (assetPath.IndexOf("/FortIdle/Resources/Art/") < 0)
                return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                return;
            material.shader = shader;
            material.SetFloat("_Smoothness", 0.12f);
            material.SetFloat("_Metallic", 0f);

            Vector4 color;
            if (description.TryGetProperty("DiffuseColor", out color))
                material.SetColor("_BaseColor", new Color(color.x, color.y, color.z, 1f));

            TexturePropertyDescription texture;
            if (!description.TryGetProperty("DiffuseColor", out texture))
                return;
            if (string.IsNullOrEmpty(texture.path) && string.IsNullOrEmpty(texture.relativePath))
                return;

            Texture2D tex = texture.texture as Texture2D;
            if (tex == null)
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FortIdle/Resources/Art/Textures/colormap.png");
            if (tex != null)
                material.SetTexture("_BaseMap", tex);
        }
    }
}
