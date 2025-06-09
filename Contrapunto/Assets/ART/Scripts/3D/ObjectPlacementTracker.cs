using UnityEngine;
using UnityEngine.Video;

public class ObjectPlacementTracker : MonoBehaviour
{
    public static ObjectPlacementTracker Instance;

    private int placedObjectsCount = 0;
    public int totalObjectsToPlace = 3;

    [Header("Prefab del objeto final")]
    public GameObject finalObjectPrefab;
    public float distanceBehindPlayer = 2f;
    public float delayBeforeFinalAppearance = 0f;

    [Header("Referencias para el prefab")]
    public GameObject playerReference;     // NestedParent_Unpack
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;

    private Transform playerTransform;
    private Camera mainCam;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Encontrar el jugador por tag
        GameObject playerRoot = GameObject.FindGameObjectWithTag("Player");
        if (playerRoot != null)
        {
            playerTransform = playerRoot.transform;
            mainCam = playerRoot.GetComponentInChildren<Camera>();
        }
        else
        {
            Debug.LogWarning("No se encontró el jugador con tag 'Player'");
        }
    }

    public void ObjectPlaced()
    {
        placedObjectsCount++;

        if (placedObjectsCount >= totalObjectsToPlace)
        {
            Debug.Log("¡Todos los objetos colocados!");
            SoundManager.Instance.PlayFinalNarration();

            if (finalObjectPrefab != null && playerTransform != null)
            {
                if (delayBeforeFinalAppearance > 0f)
                {
                    Invoke(nameof(SpawnFinalObjectBehindPlayer), delayBeforeFinalAppearance);
                }
                else
                {
                    SpawnFinalObjectBehindPlayer();
                }
            }
        }
    }

    private void SpawnFinalObjectBehindPlayer()
    {
        // Encontrar PlayerCapsule para posicionar correctamente
        Transform playerCapsule = playerReference.transform.Find("PlayerCapsule");
        if (playerCapsule == null)
        {
            Debug.LogError("No se encontró 'PlayerCapsule' dentro del playerReference.");
            return;
        }

        // Calcular posición detrás del jugador
        Vector3 spawnPosition = playerCapsule.position - playerCapsule.forward * distanceBehindPlayer;
        spawnPosition.y = playerCapsule.position.y + 0.5f; // ajustar altura si necesario

        GameObject finalObj = Instantiate(finalObjectPrefab, spawnPosition, Quaternion.identity);

        // Asignar referencias al script CambioScene
        CambioScene cambioScript = finalObj.GetComponent<CambioScene>();
        if (cambioScript != null)
        {
            cambioScript.player = playerReference;
            cambioScript.sonidoAmbiente = sonidoAmbiente;
            cambioScript.logoAmbiente = logoAmbiente;

            if (cambioScript.videoPlayer != null)
            {
                if (mainCam == null)
                    mainCam = Camera.main;

                if (mainCam != null)
                {
                    cambioScript.videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                    cambioScript.videoPlayer.targetCamera = mainCam;
                    Debug.Log("Asignada MainCamera al VideoPlayer correctamente");
                }
                else
                {
                    Debug.LogWarning("No se encontró MainCamera para el VideoPlayer");
                }
            }
        }

        // Asignar referencia al LogoAmbiente si existe
        LogoAmbiente logoAmbienteScript = finalObj.GetComponentInChildren<LogoAmbiente>();
        if (logoAmbienteScript != null)
        {
            logoAmbienteScript.playerTransform = playerCapsule;
        }
    }
}
