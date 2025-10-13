using UnityEngine;

public class DogAI : MonoBehaviour
{
    private enum AreaMode { AxisAligned, Oriented }
    
    [SerializeField] private Transform player;                  // XR Origin > Main Camera or your rig root
    
    private AreaMode areaMode = AreaMode.AxisAligned;
    
    private float minX = -5f, maxX = 5f, minZ = -5f, maxZ = 5f;
    
    private Transform areaTransform;           // e.g., an empty GameObject placed at area center
    private Vector2 areaSize = new Vector2(10f, 10f); // XZ size (meters) for Oriented mode

    [Header("Behavior")]
    private float minDistance = 5f;           // keep >= this far
    private float stopBuffer  = 0.5f;          // little buffer to reduce jitter at the edge
    private float fleeSpeed   = 6f;            // m/s
    private float edgeMargin  = 0.25f;         // keep a small gap from edges
    private bool  lockYToStart = true;         // keep cube on its initial Y

    float groundY;

    void Start()
    {
        if (!player)
        {
            Debug.LogError("Assign 'player' (e.g., XR Origin > Main Camera) on FleePlayerInArea.");
            enabled = false; return;
        }
        groundY = lockYToStart ? transform.position.y : transform.position.y;
    }

    void Update()
    {
        Vector2 cube2 = new Vector2(transform.position.x, transform.position.z);
        Vector2 plr2  = new Vector2(player.position.x,   player.position.z);
        float d = Vector2.Distance(cube2, plr2);

        if (d < minDistance)
        {
            // 1) pick a direction away from player
            Vector2 dir = cube2 - plr2;
            if (dir.sqrMagnitude < 1e-6f) dir = Random.insideUnitCircle;
            dir.Normalize();

            // 2) desired point just outside radius
            Vector2 desired = plr2 + dir * (minDistance + stopBuffer);

            // 3) clamp to area
            desired = ClampToArea(desired);

            // 4) if still too close (e.g., player near edge), go to farthest corner
            if (Vector2.Distance(desired, plr2) < minDistance)
                desired = FarthestCornerFrom(plr2);

            // 5) move
            Vector3 target = new Vector3(desired.x, lockYToStart ? groundY : transform.position.y, desired.y);
            transform.position = Vector3.MoveTowards(transform.position, target, fleeSpeed * Time.deltaTime);
        }
    }

    Vector2 ClampToArea(Vector2 p)
    {
        if (areaMode == AreaMode.AxisAligned)
        {
            float x = Mathf.Clamp(p.x, minX + edgeMargin, maxX - edgeMargin);
            float z = Mathf.Clamp(p.y, minZ + edgeMargin, maxZ - edgeMargin);
            return new Vector2(x, z);
        }
        else
        {
            if (!areaTransform) return p;
            float hx = Mathf.Max(0f, areaSize.x * 0.5f - edgeMargin);
            float hz = Mathf.Max(0f, areaSize.y * 0.5f - edgeMargin);

            // world -> local (XZ plane of areaTransform)
            Vector3 world = new Vector3(p.x, areaTransform.position.y, p.y);
            Vector3 local = areaTransform.InverseTransformPoint(world);

            local.x = Mathf.Clamp(local.x, -hx, hx);
            local.z = Mathf.Clamp(local.z, -hz, hz);

            // local -> world
            Vector3 clamped = areaTransform.TransformPoint(local);
            return new Vector2(clamped.x, clamped.z);
        }
    }

    Vector2 FarthestCornerFrom(Vector2 point)
    {
        Vector2[] corners = GetAreaCorners();
        float bestD = -1f; Vector2 best = corners[0];
        foreach (var c in corners)
        {
            float cd = Vector2.Distance(c, point);
            if (cd > bestD) { bestD = cd; best = c; }
        }
        return best;
    }

    Vector2[] GetAreaCorners()
    {
        if (areaMode == AreaMode.AxisAligned)
        {
            return new[]
            {
                new Vector2(minX + edgeMargin, minZ + edgeMargin),
                new Vector2(minX + edgeMargin, maxZ - edgeMargin),
                new Vector2(maxX - edgeMargin, minZ + edgeMargin),
                new Vector2(maxX - edgeMargin, maxZ - edgeMargin)
            };
        }
        else
        {
            float hx = Mathf.Max(0f, areaSize.x * 0.5f - edgeMargin);
            float hz = Mathf.Max(0f, areaSize.y * 0.5f - edgeMargin);
            Vector3[] local = new[]
            {
                new Vector3(-hx, 0f, -hz),
                new Vector3(-hx, 0f,  hz),
                new Vector3( hx, 0f, -hz),
                new Vector3( hx, 0f,  hz)
            };
            Vector2[] world2 = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 w = areaTransform ? areaTransform.TransformPoint(local[i]) : local[i];
                world2[i] = new Vector2(w.x, w.z);
            }
            return world2;
        }
    }
}
