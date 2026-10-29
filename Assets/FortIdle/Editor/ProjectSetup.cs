using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace FortIdle.Editor
{
    public static class ProjectSetup
    {
        const string ConfigPath = "Assets/FortIdle/Config/FortIdleConfig.asset";
        const string LitPath = "Assets/FortIdle/Materials/URPLit.mat";
        const string EmissionPath = "Assets/FortIdle/Materials/URPLitEmission.mat";
        const string ScenePath = "Assets/FortIdle/Scenes/Map1.unity";

        [MenuItem("Fort Idle/Setup Map 1")]
        public static void Setup()
        {
            EnsureFolder("Assets/FortIdle");
            EnsureFolder("Assets/FortIdle/Config");
            EnsureFolder("Assets/FortIdle/Materials");
            EnsureFolder("Assets/FortIdle/Scenes");

            FortIdleConfig config = AssetDatabase.LoadAssetAtPath<FortIdleConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FortIdleConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            Material lit = LoadOrCreateLit(LitPath, false);
            Material emission = LoadOrCreateLit(EmissionPath, true);
            AssignPipeline();
            ForceInputManager();

            PlayerSettings.companyName = "FortIdle";
            PlayerSettings.productName = "Fort Idle";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = config.presentation.windowWidth;
            PlayerSettings.defaultScreenHeight = config.presentation.windowHeight;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = config.presentation.runInBackground;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Minimal);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("FortIdle");
            FortIdleBootstrap bootstrap = root.AddComponent<FortIdleBootstrap>();
            bootstrap.config = config;
            bootstrap.litMaterial = lit;
            bootstrap.emissionMaterial = emission;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FORTIDLE SETUP_OK");
        }

        [MenuItem("Fort Idle/Build Linux Player")]
        public static void BuildLinux()
        {
            Setup();
            Build(Path.Combine("Builds", "Linux", "FortIdle.x86_64"), BuildTarget.StandaloneLinux64);
        }

        // Builds the committed Map1 scene as-is (no Setup), so a QA build leaves the scene file untouched.
        [MenuItem("Fort Idle/Build Mac Player")]
        public static void BuildMac()
        {
            Build(Path.Combine("Builds", "Mac", "FortIdle.app"), BuildTarget.StandaloneOSX);
        }

        static void Build(string relativeOutput, BuildTarget target)
        {
            string output = Path.Combine(Directory.GetCurrentDirectory(), relativeOutput);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("FORTIDLE BUILD_FAIL " + report.summary.result);

            Debug.Log("FORTIDLE BUILD_OK " + output + " seconds=" + report.summary.totalTime.TotalSeconds.ToString("0.0"));
        }

        static Material LoadOrCreateLit(string path, bool emission)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new System.Exception("URP Lit shader not found at " + path);

            Material material = new Material(shader);
            material.name = Path.GetFileNameWithoutExtension(path);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", Color.white);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static void AssignPipeline()
        {
            if (GraphicsSettings.defaultRenderPipeline != null && QualitySettings.renderPipeline != null)
                return;

            string[] guids = AssetDatabase.FindAssets("t:RenderPipelineAsset");
            RenderPipelineAsset pipeline = null;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
                if (pipeline != null)
                    break;
            }

            if (pipeline == null)
            {
                Debug.LogError("FORTIDLE SETUP_NO_PIPELINE");
                return;
            }

            if (GraphicsSettings.defaultRenderPipeline == null)
                GraphicsSettings.defaultRenderPipeline = pipeline;

            int current = QualitySettings.GetQualityLevel();
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                if (QualitySettings.renderPipeline == null)
                    QualitySettings.renderPipeline = pipeline;
            }

            QualitySettings.SetQualityLevel(current, false);
        }

        static void ForceInputManager()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0)
                return;
            SerializedObject serialized = new SerializedObject(assets[0]);
            SerializedProperty property = serialized.FindProperty("activeInputHandler");
            if (property == null)
                return;
            property.intValue = 0;
            serialized.ApplyModifiedProperties();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent))
                return;
            parent = parent.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
