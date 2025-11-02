using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoverClueBase : InteractableBase {
    [SerializeField] private string hintMessage = "";

    protected virtual string HintMessage => hintMessage;
    [SerializeField] XRBaseInteractable rayInteractable;

    protected override void Start() {
        base.Start();
        Debug.Log("PointableClueBase start");
        rayInteractable.hoverEntered.AddListener(OnHoverEntered);
        rayInteractable.hoverExited.AddListener(OnHoverExited);
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!IsEnabled)
        {
            Debug.Log("unable");
            return;
        }
        Debug.Log("able");
        HintPopup.Instance?.ShowHint(HintMessage, transform);
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        HintPopup.Instance?.HideHint(transform);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (HintPopup.Instance != null)
        {
            HintPopup.Instance.HideHint(transform);
        }
    }
}
