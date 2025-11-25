using UnityEngine;

public class NPCPointAtTarget : MonoBehaviour
{
    [Header("Bones")]
    public Transform spine2;         // Armature/Hips/Spine/Spine1/Spine2
    public Transform neck;           // Armature/Hips/Spine/Spine1/Spine2/Neck
    public Transform head;           // Armature/Hips/Spine/Spine1/Spine2/Neck/Head
    public Transform rightShoulder;  // Armature/Hips/Spine/Spine1/Spine2/RightShoulder

    [Header("Target")]
    public Transform target;
    public bool isActive = false;

    [Header("Rotation Speeds")]
    public float torsoSpeed = 2f;
    public float neckSpeed = 3f;
    public float headSpeed = 4f;
    
    [Header("Arm Point Settings")]
    public float armLift = 25f;      // how much the arm raises
    public float armTwist = 30f;     // twist outward for pointing

    private Quaternion initialShoulderLocalRot;

    void Start()
    {
        if (rightShoulder != null)
            initialShoulderLocalRot = rightShoulder.localRotation;
    }

    void LateUpdate()
    {
        if (!isActive || target == null) 
            return;

        // 1. Torso
        Vector3 torsoDir = target.position - spine2.position;
        Quaternion torsoLook = Quaternion.LookRotation(torsoDir);
        spine2.rotation = Quaternion.Slerp(spine2.rotation, torsoLook, Time.deltaTime * torsoSpeed);

        // 2. Neck
        Vector3 neckDir = target.position - neck.position;
        Quaternion neckLook = Quaternion.LookRotation(neckDir);
        neck.rotation = Quaternion.Slerp(neck.rotation, neckLook, Time.deltaTime * neckSpeed);

        // 3. Head
        Vector3 headDir = target.position - head.position;
        Quaternion headLook = Quaternion.LookRotation(headDir);
        head.rotation = Quaternion.Slerp(head.rotation, headLook, Time.deltaTime * headSpeed);

        // 4. Arm
        if (rightShoulder != null)
        {
            Quaternion targetArmRot = initialShoulderLocalRot * 
                                      Quaternion.Euler(-armLift, armTwist, 0);
            
            rightShoulder.localRotation = Quaternion.Slerp(
                rightShoulder.localRotation,
                targetArmRot,
                Time.deltaTime * 2f
            );
        }
    }

    public void ResetArm()
    {
        if (rightShoulder != null)
        {
            rightShoulder.localRotation = Quaternion.Slerp(
                rightShoulder.localRotation,
                initialShoulderLocalRot,
                Time.deltaTime * 3f
            );
        }
    }
}
