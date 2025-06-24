using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;

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
    public CanvasGroup videoCanvasGroup;   // CanvasGroup que contiene el RawImage del video
    public VideoPlayer videoPlayer;
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

        if (videoCanvasGroup != null)
        {
            videoCanvasGroup.gameObject.SetActive(false);
            videoCanvasGroup.alpha = 0f;
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
        if (videoCanvasGroup != null)
        {
            videoCanvasGroup.gameObject.SetActive(true);
            videoCanvasGroup.alpha = 0f;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Play();

            // ?? Activar animaciones en objetos de la lista
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    Animator animator = obj.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Acomodar");
                    }
                }
            }
        }

        // Fade in al inicio (0.5s)
        if (videoCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(videoCanvasGroup, 0f, 1f, 0.5f));

        // Esperar a que la duración del video esté lista
        while (videoPlayer.frameCount <= 0)
            yield return null;

        double videoDuration = videoPlayer.length;
        float fadeOutStartTime = (float)videoDuration - 0.5f;

        // Esperar hasta que sea momento de hacer el fade out
        yield return new WaitForSeconds(fadeOutStartTime);

        // Iniciar fade out (0.5s)
        if (videoCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(videoCanvasGroup, 1f, 0f, 0.5f));

        // Esperar a que el video termine
        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        if (videoCanvasGroup != null)
            videoCanvasGroup.gameObject.SetActive(false);

        // Reset y salida
        player.SetActive(true);
        visorCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (objectToEnableOnExit != null)
            objectToEnableOnExit.SetActive(true);
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
