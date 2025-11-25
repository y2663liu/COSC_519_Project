using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class WaterTeleportFX : MonoBehaviour
{
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    [SerializeField] XROrigin xrOrigin;
    [SerializeField] GameObject splashPrefab;

    void Reset()
    {
        if (!teleportationProvider) teleportationProvider = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>();
    }

    // Hook this to Water_TeleportArea -> Interactable Events -> OnSelectEntered
    public void OnTeleportSelected(SelectEnterEventArgs _)
    {
        StartCoroutine(SpawnSplashAfterMove());
    }

    IEnumerator SpawnSplashAfterMove()
    {
        // Wait a frame so the TeleportationProvider finishes moving the rig
        yield return new WaitForEndOfFrame();

        if (!xrOrigin || !splashPrefab) yield break;

        // Spawn at player feet (XR Origin floor height)
        var pos = xrOrigin.Camera.transform.position;
        pos.y = xrOrigin.Origin.transform.position.y + 0.02f;

        Instantiate(splashPrefab, pos, Quaternion.identity);
    }
}
