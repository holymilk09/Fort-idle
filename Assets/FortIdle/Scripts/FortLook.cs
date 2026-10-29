using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FortIdle
{
    // One warm sun, a night grade, and soft shadows. FortIdleConfig is left as shipped.
    public static class FortLook
    {
        public const bool SoftShadows = true;
        const float ShadowDistance = 32f;

        static Volume volume;
        static ColorAdjustments grade;
        static WhiteBalance balance;

        public static void Boot(Camera camera, Light sun)
        {
            ApplyQuality();
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                data.renderShadows = SoftShadows;
                data.renderPostProcessing = true;
            }

            if (sun != null && SoftShadows)
                TuneSun(sun, 0f);

            UniversalRenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            bool mobile = FortWorld.Config != null && FortWorld.Config.quality != null && FortWorld.Config.quality.mobile;
            if (pipeline != null && !mobile)
                pipeline.shadowDistance = ShadowDistance;
            if (mobile)
                QualitySettings.lodBias = 0.3f;

            EnsureVolume();
            Grade(0f);
        }

        public static void ApplyQuality()
        {
            if (FortWorld.Config == null || FortWorld.Config.quality == null || !FortWorld.Config.quality.mobile)
                return;
            UniversalRenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
                return;
            FortIdleConfig.QualityTuning quality = FortWorld.Config.quality;
            pipeline.supportsHDR = quality.hdr;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.renderScale = quality.renderScale;
            pipeline.msaaSampleCount = quality.msaa < 1 ? 1 : quality.msaa;
            int cascades = quality.shadowCascades < 1 ? 1 : quality.shadowCascades;
            if (cascades > 4)
                cascades = 4;
            pipeline.shadowCascadeCount = cascades;
            pipeline.shadowDistance = quality.shadowDistance;
        }

        public static void TuneSun(Light sun, float nightness)
        {
            if (sun == null || !SoftShadows)
                return;
            sun.shadows = LightShadows.Soft;
            // Night ambient is dim. A full shadow at that hour turns the ground black, so the contact shadow eases off.
            sun.shadowStrength = Mathf.Lerp(0.5f, 0.16f, Mathf.Clamp01(nightness));
        }

        public static void Grade(float nightness)
        {
            EnsureVolume();
            if (grade == null || balance == null)
                return;
            float night = Mathf.Clamp01(nightness);
            grade.postExposure.Override(Mathf.Lerp(0.04f, 0.85f, night));
            grade.contrast.Override(Mathf.Lerp(4f, 8f, night));
            grade.saturation.Override(Mathf.Lerp(4f, -10f, night));
            grade.colorFilter.Override(Color.Lerp(new Color(1f, 0.98f, 0.94f, 1f), new Color(0.84f, 0.9f, 1f, 1f), night));
            balance.temperature.Override(Mathf.Lerp(8f, -22f, night));
        }

        static void EnsureVolume()
        {
            if (volume != null)
                return;
            GameObject host = new GameObject("LookGrade");
            volume = host.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            grade = profile.Add<ColorAdjustments>(true);
            balance = profile.Add<WhiteBalance>(true);
            volume.profile = profile;
        }
    }
}
