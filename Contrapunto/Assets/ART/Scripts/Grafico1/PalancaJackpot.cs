using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PalancaJackpotFisico : MonoBehaviour
{
    [Header("Ruedas físicas")]
    public Transform ruedaLetra;
    public Transform ruedaSimbolo;
    public Transform ruedaNumero;

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
    public GameObject povC;

    [Header("Sonidos combinados")]
    public AudioSource audioSource;
    public AudioClip sonidoCompletoWin;
    public AudioClip sonidoCompletoFail;

    [Header("Audio ambiente")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientClipPostJackpot;
    public float fadeDuration = 1.5f;

    [Header("Objetos al resolver la segunda rueda (Símbolo) Último")]
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

    private bool isRolling = false;
    private bool isJackpotCompleted = false;
    private bool primerFalloYaOcurrido = false;
    private bool letraNarracionReproducida = false;   // Flag para la narración de letra
    private bool numeroNarracionReproducida = false; // Flag para la narración de número

    private Coroutine spinLetra, spinSimbolo, spinNumero;
    private Camera mainCamera;

    private readonly float[] alternativas = new float[] { 78f, 145f, 222f, 292f };
    private const float resultadoCorrecto = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (!isRolling && !isJackpotCompleted &&
            mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (isRolling || isJackpotCompleted)
            return;

        animator.SetTrigger("PlayClick");
        StartCoroutine(StartJackpot());
    }

    IEnumerator StartJackpot()
    {
        isRolling = true;

        bool esJackpot = JackpotManager.Instance.forceLetterC &&
                         JackpotManager.Instance.forceSymbolStar &&
                         JackpotManager.Instance.forceNumber12;

        // Narración especial si es el primer fallo
        if (!esJackpot && !primerFalloYaOcurrido && audio23Marti != null)
        {
            NarrationManager.Instance.PlayNarration(audio23Marti);
            primerFalloYaOcurrido = true;

            if (povC != null)
                povC.SetActive(true);
        }

        if (audioSource)
        {
            AudioClip clipElegido = esJackpot ? sonidoCompletoWin : sonidoCompletoFail;
            if (clipElegido != null)
            {
                audioSource.clip = clipElegido;
                audioSource.PlayDelayed(0.05f);
            }
        }

        yield return new WaitForSeconds(0.03f);

        // Iniciar giros
        spinLetra = StartCoroutine(GirarLibre(ruedaLetra));
        spinSimbolo = StartCoroutine(GirarLibre(ruedaSimbolo));
        spinNumero = StartCoroutine(GirarLibre(ruedaNumero));

        yield return new WaitForSeconds(1.2f);
        StopCoroutine(spinLetra);
        yield return StartCoroutine(FrenarSoloDiff(ruedaLetra, JackpotManager.Instance.forceLetterC));

        // Activar/desactivar objetos al resolver la primera rueda
        if (JackpotManager.Instance.forceLetterC)
        {
            foreach (var obj in objetosADesactivarLetra)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in objetosAActivarLetra)
                if (obj != null) obj.SetActive(true);

            // Solo la primera vez que sale la letra correcta
            if (!letraNarracionReproducida && audio33Marti != null)
            {
                NarrationManager.Instance.PlayNarration(audio33Marti);
                letraNarracionReproducida = true;
            }
        }

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinSimbolo);
        yield return StartCoroutine(FrenarSoloDiff(ruedaSimbolo, JackpotManager.Instance.forceSymbolStar));

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinNumero);
        yield return StartCoroutine(FrenarSoloDiff(ruedaNumero, JackpotManager.Instance.forceNumber12));

        // Activar/desactivar objetos al resolver la tercera rueda (Número)
        if (JackpotManager.Instance.forceNumber12)
        {
            foreach (var obj in objetosADesactivarNumero)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in objetosAActivarNumero)
                if (obj != null) obj.SetActive(true);

            // Solo la primera vez que sale el número correcto
            if (!numeroNarracionReproducida && audio32Marti != null)
            {
                NarrationManager.Instance.PlayNarration(audio32Marti);
                numeroNarracionReproducida = true;
            }
        }

        if (esJackpot)
        {
            isJackpotCompleted = true;

            if (audio3Marti != null)
                NarrationManager.Instance.PlayNarration(audio3Marti);

            if (specialObject != null)
                specialObject.SetActive(true);

            if (ambientAudioSource != null && ambientClipPostJackpot != null)
                StartCoroutine(CrossfadeAmbientAudio(ambientAudioSource, ambientClipPostJackpot, fadeDuration));

            foreach (var obj in objetosADesactivar)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in objetosAActivar)
                if (obj != null) obj.SetActive(true);
        }

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
        float destinoZ = forceCorrect
            ? resultadoCorrecto
            : alternativas[Random.Range(0, alternativas.Length)];

        float zNorm = (rueda.localEulerAngles.z % 360f + 360f) % 360f;
        float diff = (zNorm - destinoZ + 360f) % 360f;

        float startZ = rueda.localEulerAngles.z;
        float endZ = startZ - diff;

        float spinSpeed = 720f;
        float baseDur = diff / spinSpeed;
        float duration = Mathf.Max(baseDur, 0.5f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - t) * (1f - t);
            float z = Mathf.Lerp(startZ, endZ, eased);
            rueda.localEulerAngles = new Vector3(0, 0, z);
            yield return null;
        }

        rueda.localEulerAngles = new Vector3(0, 0, endZ);
    }

    IEnumerator CrossfadeAmbientAudio(AudioSource source, AudioClip newClip, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.clip = newClip;
        source.Play();

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, startVolume, timer / duration);
            yield return null;
        }

        source.volume = startVolume;
    }
}
