using UnityEngine;
using TMPro;
using System.Collections;

public class PalancaJackpot : MonoBehaviour
{
    [Header("Referencias a los TextMeshPro")]
    public TMP_Text letterText;
    public TMP_Text numberText;
    public TMP_Text symbolText;

    [Header("Objeto especial para activar cuando se complete todo")]
    public GameObject specialObject;

    private bool isRolling = false;
    private bool isJackpotCompleted = false;

    void OnMouseDown()
    {
        if (!isRolling && !isJackpotCompleted)
        {
            StartCoroutine(StartJackpot());
        }
    }

    IEnumerator StartJackpot()
    {
        isRolling = true;

        // Lanzar en paralelo todas
        Coroutine letterCoroutine = StartCoroutine(RollSingle(letterText, GetRandomLetter, GetFixedLetter, JackpotManager.Instance.forceLetterC, 0.2f));
        Coroutine numberCoroutine = StartCoroutine(RollSingle(numberText, GetRandomNumber, GetFixedNumber, JackpotManager.Instance.forceNumber12, 0.4f));
        Coroutine symbolCoroutine = StartCoroutine(RollSingle(symbolText, GetRandomSymbol, GetFixedSymbol, JackpotManager.Instance.forceSymbolStar, 0.6f));

        // Esperar que terminen todas
        yield return letterCoroutine;
        yield return numberCoroutine;
        yield return symbolCoroutine;

        // Chequear si es el resultado final (C, 12, ?)
        if (letterText.text == "C" && numberText.text == "12" && symbolText.text == "\u2726")
        {
            // Jackpot completado!
            isJackpotCompleted = true;

            if (specialObject != null)
                specialObject.SetActive(true);

            Debug.Log("Jackpot completo! Desactivando palanca.");
        }

        isRolling = false;
    }

    IEnumerator RollSingle(TMP_Text textElement, System.Func<string> randomGenerator, System.Func<string> fixedResultGetter, bool isFixed, float totalDuration)
    {
        float elapsed = 0f;

        // Fase de giro
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // Siempre random durante el giro
            textElement.text = randomGenerator();

            yield return new WaitForSeconds(0.05f);
        }

        // Resultado final
        if (isFixed)
        {
            textElement.text = fixedResultGetter();
        }
        else
        {
            textElement.text = randomGenerator();
        }
    }

    // -----------------------
    // Funciones Random
    // -----------------------

    string GetRandomLetter()
    {
        char letter;
        do
        {
            letter = (char)('A' + Random.Range(0, 26));
        } while (letter == 'C');
        return letter.ToString();
    }

    string GetRandomNumber()
    {
        int number;
        do
        {
            number = Random.Range(0, 100);
        } while (number == 12);
        return number.ToString();
    }

    string GetRandomSymbol()
    {
        char symbol;
        do
        {
            int block = Random.Range(0, 4);
            int code;
            switch (block)
            {
                case 0: code = Random.Range(33, 48); break;
                case 1: code = Random.Range(58, 65); break;
                case 2: code = Random.Range(91, 97); break;
                default: code = Random.Range(123, 127); break;
            }
            symbol = (char)code;
        }
        while (symbol == '\u2726' || char.IsLetterOrDigit(symbol));
        return symbol.ToString();
    }

    // -----------------------
    // Funciones Fijas
    // -----------------------

    string GetFixedLetter()
    {
        return "C";
    }

    string GetFixedNumber()
    {
        return "12";
    }

    string GetFixedSymbol()
    {
        return "\u2726"; // Unicode ?
    }
}
