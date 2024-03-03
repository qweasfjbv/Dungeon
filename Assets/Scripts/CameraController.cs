using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float zoomSpeed = 5f; // �� �ӵ�
    private float minOrthoSize = 20f; // �ּ� Orthographic Size
    private float maxOrthoSize = 100f; // �ִ� Orthographic Size
    private float minPosX = -100f; // ī�޶� x�� �ּ� �̵� ����
    private float maxPosX = 100f; // ī�޶� x�� �ִ� �̵� ����
    private float minPosY = -100f; // ī�޶� y�� �ּ� �̵� ����
    private float maxPosY = 100f; // ī�޶� y�� �ִ� �̵� ����

    private Vector3 dragOrigin; // �巡�� ������

    void Update()
    {
        ZoomCamera();
        MoveCamera();
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minOrthoSize, maxOrthoSize);
    }

    void MoveCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += difference;
            ClampCameraPosition();
        }
    }

    void ClampCameraPosition()
    {
        Vector3 pos = Camera.main.transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosX, maxPosX);
        pos.y = Mathf.Clamp(pos.y, minPosY, maxPosY);
        Camera.main.transform.position = pos;
    }
}