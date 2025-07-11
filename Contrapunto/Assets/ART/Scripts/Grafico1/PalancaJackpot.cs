using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine.InputSystem;

public class PalancaJackpotFisico : MonoBehaviour
{
    [Header("Ruedas físicas")]
    public Transform ruedaLetra;
    public Transform ruedaSimbolo;
    public Transform ruedaNumero;

    [Header("Animador de palanca")]
    public Animator animator;

    [Header("Activación final")]
    public GameObject specialObject;

    [Header("Narración")]
    public AudioClip audio3Marti;
    [Tooltip("Narración especial solo para el primer intento fallido")]
    public AudioClip audio23Marti;
    [Tooltip("Narración que suena al resolver la primera rueda (Letra)")]
    public AudioClip audio33Marti;
    [Tooltip("Narración que suena al resolver la tercera rueda (Número)")]
    public AudioClip audio32Marti;

    [Header("Povs")]
    public GameObject povC;                    // <— aquí

    [Header("Cámaras")]
    public Camera playerCamera;
    public Camera jackpotCamera;

    [Header("Animación de cámara")]
    public Transform jackpotCamTarget;
    public float cameraMoveSpeed = 2f;
    public float cameraReturnSpeed = 2f;

    [Header("Sonidos combinados")]
    public AudioSource audioSource;
    public AudioClip sonidoCompletoWin;
    public AudioClip sonidoCompletoFail;

    [Header("Audio ambiente")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientClipPostJackpot;
    public float fadeDuration = 1.5f;

    [Header("Objetos al resolver (Símbolo) Último")]
    public List<GameObject> objetosADesactivar;
    public List<GameObject> objetosAActivar;

    [Header("Objetos al resolver primera rueda (Letra)")]
    public List<GameObject> objetosADesactivarLetra;
    public List<GameObject> objetosAActivarLetra;

    [Header("Objetos al resolver tercera rueda (Número)")]
    public List<GameObject> objetosADesactivarNumero;
    public List<GameObject> objetosAActivarNumero;

    [Header("Prompt de click")]
    public GameObject clickCanvas;
    public float pickupRange = 3f;

    // Internals
    private bool isRolling = false;
    private bool isJackpotCompleted = false;
    private bool primerFalloYaOcurrido = false;
    private bool letraNarracionReproducida = false;
    private bool numeroNarracionReproducida = false;

    private Coroutine spinLetra, spinSimbolo, spinNumero;
    private Camera mainCamera;

    // Para deshabilitar/mantener controles
    private FirstPersonController playerController;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    // Para animación de cámara
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private bool movingCamera = false;

    private readonly float[] alternativas = new float[] { 78f, 145f, 222f, 292f };
    private const float resultadoCorrecto = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        if (clickCanvas != null) clickCanvas.SetActive(false);
        if (jackpotCamera != null) jackpotCamera.gameObject.SetActive(false);

        playerController = FindObjectOfType<FirstPersonController>();
        inputScript = playerController ? playerController.GetComponent<StarterAssetsInputs>() : null;
        playerInput = playerController ? playerController.GetComponent<PlayerInput>() : null;
    }

    void Update()
    {
        if (movingCamera && jackpotCamera != null && jackpotCamTarget != null)
        {
            jackpotCamera.transform.position = Vector3.Lerp(
                jackpotCamera.transform.position,
                jackpotCamTarget.position,
                Time.deltaTime * cameraMoveSpeed
            );
            jackpotCamera.transform.rotation = Quaternion.Slerp(
                jackpotCamera.transform.rotation,
                jackpotCamTarget.rotation,
                Time.deltaTime * cameraMoveSpeed
            );
            if (Vector3.Distance(jackpotCamera.transform.position, jackpotCamTarget.position) < 0.05f &&
                Quaternion.Angle(jackpotCamera.transform.rotation, jackpotCamTarget.rotation) < 1f)
            {
                movingCamera = false;
            }
        }
    }

