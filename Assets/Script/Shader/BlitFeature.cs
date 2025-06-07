using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlitSettings
    {
        public Material blitMaterial = null;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
    }


    public BlitSettings settings = new BlitSettings();
    BlitPass blitPass;

    public override void Create()
    {
        blitPass = new BlitPass(settings.blitMaterial, settings.passEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //blitPass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(blitPass);
    }
}