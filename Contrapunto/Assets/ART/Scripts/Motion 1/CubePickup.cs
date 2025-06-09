using UnityEngine;

public class CubePickup : MonoBehaviour
{
    [Header("Distancia máxima para recoger el cubo")]
    public float pickupRange = 3f; // podés modificarlo desde el Inspector

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró un objeto con el tag 'Player'");
        }
    }

    void OnMouseDown()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRange)
        {
            MotionManager.Instance.CollectCube();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Estás demasiado lejos para agarrar este cubo.");
            Debug.Log("Distancia al cubo: " + distance);

        }
    }
}
