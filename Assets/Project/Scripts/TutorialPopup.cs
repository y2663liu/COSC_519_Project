using TMPro;
using UnityEngine;

public class TutorialPopup : MonoBehaviour {
    private static TutorialPopup _instance;

    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text footerLabel;
    
    [SerializeField] private float distanceFromCamera = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0f, -1f, 0f);

    private Transform _currentSource;

    public static TutorialPopup Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialPopup>();
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

    public void ShowHint(string title, string body, string footer, Transform source) {
        _currentSource = source;

        if (titleLabel != null)
        {
            titleLabel.text = title;
        }

        if (bodyLabel != null)
        {
            bodyLabel.text = body;
        }

        if (footerLabel != null)
        {
            footerLabel.text = footer;
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
    
    public void HideAll()
    {
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
