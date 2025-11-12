using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayInteractionActivator : MonoBehaviour
{
    [SerializeField] XRRayInteractor interactionInteractor;
    [SerializeField] InputActionProperty interactionAction;
    void Start()
    {
        interactionInteractor.gameObject.SetActive(false);

        interactionAction.action.performed += Action_preformed;
    }

    private void Action_preformed(InputAction.CallbackContext context)
    {
        interactionInteractor.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (interactionAction.action.WasReleasedThisFrame())
        {
            interactionInteractor.gameObject.SetActive(false);
        }
    }
}
