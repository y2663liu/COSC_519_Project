using UnityEngine;
public class AutoDestroyOnStop : MonoBehaviour
{
    ParticleSystem ps;
    void Awake(){ ps = GetComponent<ParticleSystem>(); }
    void Update(){ if (ps && !ps.IsAlive(true)) Destroy(gameObject); }
}

