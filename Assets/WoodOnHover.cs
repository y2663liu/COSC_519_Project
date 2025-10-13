using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WoodOnHover : MonoBehaviour
{
    [SerializeField] Canvas woodCanvas;
    [SerializeField] XRBaseInteractable rayInteractable;
    void Start()
    {
        woodCanvas.enabled = false;
        
        rayInteractable.hoverEntered.AddListener(OnHoverEntered);
        rayInteractable.hoverExited.AddListener(OnHoverExited);
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        woodCanvas.enabled = true;
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        woodCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
