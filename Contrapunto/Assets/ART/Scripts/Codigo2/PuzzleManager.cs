using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] pieces;

    public void CheckPuzzle()
    {
        foreach (var piece in pieces)
        {
            if (!piece.IsCorrect())
                return;
        }

        Debug.Log("¡Puzzle completo!");

        // Cierra el screenCanvas y gira la cámara de vuelta
        var terminal = FindObjectOfType<TerminalSimple>();
        if (terminal != null)
            terminal.CloseScreen();
    }
}
