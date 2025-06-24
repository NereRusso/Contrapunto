using UnityEngine;
using System.Collections;

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

        // ¿Se completa el jackpot?
        bool esJackpot = JackpotManager.Instance.forceLetterC &&
                         JackpotManager.Instance.forceSymbolStar &&
                         JackpotManager.Instance.forceNumber12;

        // ?? Elegir y reproducir sonido con leve delay
        if (audioSource)
        {
            AudioClip clipElegido = esJackpot ? sonidoCompletoWin : sonidoCompletoFail;
            if (clipElegido != null)
            {
                audioSource.clip = clipElegido;
                audioSource.PlayDelayed(0.05f); // suena un pelito antes del giro
            }
        }

        // Esperar menos de lo normal para que el giro parezca seguir al sonido
        yield return new WaitForSeconds(0.03f); // leve respiro

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

        // Activación final si ganó
        if (esJackpot)
        {
            isJackpotCompleted = true;

            if (audio3Marti != null)
                NarrationManager.Instance.PlayNarration(audio3Marti);

            if (specialObject != null)
                specialObject.SetActive(true);
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
}
