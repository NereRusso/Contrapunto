using UnityEngine;

public class CubePickup : MonoBehaviour
{
    [Header("Distancia máxima para recoger el cubo")]
    public float pickupRange = 3f;

    [Header("Texto de instrucción")]
    [Tooltip("Arrastrá acá el GameObject del Canvas con la palabra 'Click'")]
    public GameObject clickPrompt;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("No se encontró un objeto con Tag 'Player'");

        if (clickPrompt != null)
            clickPrompt.SetActive(false);
        else
            Debug.LogError("No asignaste clickPrompt en el Inspector");
    }

    void Update()
    {
        // Si ya se ocultó y te alejás, no pasa nada.
        if (player == null || clickPrompt == null) return;

        if (clickPrompt.activeSelf)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d > pickupRange)
                clickPrompt.SetActive(false);
        }
    }

    void OnMouseOver()
    {
        if (player == null || clickPrompt == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= pickupRange)
            clickPrompt.SetActive(true);
    }

    void OnMouseExit()
    {
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
    }

    void OnMouseDown()
    {
        if (player == null || clickPrompt == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= pickupRange)
        {
            // 1) Oculto el prompt inmediatamente
            clickPrompt.SetActive(false);

            // 2) Llamo al manager
            MotionManager.Instance.CollectCube();

            // 3) Destruyo este cubo
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // Por si el script/cubo se desactiva sin pasar por OnMouseDown
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
    }

    void OnDestroy()
    {
        // Refuerzo extra: si llega a destruirse el objeto
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
    }
}
