using UnityEngine;
using System.Collections;

public class PalancaJackpotFisico : MonoBehaviour
{
    [Header("Ruedas físicas")]
    public Transform ruedaLetra;
    public Transform ruedaSimbolo;
    public Transform ruedaNumero;

    public Animator animator;   // referencia al Animator

    [Header("Activación final")]
    public GameObject specialObject;

    private bool isRolling = false;
    private bool isJackpotCompleted = false;

    private Coroutine spinLetra, spinSimbolo, spinNumero;

    private readonly float[] alternativas = new float[] { 78f, 145f, 222f, 292f };
    private const float resultadoCorrecto = 0f;

    void OnMouseDown()
    {
        if (!isRolling && !isJackpotCompleted)
        {
            animator.SetTrigger("PlayClick");     // dispara la animación
            StartCoroutine(StartJackpot());       // y luego sigue con las ruedas
        }
    }


    IEnumerator StartJackpot()
    {
        isRolling = true;

        // 1. Arrancan todas a full
        spinLetra = StartCoroutine(GirarLibre(ruedaLetra));
        spinSimbolo = StartCoroutine(GirarLibre(ruedaSimbolo));
        spinNumero = StartCoroutine(GirarLibre(ruedaNumero));

        // 2. Tras 1.2s frenamos una a una
        yield return new WaitForSeconds(1.2f);
        StopCoroutine(spinLetra);
        yield return StartCoroutine(FrenarSoloDiff(ruedaLetra, JackpotManager.Instance.forceLetterC));

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinSimbolo);
        yield return StartCoroutine(FrenarSoloDiff(ruedaSimbolo, JackpotManager.Instance.forceSymbolStar));

        yield return new WaitForSeconds(0.4f);
        StopCoroutine(spinNumero);
        yield return StartCoroutine(FrenarSoloDiff(ruedaNumero, JackpotManager.Instance.forceNumber12));

        // 3. Comprobamos jackpot
        if (JackpotManager.Instance.forceLetterC &&
            JackpotManager.Instance.forceSymbolStar &&
            JackpotManager.Instance.forceNumber12)
        {
            isJackpotCompleted = true;
            if (specialObject != null) specialObject.SetActive(true);
            Debug.Log("¡Jackpot completo!");
        }

        isRolling = false;
    }

    // Giro infinito a velocidad constante
    IEnumerator GirarLibre(Transform rueda)
    {
        const float spinSpeed = 720f;
        while (true)
        {
            rueda.localEulerAngles += new Vector3(0, 0, -spinSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // Frenado suave recorriendo solo la diff exacta
    IEnumerator FrenarSoloDiff(Transform rueda, bool forceCorrect)
    {
        // 1?? Elegir destino exacto
        float destinoZ = forceCorrect
            ? resultadoCorrecto
            : alternativas[Random.Range(0, alternativas.Length)];

        // 2?? Normalizar posición actual y calcular diff horario
        float zNorm = (rueda.localEulerAngles.z % 360f + 360f) % 360f;
        float diff = (zNorm - destinoZ + 360f) % 360f;

        // 3?? Definir ángulo de inicio y fin
        float startZ = rueda.localEulerAngles.z;
        float endZ = startZ - diff; // al restar diff, Unity lo muestra como destinoZ

        // 4?? Calcular duración (velocidad = 720°/s, pero mínimo 0.5s)
        float spinSpeed = 720f;
        float baseDur = diff / spinSpeed;
        float duration = Mathf.Max(baseDur, 0.5f);
        float elapsed = 0f;

        // 5?? Ease-out cuadrático sin snap final
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - t) * (1f - t);
            float z = Mathf.Lerp(startZ, endZ, eased);
            rueda.localEulerAngles = new Vector3(0, 0, z);
            yield return null;
        }

        // 6?? Ajuste final exacto para eliminar cualquier float error
        rueda.localEulerAngles = new Vector3(0, 0, endZ);
    }


}
