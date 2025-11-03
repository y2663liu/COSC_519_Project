using TMPro;
using UnityEngine;

public class HintPopup : MonoBehaviour {
    private static HintPopup _instance;

    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private TMP_Text hintLabel;
    private float distanceFromCamera = 3f;
    private Vector3 offset = new Vector3(0f, -1f, 0f);

    private Transform _currentSource;

    public static HintPopup Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HintPopup>();
            }

            return _instance;
        }
    }

    private void Awake() {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        HideImmediate();
    }

    private void LateUpdate() {
        if (_currentSource == null)
        {
            return;
        }

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        var forward = mainCamera.transform.forward;
        var position = mainCamera.transform.position + forward * distanceFromCamera + offset;
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(forward, mainCamera.transform.up);
    }

    public void ShowHint(string message, Transform source) {
        _currentSource = source;
        if (hintLabel != null)
        {
            hintLabel.text = message;
        }

        if (popupCanvas != null)
        {
            popupCanvas.enabled = true;
        }
    }

    public void HideHint(Transform source) {
        if (_currentSource != null && source != null && source != _currentSource)
        {
            return;
        }

        HideImmediate();
    }

    private void HideImmediate() {
        _currentSource = null;
        if (popupCanvas != null)
        {
            popupCanvas.enabled = false;
        }
    }
}
