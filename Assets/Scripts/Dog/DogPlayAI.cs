using UnityEngine;

public class DogPlayAI : MonoBehaviour
{
    
    [SerializeField] private Transform player;
    
    private float width = 10f;
    private float speed   = 6f;
    
    private float minDistance = 3f;           // keep >= this far
    private float stopBuffer  = 0.5f;          // little buffer to reduce jitter at the edge
    private float edgeMargin  = 0.25f;         // keep a small gap from edges

    float groundY; // y value for the dog
    float minX, maxX, minZ, maxZ; // boundary of the dog

    void Start()
    {
        groundY = transform.position.y;
        
        float half = Mathf.Max(0f, width * 0.5f);
        float cx = transform.position.x;
        float cz = transform.position.z;
        minX = cx - half;  maxX = cx + half;
        minZ = cz - half;  maxZ = cz + half;
    }

    void Update()
    {
        Vector2 dogLoc    = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerLoc = new Vector2(player.position.x,   player.position.z);
        float d = Vector2.Distance(dogLoc, playerLoc);

        if (d < minDistance)
        {
            // Direction away from player (any if overlapping)
            Vector2 dir = dogLoc - playerLoc;
            if (dir.sqrMagnitude < 1e-6f) dir = Random.insideUnitCircle;
            dir.Normalize();

            // Aim just outside the keep-out radius
            Vector2 desired = playerLoc + dir * (minDistance + stopBuffer);

            // Clamp to our square bounds
            desired = ClampToArea(desired);

            // If still too close (e.g., player near/inside the square), pick farthest corner
            if (Vector2.Distance(desired, playerLoc) < minDistance)
                desired = FarthestCornerFrom(playerLoc);

            // Move (XZ only), keep original Y
            Vector3 target = new Vector3(desired.x, groundY, desired.y);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    Vector2 ClampToArea(Vector2 p)
    {
        float x = Mathf.Clamp(p.x, minX + edgeMargin, maxX - edgeMargin);
        float z = Mathf.Clamp(p.y, minZ + edgeMargin, maxZ - edgeMargin);
        return new Vector2(x, z);
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
        return new[]
        {
            new Vector2(minX + edgeMargin, minZ + edgeMargin),
            new Vector2(minX + edgeMargin, maxZ - edgeMargin),
            new Vector2(maxX - edgeMargin, minZ + edgeMargin),
            new Vector2(maxX - edgeMargin, maxZ - edgeMargin)
        };
    }
}
