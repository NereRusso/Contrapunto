using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMoManager : MonoBehaviour
{
    public static UIMoManager Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI countText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateCubesRemaining(int remaining)
    {
        if (remaining > 0)
            countText.text = $"Faltan {remaining} keyframes";
        else
            countText.text = "Click para sincronizar";
    }
}
