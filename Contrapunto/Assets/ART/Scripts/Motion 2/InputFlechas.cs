using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFlechas : MonoBehaviour
{
    public static InputFlechas instance;

    public KeyCode teclaIzquierda = KeyCode.LeftArrow;
    public KeyCode teclaAbajo = KeyCode.DownArrow;
    public KeyCode teclaArriba = KeyCode.UpArrow;
    public KeyCode teclaDerecha = KeyCode.RightArrow;

    public ZonaHit[] zonasDeGolpe;

    [Header("Configuración")]
    public float rangoPerfecto = 5f;
    public float rangoBueno = 30f;

    [Header("Efectos visuales")]
    public Image feedbackImage;
    public Sprite spritePerfecto;
    public Sprite spriteBien;
    public Sprite spriteFallo;

    [Header("Rendimiento")]
    public Slider barraRendimiento;
    public float puntosPorPerfecto = 5f;
    public float puntosPorBien = 2f;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaIzquierda))
            VerificarFlechas(FlechaMovimiento.Direccion.Izquierda);

        if (Input.GetKeyDown(teclaAbajo))
            VerificarFlechas(FlechaMovimiento.Direccion.Abajo);

        if (Input.GetKeyDown(teclaArriba))
            VerificarFlechas(FlechaMovimiento.Direccion.Arriba);

        if (Input.GetKeyDown(teclaDerecha))
            VerificarFlechas(FlechaMovimiento.Direccion.Derecha);
    }

    void VerificarFlechas(FlechaMovimiento.Direccion direccion)
    {
        ZonaHit zona = null;

        foreach (var z in zonasDeGolpe)
        {
            if (z.direccionZona == direccion)
            {
                zona = z;
                break;
            }
        }

        if (zona == null) return;

        Vector3 zonaPos = zona.GetComponent<RectTransform>().position;

        FlechaMovimiento[] flechas = FindObjectsOfType<FlechaMovimiento>();
        foreach (var flecha in flechas)
        {
            if (flecha.direccionActual == direccion)
            {
                float distancia = Vector3.Distance(flecha.GetComponent<RectTransform>().position, zonaPos);

                if (distancia <= rangoPerfecto)
                {
                    MostrarFeedback(spritePerfecto, puntosPorPerfecto);
                    ManagerOpOne.Instance.ReproducirSonidoAcierto();
                    Destroy(flecha.gameObject);
                    return;
                }
                else if (distancia <= rangoBueno)
                {
                    MostrarFeedback(spriteBien, puntosPorBien);
                    ManagerOpOne.Instance.ReproducirSonidoAcierto();
                    Destroy(flecha.gameObject);
                    return;
                }
            }
        }

        MostrarFallo(direccion);
    }

    public void MostrarFallo(FlechaMovimiento.Direccion direccion)
    {
        MostrarFeedback(spriteFallo, 0);
        ManagerOpOne.Instance.ReproducirSonidoFallo();
    }

    void MostrarFeedback(Sprite sprite, float puntos = 0)
    {
        feedbackImage.sprite = sprite;

        // Definí la altura deseada
        float alturaFija = 50f;

        // Calculá la proporción del sprite (ancho dividido por alto)
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;

        // Calculá el ancho proporcional
        float anchoProporcional = alturaFija * aspectRatio;

        // Aplicalo al RectTransform del Image
        feedbackImage.rectTransform.sizeDelta = new Vector2(anchoProporcional, alturaFija);

        feedbackImage.gameObject.SetActive(true);
        CancelInvoke(nameof(EsconderFeedback));
        Invoke(nameof(EsconderFeedback), 0.5f);

        if (puntos > 0)
        {
            barraRendimiento.value += puntos;

            if (barraRendimiento.value >= barraRendimiento.maxValue)
            {
                barraRendimiento.value = barraRendimiento.maxValue;
                FinalDance.instance.FinDelJuego();
            }
        }
    }


    void EsconderFeedback()
    {
        feedbackImage.gameObject.SetActive(false);
    }
}
