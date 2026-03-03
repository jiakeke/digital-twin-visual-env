using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;       // 城市中心
    public float distance = 5f;   // 固定距离
    public float rotationSpeed = 0.2f;
    public float minYAngle = 10f;  // 上下旋转限制
    public float maxYAngle = 80f;

    private float yaw = 0f;        // 水平角度
    private float pitch = 30f;     // 垂直角度
    private Vector3 lastMousePosition;
    private bool rightMouseDown = false;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController: target not assigned!");
            return;
        }
        UpdateCameraPosition();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
            rightMouseDown = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            rightMouseDown = false;
        }

        if (rightMouseDown)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw += delta.x * rotationSpeed;
            pitch -= delta.y * rotationSpeed;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

            UpdateCameraPosition();

            lastMousePosition = Input.mousePosition;
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}