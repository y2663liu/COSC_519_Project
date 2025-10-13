using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{
    [SerializeField] XRRayInteractor teleportationInteractor;
    [SerializeField] InputActionProperty teleportationAction;
    void Start()
    {
        teleportationInteractor.gameObject.SetActive(false);

        teleportationAction.action.performed += Action_preformed;
    }

    private void Action_preformed(InputAction.CallbackContext context)
    {
        teleportationInteractor.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (teleportationAction.action.WasReleasedThisFrame())
        {
            teleportationInteractor.gameObject.SetActive(false);
        }
    }
}
