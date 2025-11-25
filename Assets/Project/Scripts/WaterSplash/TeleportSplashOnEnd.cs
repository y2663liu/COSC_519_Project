using UnityEngine;
using Unity.XR.CoreUtils;

using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Collections;

public class TeleportSplashOnEnd : MonoBehaviour
{
    [Header("Assign scene instances")]
    [SerializeField] TeleportationProvider teleportationProvider; // SAME provider your Teleport Ray uses
    [SerializeField] XROrigin xrOrigin;                           // Your XR Origin / VR rig

    [Header("FX")]
    [SerializeField] GameObject splashPrefab;                     // Your FX_WaterSplash prefab

    [Header("Spawn height")]
    [Tooltip("Set this to your water plane Y (e.g., 0.0).")]
    [SerializeField] float fixedWaterY = 0.0f;
    [SerializeField] float yOffset = 0.02f;

    [Header("Rotation")]
    [Tooltip("Extra rotation to apply on top of the prefab's rotation (e.g., 90,0,0).")]
    [SerializeField] Vector3 customEulerOffset = Vector3.zero;

    void OnEnable()
    {
        // Simple auto-find fallback (ok to show deprecation warnings in Console)
        if (!teleportationProvider) teleportationProvider = FindObjectOfType<TeleportationProvider>();
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>();

        if (teleportationProvider != null)
        {
            // Use non-obsolete XRI 3.x events
            teleportationProvider.locomotionStarted += OnLocomotionStarted;
            teleportationProvider.locomotionEnded   += OnLocomotionEnded;
        }
        else
        {
            Debug.LogWarning("[Splash] No TeleportationProvider assigned/found.");
        }
    }

    void OnDisable()
    {
        if (teleportationProvider != null)
        {
            teleportationProvider.locomotionStarted -= OnLocomotionStarted;
            teleportationProvider.locomotionEnded   -= OnLocomotionEnded;
        }
    }

    void OnLocomotionStarted(UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider provider) { /* optional hook */ }

    void OnLocomotionEnded(UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider provider)
    {
        // Wait one frame so the rig/camera fully settle after teleport
        StartCoroutine(SpawnNextFrame());
    }

    IEnumerator SpawnNextFrame()
    {
        yield return null;

        if (xrOrigin == null || splashPrefab == null)
            yield break;

        // Use camera XZ, clamp Y to water surface
        Transform camTransform = xrOrigin.Camera != null
            ? xrOrigin.Camera.transform
            : xrOrigin.Origin.transform;

        Vector3 pos = camTransform.position;
        pos.y = fixedWaterY + yOffset;

        // ✅ Use the prefab's rotation, plus an optional offset, so your 90° tilt is respected
        Quaternion rot = splashPrefab.transform.rotation * Quaternion.Euler(customEulerOffset);

        // Spawn
        GameObject go = Instantiate(splashPrefab, pos, rot);

        // Ensure particles play even if Play On Awake was off
        var psAll = go.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < psAll.Length; i++)
            psAll[i].Play(true);
    }
}
