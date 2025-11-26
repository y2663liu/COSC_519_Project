using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private readonly Material material;
    private BlurSettings settings;

    // Temporary RTHandle for blur buffer
    private RTHandle blurTexture;

    public BlurRenderPass(Material material)
    {
        this.material = material;

        // Run just before built-in post-processing
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        // We need access to the camera color buffer
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    /// <summary>
    /// Called from the Renderer Feature each frame.
    /// </summary>
    public void UpdateSettings(BlurSettings settings)
    {
        this.settings = settings;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Safety checks
        if (material == null || settings == null || !settings.IsActive())
            return;

        var cameraData = renderingData.cameraData;

        // Allocate / resize the temporary RT to match the camera target
        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(
            ref blurTexture,
            desc,
            FilterMode.Bilinear,
            TextureWrapMode.Clamp,
            name: "_BlurTex"
        );

        CommandBuffer cmd = CommandBufferPool.Get("Gaussian Blur Post Process");

        // Pass spread from volume to shader
        material.SetFloat("_Spread", settings.strength.value);

        // Camera color target (compatibility mode path)
        RTHandle cameraTargetHandle = cameraData.renderer.cameraColorTargetHandle;

        // Use classic CommandBuffer.Blit so the shader sees _MainTex
        RenderTargetIdentifier srcID = cameraTargetHandle.nameID;
        RenderTargetIdentifier tmpID = blurTexture.nameID;

        // Horizontal pass (material pass 0)
        cmd.Blit(srcID, tmpID, material, 0);
        // Vertical pass (material pass 1)
        cmd.Blit(tmpID, srcID, material, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        if (blurTexture != null)
        {
            blurTexture.Release();
            blurTexture = null;
        }
    }
}



