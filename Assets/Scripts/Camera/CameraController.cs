using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction zoomAction;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 20f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 40f;

    [Header("Bounds")]
    private Vector2 xBounds;
    private Vector2 yBounds;

    private Camera cam;
    private Vector2 lastMousePos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        moveAction.Enable();
        zoomAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        moveAction.Disable();
        zoomAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (moveAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Move Camera Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }

        if (zoomAction.bindings.Count == 0)
        {
            Debug.LogError("The Zoom Camera Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }

        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardPan();
        HandleZoom();
        ClampPosition();
    }

    private void HandleKeyboardPan()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, input.y, 0);
        transform.Translate(move * panSpeed * Time.deltaTime, Space.World);
    }

    private void HandleZoom()
    {
        float scroll = zoomAction.ReadValue<float>();

        if (cam.orthographic)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
        else
        {
            transform.position += transform.forward * scroll * zoomSpeed;
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, xBounds.x, xBounds.y);
        pos.y = Mathf.Clamp(pos.y, yBounds.x, yBounds.y);
        transform.position = pos;
    }

    public void ResetCamera()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void SetBounds(int gridWidth, int gridHeight, float cellSize, Vector2 originalPos)
    {
        xBounds = new Vector2(originalPos.x - gridWidth * cellSize / 3f, originalPos.x + gridWidth * cellSize / 3f);
        yBounds = new Vector2(originalPos.y - gridHeight * cellSize / 2f, originalPos.y + gridHeight * cellSize / 2f);
    }
}
