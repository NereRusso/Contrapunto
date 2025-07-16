using UnityEngine;

public class PromptManager : MonoBehaviour
{
    [Tooltip("Tu único Canvas World Space con el texto 'Click'")]
    public GameObject clickPrompt;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    Camera cam;
    Transform player;

    void Start()
    {
        cam = Camera.main;
        player = GameObject.FindWithTag("Player")?.transform;
        if (clickPrompt != null) clickPrompt.SetActive(false);
        else Debug.LogError("No asignaste clickPrompt en PromptManager");
    }

    void Update()
    {
        if (cam == null || player == null) return;

        // Ray desde el centro de pantalla
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit[] hits = Physics.RaycastAll(ray, pickupRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        CubePickup nearest = null;
        foreach (var h in hits)
        {
            nearest = h.collider.GetComponent<CubePickup>();
            if (nearest != null) break;
        }

        // Solo activar/desactivar el prompt; no tocamos su posición ni rotación
        if (nearest != null)
            clickPrompt.SetActive(true);
        else
            clickPrompt.SetActive(false);
    }
}
