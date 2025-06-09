using UnityEngine;
using UnityEngine.UI;

public class FlechaMovimiento : MonoBehaviour
{
    public float velocidad = 300f;

    public Image imagenFlecha;
    public Sprite spriteIzquierda, spriteAbajo, spriteArriba, spriteDerecha;

    public enum Direccion { Izquierda, Abajo, Arriba, Derecha }
    public Direccion direccionActual;

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        switch (direccionActual)
        {
            case Direccion.Izquierda:
                imagenFlecha.sprite = spriteIzquierda;
                break;
            case Direccion.Abajo:
                imagenFlecha.sprite = spriteAbajo;
                break;
            case Direccion.Arriba:
                imagenFlecha.sprite = spriteArriba;
                break;
            case Direccion.Derecha:
                imagenFlecha.sprite = spriteDerecha;
                break;
        }
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.up * velocidad * Time.deltaTime;

        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);

        if (screenPoint.y > Screen.height + 50f)
        {
            // Notificar que se falló esta flecha
            InputFlechas.instance.MostrarFallo(direccionActual);

            Destroy(gameObject);
        }
    }
}
