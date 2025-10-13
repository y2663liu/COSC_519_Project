using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform cam;

    void LateUpdate()
    {
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam == null) return;

        // Face camera but keep upright
        Vector3 toCam = (cam.position - transform.position);
        toCam.y = 0f; // comment this line if you want full face, including tilt
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
    }
}
