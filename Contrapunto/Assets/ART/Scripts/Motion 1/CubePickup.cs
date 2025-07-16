using UnityEngine;

public class CubePickup : MonoBehaviour
{
    public float pickupRange = 3f;

    Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void OnMouseDown()
    {
        if (player == null) return;
        if (Vector3.Distance(transform.position, player.position) <= pickupRange)
        {
            MotionManager.Instance.CollectCube();
            Destroy(gameObject);
        }
    }
}
