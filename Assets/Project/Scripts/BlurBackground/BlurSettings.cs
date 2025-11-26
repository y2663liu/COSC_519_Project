using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/Blur")]
public class BlurSettings : VolumeComponent, IPostProcessComponent
{
    // Controls blur strength (spread)
    [Tooltip("Standard deviation (spread) of the blur. Higher values = stronger blur.")]
    public ClampedFloatParameter strength = new ClampedFloatParameter(0.0f, 0.0f, 15.0f);

    public bool IsActive()
    {
        return (strength.value > 0.0f) && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}