using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private List<RectTransform> uiPanels;

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
        GameState gameState = MatchManager.Instance.GetGameState();
        if (gameState != GameState.Playing && gameState != GameState.Paused)
            return;

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
        if (Utility.IsPointerOverPanel(uiPanels)) return;

        float scroll = zoomAction.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f) return;

        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        targetPosition = ClampToBounds(virtualCam.transform.position);
    }

    // ================= APPLY =================
    private void ApplyMovement()
    {
        // Zoom
        var lens = virtualCam.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetZoom,
            Time.deltaTime * zoomSmooth);
        virtualCam.Lens = lens;

        targetPosition = ClampToBounds(targetPosition);

        // Move
        virtualCam.transform.position = Vector3.SmoothDamp(
            virtualCam.transform.position,
            targetPosition,
            ref velocity,
            smoothTime);
    }

    // ================= CLAMP =================
    private Vector3 ClampToBounds(Vector3 pos)
    {
        Bounds b = bounds.bounds;

        float camHeight = virtualCam.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        if (camWidth * 2f >= b.size.x)
        {
            pos.x = b.center.x;
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x,
                b.min.x + camWidth,
                b.max.x - camWidth);
        }

        if (camHeight * 2f >= b.size.y)
        {
            pos.y = b.center.y;
        }
        else
        {
            pos.y = Mathf.Clamp(
                pos.y,
                b.min.y + camHeight,
                b.max.y - camHeight);
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
        float zoomX = mapWidth / (1.8f * aspect);

        return Mathf.Max(zoomY, zoomX);
    }

    public void FocusCell(int x, int y, Grid<GridCell> grid)
    {
        Vector3 cellWorldPos = Utility.GridToWorldPosition(x, y, grid.GetWidth(), grid.GetHeight(), grid.GetCellSize(), grid.GetOriginPosition());

        targetPosition = new Vector3(cellWorldPos.x, cellWorldPos.y, virtualCam.transform.position.z);
        //targetPosition = ClampToBounds(pos);
        //ApplyMovement();
    }
}