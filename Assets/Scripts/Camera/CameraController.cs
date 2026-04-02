using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction zoomAction;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 25f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float zoomSmooth = 10f;
    [SerializeField] private float minZoom = 5f;

    [Header("Refs")]
    [SerializeField] private CinemachineCamera virtualCam;
    [SerializeField] private BoxCollider2D bounds;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private float targetZoom;
    private float maxZoom = 20f;

    private void OnEnable()
    {
        moveAction.Enable();
        zoomAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        zoomAction.Disable();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        targetPosition = virtualCam.transform.position;
        targetZoom = virtualCam.Lens.OrthographicSize;
    }

    private void Update()
    {
        HandlePan();
        HandleZoom();
        ApplyMovement();
    }

    // ================= PAN =================
    private void HandlePan()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 dir = (Vector3)input.normalized;
        targetPosition += panSpeed * Time.deltaTime * dir;

        targetPosition = ClampToBounds(targetPosition);
    }

    // ================= ZOOM =================
    private void HandleZoom()
    {
        float scroll = zoomAction.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f) return;

        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        targetPosition = ClampToBounds(virtualCam.transform.position);
    }

    // ================= APPLY =================
    private void ApplyMovement()
    {
        // Smooth position
        Vector3 pos = Vector3.SmoothDamp(
            virtualCam.transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        virtualCam.transform.position = pos;

        // Smooth zoom
        var lens = virtualCam.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetZoom,
            Time.deltaTime * zoomSmooth
        );
        virtualCam.Lens = lens;
    }

    // ================= CLAMP =================
    private Vector3 ClampToBounds(Vector3 pos)
    {
        float camHeight = targetZoom;
        float camWidth = camHeight * Camera.main.aspect;

        Bounds b = bounds.bounds;

        float mapWidth = 35 * 0.5f;
        float mapHeight = 35 * 0.5f;

        Vector3 center = b.center;

        // If zoom out to much → force back to center
        if (camWidth >= mapWidth)
            pos.x = Mathf.Lerp(pos.x, center.x, Time.deltaTime * 5f);
        else
        {
            float minX = b.min.x + camWidth;
            float maxX = b.max.x - camWidth;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }

        if (camHeight >= mapHeight)
            pos.y = Mathf.Lerp(pos.y, center.y, Time.deltaTime * 5f);
        else
        {
            float minY = b.min.y + camHeight;
            float maxY = b.max.y - camHeight;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        return pos;
    }

    public void Init()
    {
        if (virtualCam.TryGetComponent<CinemachineConfiner2D>(out var confiner))
            confiner.InvalidateBoundingShapeCache();
        maxZoom = CalculateMaxZoom();
    }

    private float CalculateMaxZoom()
    {
        Bounds b = bounds.bounds;

        float mapWidth = b.size.x;
        float mapHeight = b.size.y;

        float aspect = Camera.main.aspect;

        float zoomY = mapHeight / 2f;
        float zoomX = mapWidth / (2f * aspect);

        return Mathf.Max(zoomY, zoomX);
    }
}