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

    [Header("Sonidos combinados")]
    public AudioSource audioSource;
    public AudioClip sonidoCompletoWin;   // incluye ruedas + éxito
    public AudioClip sonidoCompletoFail;  // incluye ruedas + falla

    [Header("Audio ambiente")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientClipPostJackpot;
    public float fadeDuration = 1.5f;

    [Header("Activar / Desactivar objetos al Jackpot")]
    public List<GameObject> objetosADesactivar;
    public List<GameObject> objetosAActivar;

    private bool isRolling = false;
    private bool isJackpotCompleted = false;

    private Coroutine spinLetra, spinSimbolo, spinNumero;

    private readonly float[] alternativas = new float[] { 78f, 145f, 222f, 292f };
    private const float resultadoCorrecto = 0f;

    void OnMouseDown()
    {
        if (!isRolling && !isJackpotCompleted)
        {
            animator.SetTrigger("PlayClick");
            StartCoroutine(StartJackpot());
        }
    }

    IEnumerator StartJackpot()
    {
        isRolling = true;

        bool esJackpot = JackpotManager.Instance.forceLetterC &&
                         JackpotManager.Instance.forceSymbolStar &&
                         JackpotManager.Instance.forceNumber12;

        // Reproducir sonido inicial
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

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinSimbolo);
        yield return StartCoroutine(FrenarSoloDiff(ruedaSimbolo, JackpotManager.Instance.forceSymbolStar));

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinNumero);
        yield return StartCoroutine(FrenarSoloDiff(ruedaNumero, JackpotManager.Instance.forceNumber12));

        // Si se completó el jackpot
        if (esJackpot)
        {
            isJackpotCompleted = true;

            if (audio3Marti != null)
                NarrationManager.Instance.PlayNarration(audio3Marti);

            if (specialObject != null)
                specialObject.SetActive(true);

            // Cambiar sonido ambiente con crossfade
            if (ambientAudioSource != null && ambientClipPostJackpot != null)
            {
                StartCoroutine(CrossfadeAmbientAudio(ambientAudioSource, ambientClipPostJackpot, fadeDuration));
            }

            // Activar / desactivar objetos
            foreach (GameObject obj in objetosADesactivar)
                if (obj != null) obj.SetActive(false);

            foreach (GameObject obj in objetosAActivar)
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

        // Fade out
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.clip = newClip;
        source.Play();

        // Fade in
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
