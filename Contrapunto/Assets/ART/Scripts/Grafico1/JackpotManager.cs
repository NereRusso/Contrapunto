using UnityEngine;

public class JackpotManager : MonoBehaviour
{
    public static JackpotManager Instance;

    // Estado de cada parte del jackpot
    public bool forceLetterC = false;
    public bool forceNumber12 = false;
    public bool forceSymbolStar = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