    private void OnMouseEnter()
    {
        if (!isRolling && !isJackpotCompleted &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
            clickCanvas?.SetActive(true);
    }

    private void OnMouseExit()
    {
        clickCanvas?.SetActive(false);
    }

    void OnMouseDown()
    {
        clickCanvas?.SetActive(false);
        if (isRolling || isJackpotCompleted) return;

        animator.SetTrigger("PlayClick");
        StartCoroutine(StartJackpot());
    }

    IEnumerator StartJackpot()
    {
        isRolling = true;

        // Desactivar controles
        if (playerController != null) playerController.enabled = false;
        if (inputScript != null) inputScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Configurar cámaras
        originalCamPos = playerCamera.transform.position;
        originalCamRot = playerCamera.transform.rotation;
        jackpotCamera.transform.position = originalCamPos;
        jackpotCamera.transform.rotation = originalCamRot;
        jackpotCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);

        // Animar a target
        movingCamera = true;
        yield return new WaitUntil(() => movingCamera == false);

        // Narración primer fallo + activar povC
        bool esJackpot = JackpotManager.Instance.forceLetterC &&
                         JackpotManager.Instance.forceSymbolStar &&
                         JackpotManager.Instance.forceNumber12;
        if (!esJackpot && !primerFalloYaOcurrido && audio23Marti != null)
        {
            NarrationManager.Instance.PlayNarration(audio23Marti);
            primerFalloYaOcurrido = true;
            if (povC != null)            // <— aquí
                povC.SetActive(true);
        }

        // Sonido compuesto
        if (audioSource)
        {
            var clip = esJackpot ? sonidoCompletoWin : sonidoCompletoFail;
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        yield return new WaitForSeconds(0.03f);

        // Iniciar giros
        spinLetra = StartCoroutine(GirarLibre(ruedaLetra));
        spinSimbolo = StartCoroutine(GirarLibre(ruedaSimbolo));
        spinNumero = StartCoroutine(GirarLibre(ruedaNumero));

        // Frenar letra
        yield return new WaitForSeconds(1.2f);
        StopCoroutine(spinLetra);
        yield return StartCoroutine(FrenarSoloDiff(ruedaLetra, JackpotManager.Instance.forceLetterC));
        if (JackpotManager.Instance.forceLetterC)
        {
            objetosADesactivarLetra.ForEach(o => o?.SetActive(false));
            objetosAActivarLetra.ForEach(o => o?.SetActive(true));
            if (!letraNarracionReproducida && audio33Marti != null)
            {
                NarrationManager.Instance.PlayNarration(audio33Marti);
                letraNarracionReproducida = true;
            }
        }

        // Frenar símbolo
        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinSimbolo);
        yield return StartCoroutine(FrenarSoloDiff(ruedaSimbolo, JackpotManager.Instance.forceSymbolStar));

        // Frenar número
        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinNumero);
        yield return StartCoroutine(FrenarSoloDiff(ruedaNumero, JackpotManager.Instance.forceNumber12));
        if (JackpotManager.Instance.forceNumber12)
        {
            objetosADesactivarNumero.ForEach(o => o?.SetActive(false));
            objetosAActivarNumero.ForEach(o => o?.SetActive(true));
            if (!numeroNarracionReproducida && audio32Marti != null)
            {
                NarrationManager.Instance.PlayNarration(audio32Marti);
                numeroNarracionReproducida = true;
            }
        }

        // Lógica jackpot completo
        if (esJackpot)
        {
            isJackpotCompleted = true;
            if (audio3Marti != null) NarrationManager.Instance.PlayNarration(audio3Marti);
            specialObject?.SetActive(true);
            if (ambientAudioSource != null && ambientClipPostJackpot != null)
                StartCoroutine(CrossfadeAmbientAudio(ambientAudioSource, ambientClipPostJackpot, fadeDuration));
            objetosADesactivar.ForEach(o => o?.SetActive(false));
            objetosAActivar.ForEach(o => o?.SetActive(true));
        }

        // Animar vuelta de cámara
        float t = 0f;
        Vector3 startPos = jackpotCamera.transform.position;
        Quaternion startRot = jackpotCamera.transform.rotation;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraReturnSpeed;
            jackpotCamera.transform.position = Vector3.Lerp(startPos, originalCamPos, t);
            jackpotCamera.transform.rotation = Quaternion.Slerp(startRot, originalCamRot, t);
            yield return null;
        }

        // Restaurar controles y cámara jugador
        jackpotCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        if (playerController != null) playerController.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;

        isRolling = false;
    }

    IEnumerator GirarLibre(Transform rueda)
    {
        const float spinSpeed = 720f;
        while (true)
        {
            rueda.localEulerAngles += new Vector3(0, 0, -spinSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator FrenarSoloDiff(Transform rueda, bool forceCorrect)
    {
        float destinoZ = forceCorrect ? resultadoCorrecto : alternativas[Random.Range(0, alternativas.Length)];
        float zNorm = (rueda.localEulerAngles.z % 360f + 360f) % 360f;
        float diff = (zNorm - destinoZ + 360f) % 360f;
        float startZ = rueda.localEulerAngles.z;
        float endZ = startZ - diff;
        float spinSpeed = 720f;
        float duration = Mathf.Max(diff / spinSpeed, 0.5f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tParam = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - tParam) * (1f - tParam);
            float z = Mathf.Lerp(startZ, endZ, eased);
            rueda.localEulerAngles = new Vector3(0, 0, z);
            yield return null;
        }

        rueda.localEulerAngles = new Vector3(0, 0, endZ);
    }

    IEnumerator CrossfadeAmbientAudio(AudioSource source, AudioClip newClip, float duration)
    {
        float startVol = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.clip = newClip;
        source.Play();

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, startVol, timer / duration);
            yield return null;
        }
        source.volume = startVol;
    }
}
