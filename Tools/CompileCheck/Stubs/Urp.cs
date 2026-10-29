// Compile-only stand-ins for com.unity.render-pipelines.universal and the SRP Volume API.
// Add members here when new code needs them, matching the real URP 17 signatures.
namespace UnityEngine.Rendering
{
    public class VolumeParameter<T>
    {
        public T value { get; set; }
        public bool overrideState { get; set; }
        public void Override(T x) { }
    }
    public class FloatParameter : VolumeParameter<float> { }
    public class ClampedFloatParameter : FloatParameter { }
    public class MinFloatParameter : FloatParameter { }
    public class ColorParameter : VolumeParameter<Color> { }
    public class BoolParameter : VolumeParameter<bool> { }
    public abstract class VolumeComponent : ScriptableObject
    {
        public bool active { get; set; }
    }
    public class VolumeProfile : ScriptableObject
    {
        public T Add<T>(bool overrides = false) where T : VolumeComponent { return null; }
        public bool TryGet<T>(out T component) where T : VolumeComponent { component = null; return false; }
    }
    public class Volume : MonoBehaviour
    {
        public bool isGlobal { get; set; }
        public float priority { get; set; }
        public float weight { get; set; }
        public VolumeProfile profile { get; set; }
        public VolumeProfile sharedProfile { get; set; }
    }
}

namespace UnityEngine.Rendering.Universal
{
    public sealed class ColorAdjustments : VolumeComponent
    {
        public FloatParameter postExposure = new FloatParameter();
        public ClampedFloatParameter contrast = new ClampedFloatParameter();
        public ColorParameter colorFilter = new ColorParameter();
        public ClampedFloatParameter hueShift = new ClampedFloatParameter();
        public ClampedFloatParameter saturation = new ClampedFloatParameter();
    }
    public sealed class WhiteBalance : VolumeComponent
    {
        public ClampedFloatParameter temperature = new ClampedFloatParameter();
        public ClampedFloatParameter tint = new ClampedFloatParameter();
    }
    public sealed class Bloom : VolumeComponent
    {
        public MinFloatParameter threshold = new MinFloatParameter();
        public MinFloatParameter intensity = new MinFloatParameter();
    }
    public sealed class Vignette : VolumeComponent
    {
        public ColorParameter color = new ColorParameter();
        public ClampedFloatParameter intensity = new ClampedFloatParameter();
    }
    public class UniversalAdditionalCameraData : MonoBehaviour
    {
        public bool renderShadows { get; set; }
        public bool renderPostProcessing { get; set; }
        public AntialiasingMode antialiasing { get; set; }
    }
    public enum AntialiasingMode { None, FastApproximateAntialiasing, SubpixelMorphologicalAntiAliasing, TemporalAntiAliasing }
    public static class CameraExtensions
    {
        public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this Camera camera) { return null; }
    }
    public class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        public bool supportsHDR { get; set; }
        public bool supportsCameraDepthTexture { get; set; }
        public bool supportsCameraOpaqueTexture { get; set; }
        public float renderScale { get; set; }
        public int msaaSampleCount { get; set; }
        public int shadowCascadeCount { get; set; }
        public float shadowDistance { get; set; }
        public override RenderPipeline CreatePipeline() { return null; }
    }
}
