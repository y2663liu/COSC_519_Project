using UnityEngine;

public class NPCPointAtTarget : MonoBehaviour
{
    [Header("Bones")]
    public Transform spine2;         // Armature/Hips/Spine/Spine1/Spine2
    public Transform neck;           // Armature/Hips/Spine/Spine1/Spine2/Neck
    public Transform head;           // Armature/Hips/Spine/Spine1/Spine2/Neck/Head
    public Transform rightShoulder;  // Armature/Hips/Spine/Spine1/Spine2/RightShoulder

    [Header("Point Target")]
    public Transform target;         // The fixed world position to point at
    public bool isActive = false;

    [Header("Rotation Speeds")]
    public float torsoSpeed = 2f;
    public float neckSpeed = 3f;
    public float headSpeed = 4f;

    [Header("Arm Pose")]
    public float armLift = 25f;      // degrees up
    public float armTwist = 30f;     // degrees outward

    private Quaternion initialShoulderLocalRot;

    void Start()
    {
        if (rightShoulder != null)
            initialShoulderLocalRot = rightShoulder.localRotation;
    }

    void LateUpdate()
    {
        if (!isActive || target == null || spine2 == null)
            return;

        // 1. Rotate torso toward target
        Vector3 torsoDir = (target.position - spine2.position).normalized;
        Quaternion torsoLook = Quaternion.LookRotation(torsoDir, Vector3.up);
        spine2.rotation = Quaternion.Slerp(
            spine2.rotation,
            torsoLook,
            Time.deltaTime * torsoSpeed
        );

        // 2. Rotate neck
        if (neck != null)
        {
            Vector3 neckDir = (target.position - neck.position).normalized;
            Quaternion neckLook = Quaternion.LookRotation(neckDir, Vector3.up);
            neck.rotation = Quaternion.Slerp(
                neck.rotation,
                neckLook,
                Time.deltaTime * neckSpeed
            );
        }

        // 3. Rotate head
        if (head != null)
        {
            Vector3 headDir = (target.position - head.position).normalized;
            Quaternion headLook = Quaternion.LookRotation(headDir, Vector3.up);
            head.rotation = Quaternion.Slerp(
                head.rotation,
                headLook,
                Time.deltaTime * headSpeed
            );
        }

        // 4. Raise and twist arm to simulate pointing
        if (rightShoulder != null)
        {
            Quaternion targetArmRot = initialShoulderLocalRot *
                                      Quaternion.Euler(-armLift, armTwist, 0f);

            rightShoulder.localRotation = Quaternion.Slerp(
                rightShoulder.localRotation,
                targetArmRot,
                Time.deltaTime * 2f
            );
        }
    }

    public void ResetArm()
    {
        if (rightShoulder == null) return;

        rightShoulder.localRotation = Quaternion.Slerp(
            rightShoulder.localRotation,
            initialShoulderLocalRot,
            Time.deltaTime * 3f
        );
    }
}
