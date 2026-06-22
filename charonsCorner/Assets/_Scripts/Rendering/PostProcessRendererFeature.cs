using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PostProcessRendererFeature : ScriptableRendererFeature
{
    class PostProcessRenderPass : ScriptableRenderPass
    {
        private const string _passName = "PostProcessPass";
        private Material _blitMaterial;

        public void Setup(Material mat)
        {
            _blitMaterial = mat;
            requiresIntermediateTexture = true;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Return if effect is not active
            VolumeStack stack = VolumeManager.instance.stack;
            PostProcessVolumeComponent volumeComponent = stack.GetComponent<PostProcessVolumeComponent>();
            if (volumeComponent.IsActive() == false)
            {
                return;
            }
            
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("Skipping rendering pass. PostProcessRendererFeature requires an intermediate color texture.");
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;

            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"Color-{_passName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters parameters = new(source, destination,
                _blitMaterial, 0);
            renderGraph.AddBlitPass(parameters, passName: _passName);

            resourceData.cameraColor = destination;
        }
    }

    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    public Material material;

    PostProcessRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new PostProcessRenderPass();
        
        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogWarning("PostProcessRendererFeature material is null and will be skipped.");
        }

        m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
