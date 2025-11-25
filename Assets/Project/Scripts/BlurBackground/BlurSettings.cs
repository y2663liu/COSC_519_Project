using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Blue")]
public class BlurSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard deviation (spread) of the blue Grid size is approx. 3x largers.")]
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
