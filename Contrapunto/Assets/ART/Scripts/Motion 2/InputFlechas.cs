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
    public TextMeshProUGUI feedbackTexto;

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
                    MostrarFeedback("PERFECTO", puntosPorPerfecto);
                    Destroy(flecha.gameObject);
                    return;
                }
                else if (distancia <= rangoBueno)
                {
                    MostrarFeedback("BIEN", puntosPorBien);
                    Destroy(flecha.gameObject);
                    return;
                }
            }
        }

        // No hacemos nada si no hay flechas  No es fallo
    }


    public void MostrarFallo(FlechaMovimiento.Direccion direccion)
    {
        MostrarFeedback("FALLO", 0);
    }


    void MostrarFeedback(string texto, float puntos = 0)
    {
        feedbackTexto.text = texto;
        feedbackTexto.gameObject.SetActive(true);
        CancelInvoke(nameof(EsconderFeedback));
        Invoke(nameof(EsconderFeedback), 0.5f);

        // Sumar puntos si corresponde
        if (puntos > 0)
        {
            barraRendimiento.value += puntos;

            // Limitar que no se pase de 100 y detectar si se llenó
            if (barraRendimiento.value >= barraRendimiento.maxValue)
            {
                barraRendimiento.value = barraRendimiento.maxValue;

                //Llamar al final del juego
                FinalDance.instance.FinDelJuego();
            }
        }
    }



    void EsconderFeedback()
    {
        feedbackTexto.gameObject.SetActive(false);
    }
}
