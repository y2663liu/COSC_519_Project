using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature]
public class BlurRendererFeature : ScriptableRendererFeature
{
    private Material blurMaterial;
    private BlurRenderPass blurPass;

    public override void Create()
    {
        // Find the blur shader
        Shader shader = Shader.Find("Custom/Blur");
        if (shader == null)
        {
            Debug.LogError("BlurRendererFeature: Could not find shader 'Custom/Blur'. " +
                           "Make sure your Blur.shader starts with: Shader \"Custom/Blur\" { ... }");
            return;
        }

        // Create the material once
        blurMaterial = CoreUtils.CreateEngineMaterial(shader);

        // Create the render pass, injecting before built-in post-processing
        blurPass = new BlurRenderPass(blurMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blurPass == null || blurMaterial == null)
            return;

        // Get BlurSettings from the active volume stack
        var stack    = VolumeManager.instance.stack;
        var settings = stack.GetComponent<BlurSettings>();

        Debug.Log($"[BlurFeature] settings found={settings != null}, active={settings?.IsActive()}, strength={settings?.strength.value}");

        // Only run when the effect is active
        if (settings == null || !settings.IsActive())
            return;

        blurPass.UpdateSettings(settings);
        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing);
            return;
        }

        if (blurPass != null)
        {
            blurPass.Dispose();
            blurPass = null;
        }

        if (blurMaterial != null)
        {
            if (Application.isPlaying)
                Object.Destroy(blurMaterial);
            else
                Object.DestroyImmediate(blurMaterial);

            blurMaterial = null;
        }

        base.Dispose(disposing);
    }
}
