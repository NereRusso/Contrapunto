using UnityEngine;

public class CubePickup : MonoBehaviour
{
    [Header("Distancia m�xima para recoger el cubo")]
    public float pickupRange = 3f;

    [Header("Texto de instrucci�n")]
    [Tooltip("Arrastr� ac� el GameObject del Canvas con la palabra 'Click'")]
    public GameObject clickPrompt;

    private Transform player;
    private Camera cam;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("No se encontr� un objeto con Tag 'Player'");

        cam = Camera.main;
        if (cam == null)
            Debug.LogError("No se encontr� la c�mara principal con tag MainCamera");

        if (clickPrompt != null)
            clickPrompt.SetActive(false);
        else
            Debug.LogError("No asignaste clickPrompt en el Inspector");
    }

    void Update()
    {
        if (player == null || clickPrompt == null || cam == null)
            return;

        float d = Vector3.Distance(transform.position, player.position);
        // Si est�s dentro de rango, lanz� un raycast desde el centro de la c�mara:
        if (d <= pickupRange)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange) && hit.transform == transform)
            {
                clickPrompt.SetActive(true);
            }
            else
            {
                clickPrompt.SetActive(false);
            }
        }
        else
        {
            clickPrompt.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (player == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= pickupRange)
        {
            clickPrompt.SetActive(false);
            MotionManager.Instance.CollectCube();
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
    }

    void OnDestroy()
    {
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
    }
}
