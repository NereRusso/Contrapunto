using UnityEngine;

public class FlechaSpawner : MonoBehaviour
{
    [Header("Prefab de Flecha")]
    public GameObject flechaPrefab;

    [Header("Columnas donde aparecerán")]
    public Transform[] columnas; // 0 = Izquierda, 1 = Abajo, 2 = Arriba, 3 = Derecha

    [Header("Configuración")]
    public float intervaloSpawn = 1.2f; // Cada cuánto tiempo aparece una flecha

    // Llamar a este método para comenzar a generar flechas
    public void IniciarSpawner()
    {
        InvokeRepeating(nameof(GenerarFlecha), 0f, intervaloSpawn);
    }

    // Llamar a este método para detener el spawner
    public void DetenerSpawner()
    {
        CancelInvoke(nameof(GenerarFlecha));
    }

    void GenerarFlecha()
    {
        if (flechaPrefab == null)
        {
            Debug.LogWarning("FlechaPrefab no asignado!");
            return;
        }

        int direccion = Random.Range(0, columnas.Length);
        Transform columna = columnas[direccion];

        // Instancia en modo UI (worldPositionStays = false)
        GameObject flecha = Instantiate(flechaPrefab, columna, false);

        // Forzar que empiece justo en el fondo de la columna
        RectTransform rtFlecha = flecha.GetComponent<RectTransform>();
        RectTransform rtColumna = columna.GetComponent<RectTransform>();
        float alturaColumna = rtColumna.rect.height;

        // Posición local al borde inferior:
        rtFlecha.anchoredPosition = new Vector2(0, -alturaColumna * 0.5f);

        // Para que quede abajo de todas las otras flechas en la jerarquía
        flecha.transform.SetAsFirstSibling();

        // Asignar dirección a la flecha (si usás FlechaMovimiento)
        FlechaMovimiento flechaMov = flecha.GetComponent<FlechaMovimiento>();
        if (flechaMov != null)
            flechaMov.direccionActual = (FlechaMovimiento.Direccion)direccion;
    }


}
