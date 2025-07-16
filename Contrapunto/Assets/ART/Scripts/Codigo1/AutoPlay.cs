using UnityEngine;

public class AutoPlayFinal : MonoBehaviour
{
    [Tooltip("Tu componente TerminalMal (con PlayVideoAndClose público)")]
    public TerminalMal terminalMal;

    void Start()
    {
        if (terminalMal == null)
        {
            Debug.LogError("AutoPlayFinal: asignar TerminalMal en el inspector");
            return;
        }
        // Lanza inmediatamente la misma rutina de vídeo final
        terminalMal.PlayVideoAndClose();
    }
}
