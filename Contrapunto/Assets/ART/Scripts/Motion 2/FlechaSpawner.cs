using UnityEngine;

public class FlechaSpawner : MonoBehaviour
{
    [Header("Prefab de Flecha")]
    public GameObject flechaPrefab;

    [Header("Columnas donde aparecer�n")]
    public Transform[] columnas; // 0 = Izquierda, 1 = Abajo, 2 = Arriba, 3 = Derecha

    [Header("Configuraci�n")]
    public float intervaloSpawn = 1.2f; // Cada cu�nto tiempo aparece una flecha

    // Llamar a este m�todo para comenzar a generar flechas
    public void IniciarSpawner()
    {
        InvokeRepeating(nameof(GenerarFlecha), 0f, intervaloSpawn);
    }

    // Llamar a este m�todo para detener el spawner
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

        // Posici�n local al borde inferior:
        rtFlecha.anchoredPosition = new Vector2(0, -alturaColumna * 0.5f);

        // Para que quede abajo de todas las otras flechas en la jerarqu�a
        flecha.transform.SetAsFirstSibling();

        // Asignar direcci�n a la flecha (si us�s FlechaMovimiento)
        FlechaMovimiento flechaMov = flecha.GetComponent<FlechaMovimiento>();
        if (flechaMov != null)
            flechaMov.direccionActual = (FlechaMovimiento.Direccion)direccion;
    }


}
