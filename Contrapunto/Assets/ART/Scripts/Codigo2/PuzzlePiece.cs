using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    private int currentRotation = 0;
    public int correctRotation = 0;
    private RectTransform rectTransform;
    private PuzzleManager manager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        manager = FindObjectOfType<PuzzleManager>();

        int[] rotations = { 0, 90, 180, 270 };
        currentRotation = rotations[Random.Range(0, rotations.Length)];
        rectTransform.rotation = Quaternion.Euler(0, 0, currentRotation);

        GetComponent<Button>().onClick.AddListener(RotatePiece);
    }

    void RotatePiece()
    {
        Debug.Log($"Click en {gameObject.name}");
        currentRotation = (currentRotation + 90) % 360;
        rectTransform.rotation = Quaternion.Euler(0, 0, currentRotation);
        manager.CheckPuzzle();
    }


    public bool IsCorrect()
    {
        return currentRotation == correctRotation;
    }
}
