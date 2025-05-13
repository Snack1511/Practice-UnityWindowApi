using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.X86.Avx;


class BlitPass : ScriptableRenderPass
{
    Material blitMaterial;

    public BlitPass(Material material, RenderPassEvent evt)
    {
        blitMaterial = material;
        renderPassEvent = evt;
    }

    class PassData
    {
        public TextureHandle src;
        //public TextureHandle tmp;
        public Material material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        using (var builder = renderGraph.AddUnsafePass<PassData>("BlitPass", out var passData)) 
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            passData.src = resourceData.activeColorTexture;
            passData.material = blitMaterial;

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.sRGB = false;
            desc.enableRandomWrite = false;
            desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;

            //passData.tmp = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TmpCopy", true);

            builder.UseTexture(passData.src, AccessFlags.ReadWrite);
            //builder.UseTexture(passData.tmp, AccessFlags.ReadWrite);

            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);

            builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
            {
                if (!data.src.IsValid())
                {
                    Debug.LogError("activeColorTexture is null!");
                    return;
                }

                //if (!data.tmp.IsValid())
                //{
                //    Debug.LogError("Temporary texture creation failed!");
                //    return;
                //}

                var CommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                if(CommandBuffer == null)
                {
                    Debug.LogError("Temporary texture creation failed!");
                    return;
                }

                //if (data.src.Equals(data.tmp))
                //{
                //    Debug.LogError("BlitPass: Source and temporary textures are the same! This may cause unexpected behavior.");
                //    return;
                //}
                //Blitter.BlitTexture(CommandBuffer, data.src, data.tmp, );
                Blitter.BlitTexture(CommandBuffer, data.src, data.src, data.material, 0);
            });
        }
    }
}