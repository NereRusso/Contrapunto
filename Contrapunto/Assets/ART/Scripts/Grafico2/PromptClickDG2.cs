using UnityEngine;

public class PromptClickDG2 : MonoBehaviour
{
    [Tooltip("Tu único Canvas World Space con el texto 'Click'")]
    public GameObject clickPrompt;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (clickPrompt != null) clickPrompt.SetActive(false);
        else Debug.LogError("No asignaste clickPrompt en PromptManager");
    }

    void Update()
    {
        if (cam == null) return;

        // Ray desde el centro de pantalla
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit[] hits = Physics.RaycastAll(ray, pickupRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        PovInteractionTwo nearest = null;
        foreach (var h in hits)
        {
            // solo nos quedamos con los PovInteraction que SIGAN habilitados
            if (h.collider.TryGetComponent<PovInteractionTwo>(out var pov) && pov.enabled)
            {
                nearest = pov;
                break;
            }
        }

        clickPrompt.SetActive(nearest != null);
    }
}
