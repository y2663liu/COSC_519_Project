using UnityEngine;

public class HintPopupClue : ProximityInteractableBase
{
    public string title;
    public string message;
    public string footer;
    

    protected override void Start() {
        base.Start();
    }

    protected override void OnPlayerEnteredRange()
    {
        HintPopup.Instance?.ShowHint(title, message, footer, transform);
    }

    protected override void OnPlayerExitedRange()
    {
        HintPopup.Instance?.HideHint(transform);
    }
}