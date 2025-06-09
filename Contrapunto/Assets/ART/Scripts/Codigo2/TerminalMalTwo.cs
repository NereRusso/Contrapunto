using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;

public class TerminalSimple : MonoBehaviour
{
    [Header("Pantalla y Cámaras")]
    public GameObject screenCanvas;
    public Camera playerCamera;
    public Camera eventCamera;
    public Transform eventCameraTarget;

    [Header("Fade & Vídeo")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;

    [Header("Post–actividad")]
    public float postActivityDelay = 2f;
    public AudioSource postNarrationAudio;
    public VideoPlayer nextVideoPlayer;
    public string nextSceneName;

    [Header("Animaciones")]
    public float interactionDistance = 5f;
    public float screenOpenSpeed = 2f;
    public float screenCloseSpeed = 2f;
    public float cameraMoveSpeed = 2f;
    public float cameraReturnSpeed = 2f;

    // Internos
    private bool hasActivated = false;
    private bool screenOpening = false;
    private bool movingCamera = false;
    private Vector3 screenTargetScale;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private FirstPersonController playerController;

    void Start()
    {
        // Preparamos el screenCanvas
        screenTargetScale = screenCanvas.transform.localScale;
        screenCanvas.transform.localScale = Vector3.zero;
        screenCanvas.SetActive(false);

        // Preparamos el fadeCanvas
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;

        playerController = FindObjectOfType<FirstPersonController>();
        eventCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        // Detectar click en el cubo para activar
        if (!hasActivated && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.transform == transform
                && Vector3.Distance(Camera.main.transform.position, transform.position) <= interactionDistance)
            {
                ActivateScreen();
            }
        }

        // Animar apertura de canvas
        if (screenOpening)
        {
            screenCanvas.transform.localScale = Vector3.Lerp(
                screenCanvas.transform.localScale,
                screenTargetScale,
                Time.deltaTime * screenOpenSpeed
            );
            if (Vector3.Distance(screenCanvas.transform.localScale, screenTargetScale) < 0.01f)
            {
                screenCanvas.transform.localScale = screenTargetScale;
                screenOpening = false;
            }
        }

        // Animar movimiento de camera de evento
        if (movingCamera)
        {
            eventCamera.transform.position = Vector3.Lerp(
                eventCamera.transform.position,
                eventCameraTarget.position,
                Time.deltaTime * cameraMoveSpeed
            );
            eventCamera.transform.rotation = Quaternion.Lerp(
                eventCamera.transform.rotation,
                eventCameraTarget.rotation,
                Time.deltaTime * cameraMoveSpeed
            );
            if (Vector3.Distance(eventCamera.transform.position, eventCameraTarget.position) < 0.05f &&
                Quaternion.Angle(eventCamera.transform.rotation, eventCameraTarget.rotation) < 1f)
            {
                movingCamera = false;
            }
        }
    }

    void ActivateScreen()
    {
        hasActivated = true;
        screenOpening = true;
        screenCanvas.SetActive(true);

        // Desactivamos al jugador y guardamos cámara original
        originalCamPosition = playerCamera.transform.position;
        originalCamRotation = playerCamera.transform.rotation;
        playerController.enabled = false;
        playerCamera.gameObject.SetActive(false);

        // Activamos cámara de evento
        eventCamera.transform.position = originalCamPosition;
        eventCamera.transform.rotation = originalCamRotation;
        eventCamera.gameObject.SetActive(true);
        movingCamera = true;

        // Habilitamos cursor para UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Llamar desde PuzzleManager cuando el puzzle esté completo
    public void CloseScreen()
    {
        StartCoroutine(CloseScreenCoroutine());
    }

    IEnumerator CloseScreenCoroutine()
    {
        // 1) Cerrar el screenCanvas con animación de escala
        Vector3 startScale = screenCanvas.transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * screenCloseSpeed;
            screenCanvas.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        screenCanvas.SetActive(false);

        // 2) Animar retorno de cámara a la del jugador
        t = 0f;
        Vector3 camFromPos = eventCamera.transform.position;
        Quaternion camFromRot = eventCamera.transform.rotation;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraReturnSpeed;
            eventCamera.transform.position = Vector3.Lerp(camFromPos, originalCamPosition, t);
            eventCamera.transform.rotation = Quaternion.Slerp(camFromRot, originalCamRotation, t);
            yield return null;
        }
        eventCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        playerController.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(1f);
        // 3) Reproducir narración y esperar
        if (postNarrationAudio != null)
            postNarrationAudio.Play();
        yield return new WaitForSeconds(postActivityDelay);

        // 4) Fade a negro
        fadeCanvas.blocksRaycasts = true;
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t / fadeDuration;
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // 5) Reproducir video y al mismo tiempo desactivar el fade (para no ver el fondo)
        if (nextVideoPlayer != null)
        {
            nextVideoPlayer.Play();

            // ?? En el mismo frame que empieza el video, quitamos el fade
            fadeCanvas.gameObject.SetActive(false);

            nextVideoPlayer.loopPointReached += _ =>
            {
                SceneManager.LoadScene(nextSceneName);
            };
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }

    }
}
