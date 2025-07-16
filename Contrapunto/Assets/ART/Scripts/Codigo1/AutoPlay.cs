using UnityEngine;

public class AutoPlayFinal : MonoBehaviour
{
    [Tooltip("Tu componente TerminalMal (con PlayVideoAndClose p�blico)")]
    public TerminalMal terminalMal;

    void Start()
    {
        if (terminalMal == null)
        {
            Debug.LogError("AutoPlayFinal: asignar TerminalMal en el inspector");
            return;
        }
        // Lanza inmediatamente la misma rutina de v�deo final
        terminalMal.PlayVideoAndClose();
    }
}
