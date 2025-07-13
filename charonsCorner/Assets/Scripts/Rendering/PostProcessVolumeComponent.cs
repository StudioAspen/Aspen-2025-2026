using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeRequiresRendererFeatures(typeof(PostProcessRendererFeature))]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[VolumeComponentMenu("CharonsCorner/PostProcess")]
public sealed class PostProcessVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public PostProcessVolumeComponent()
    {
        displayName = "PostProcess";
    }

    [Tooltip("Intensity")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 1f);

    public bool IsActive()
    {
        return intensity.GetValue<float>() > 0.0f;
    }
}
