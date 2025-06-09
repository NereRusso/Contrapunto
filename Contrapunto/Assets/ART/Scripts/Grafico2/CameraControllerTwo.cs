using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CameraControllerTwo : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public Vector2 xLimits = new Vector2(-3f, 3f);
    public Vector2 yLimits = new Vector2(-2f, 2f);

    [Header("Rotación")]
    public Transform visorPivot;
    public float lookSpeed = 1f;
    public float minYaw = -30f, maxYaw = 30f;
    public float minPitch = -20f, maxPitch = 20f;

    [Header("Objetivo de enfoque")]
    public Transform focusPoint; // Nuevo ? Empty Object en el centro
    public float centeringThreshold = 0.05f;
    public Camera visorCamera;

    [Header("Objetos que se van a acomodar")]
    public List<Transform> targetObjects;

    [Header("Feedback visual y sonido")]
    public CanvasGroup bordeBlanco;
    public AudioSource sonidoAprobado;

    [Header("Otros")]
    public GameObject player;
    public GameObject objectToEnableOnExit;

    [Header("Snap manual")]
    public bool useManualSnap = true;
    public Vector3 manualSnapPosition = new Vector3(27f, 8f, 29f);
    public Vector3 manualSnapEulerRotation = Vector3.zero;

    // Internas
    private float rotationX = 0f, rotationY = 0f;
    private float currentXOffset = 0f, currentYOffset = 0f;
    private Vector3 initialPos;
    private Quaternion initialRot;
    private bool isCentering = false;

    void OnEnable()
    {
        if (useManualSnap)
        {
            transform.position = manualSnapPosition;
            transform.rotation = Quaternion.Euler(manualSnapEulerRotation);
        }

        initialPos = transform.position;
        initialRot = transform.rotation;
        currentXOffset = currentYOffset = 0f;
        rotationX = rotationY = 0f;
        isCentering = false;

        if (bordeBlanco != null)
        {
            bordeBlanco.gameObject.SetActive(true);
            bordeBlanco.alpha = 0f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isCentering) return;

        float inX = Input.GetAxis("Horizontal");
        float inY = Input.GetAxis("Vertical");

        if (inX != 0f || inY != 0f)
        {
            Vector3 yawForward = Vector3.ProjectOnPlane(visorPivot.forward, Vector3.up).normalized;
            Vector3 rightDir = Vector3.Cross(Vector3.up, yawForward).normalized;
            Vector3 upDir = Vector3.up;

            currentXOffset += inX * moveSpeed * Time.deltaTime;
            currentYOffset += inY * moveSpeed * Time.deltaTime;

            currentXOffset = Mathf.Clamp(currentXOffset, xLimits.x, xLimits.y);
            currentYOffset = Mathf.Clamp(currentYOffset, yLimits.x, yLimits.y);

            transform.position = initialPos + rightDir * currentXOffset + upDir * currentYOffset;
        }

        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, minYaw, maxYaw);
        rotationY = Mathf.Clamp(rotationY, minPitch, maxPitch);
        visorPivot.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Verificar si el Focus Point está centrado en la vista
        Vector3 vp = visorCamera.WorldToViewportPoint(focusPoint.position);
        if (vp.z > 0f &&
            vp.x > 0.5f - centeringThreshold && vp.x < 0.5f + centeringThreshold &&
            vp.y > 0.5f - centeringThreshold && vp.y < 0.5f + centeringThreshold)
        {
            isCentering = true;
            StartCoroutine(FreezeThenReturn());
        }
    }

    IEnumerator FreezeThenReturn()
    {
        // Fade in borde blanco
        if (bordeBlanco != null)
            yield return StartCoroutine(FadeCanvasGroup(bordeBlanco, 0f, 1f, 0.5f));

        if (sonidoAprobado != null)
            sonidoAprobado.Play();

        // Disparar animaciones de acomodo
        foreach (Transform obj in targetObjects)
        {
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Acomodar");
            }
        }

        yield return new WaitForSeconds(1.5f); // Esperar que termine la animación

        // Fade out borde blanco
        if (bordeBlanco != null)
            yield return StartCoroutine(FadeCanvasGroup(bordeBlanco, 1f, 0f, 0.5f));

        bordeBlanco.gameObject.SetActive(false);

        // Volver al jugador
        player.SetActive(true);
        visorCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);

        // Habilitar objeto extra
        if (objectToEnableOnExit != null)
            objectToEnableOnExit.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float timer = 0f;
        cg.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
