using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 12f;

    [Header("缩放")]
    public float zoomSpeed = 5f;
    public float minZoom = 6f;
    public float maxZoom = 18f;

    private Camera cam;
    private Vector3 lastMousePosition;

    void Awake()
    {
        cam = GetComponent<Camera>();

        // RTS 斜俯视角
        transform.rotation = Quaternion.Euler(25f, 0f, 0f);

        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
        }
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
        HandleKeyboardMove();
    }

    // 按住鼠标右键拖动场景
    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // 沿摄像机的水平地面方向移动
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 movement =
                -right * delta.x - forward * delta.y;

            transform.position += movement * 0.02f;

            lastMousePosition = Input.mousePosition;
        }
    }

    // 鼠标滚轮缩放
    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize =
                Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    // WASD 键盘移动
    void HandleKeyboardMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 direction = right * x + forward * z;

        transform.position +=
            direction.normalized * moveSpeed * Time.deltaTime;
    }
}
