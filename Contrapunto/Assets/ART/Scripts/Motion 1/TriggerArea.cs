using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    [Tooltip("El Canvas (World Space) que está en el cilindro")]
    public Canvas worldCanvas;

    void Start()
    {
        worldCanvas.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Actualizo el texto AND muestro el Canvas
            UIMoManager.Instance.UpdateCubesRemaining(MotionManager.Instance.GetRemainingCubes());
            worldCanvas.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }
}
