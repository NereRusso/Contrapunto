using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerGlitch : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI fullTextDisplay; // El texto completo con palabras glitch
    public TMP_InputField inputField;
    public Button enterButton;

    [Header("Glitch Words")]
    public string[] glitchWords;
    private int currentWordIndex = 0;

    [Header("Sounds")]
    public AudioSource errorSound;

    private void Start()
    {
        ShowCurrentGlitchWord();
        enterButton.onClick.AddListener(CheckWord);
    }

    void ShowCurrentGlitchWord()
    {
        string displayText = "";

        for (int i = 0; i < glitchWords.Length; i++)
        {
            if (i == currentWordIndex)
            {
                displayText += $"<color=#FF00FF><b>{glitchWords[i]}</b></color> "; // Efecto glitch (color magenta + bold)
            }
            else
            {
                displayText += $"{glitchWords[i]} ";
            }
        }

        fullTextDisplay.text = displayText;
    }

    void CheckWord()
    {
        string entered = inputField.text.Trim();

        if (entered.ToLower() == glitchWords[currentWordIndex].ToLower())
        {
            Debug.Log("Correcto!");
            currentWordIndex++;

            if (currentWordIndex >= glitchWords.Length)
            {
                Debug.Log("Terminaste todas las palabras!");
                // Aquí podrías hacer que termine el juego o pase algo
                return;
            }

            inputField.text = "";
            ShowCurrentGlitchWord();
        }
        else
        {
            Debug.Log("Incorrecto!");
            errorSound.Play();
            inputField.text = "";
            inputField.ActivateInputField(); // volver a enfocar
        }
    }
}
