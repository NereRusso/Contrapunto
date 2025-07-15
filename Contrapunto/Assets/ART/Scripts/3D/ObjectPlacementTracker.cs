using UnityEngine;

public class ObjectPlacementTracker : MonoBehaviour
{
    public static ObjectPlacementTracker Instance;

    private int placedObjectsCount = 0;
    public int totalObjectsToPlace = 3;

    [Header("Objetos a habilitar al completar")]
    public GameObject[] objetosAHabilitar;
    public GameObject[] objetosADeshabilitar;

    [Header("Cambio de audio al completar")]
    public AudioSource targetAudioSource;
    public AudioClip finalAmbientClip;
    [Range(0f, 1f)] public float finalVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ObjectPlaced()
    {
        placedObjectsCount++;

        if (placedObjectsCount >= totalObjectsToPlace)
        {
            Debug.Log("¡Todos los objetos colocados!");

            foreach (GameObject obj in objetosAHabilitar)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
            
            foreach (GameObject obj in objetosADeshabilitar)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            // Cambio simple de audio
            if (targetAudioSource != null && finalAmbientClip != null)
            {
                targetAudioSource.Stop();
                targetAudioSource.clip = finalAmbientClip;
                targetAudioSource.volume = finalVolume;
                targetAudioSource.Play();
            }
        }
    }
}
