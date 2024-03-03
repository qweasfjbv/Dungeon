using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float zoomSpeed = 5f; // 줌 속도
    private float minOrthoSize = 20f; // 최소 Orthographic Size
    private float maxOrthoSize = 100f; // 최대 Orthographic Size
    private float minPosX = -100f; // 카메라 x축 최소 이동 범위
    private float maxPosX = 100f; // 카메라 x축 최대 이동 범위
    private float minPosY = -100f; // 카메라 y축 최소 이동 범위
    private float maxPosY = 100f; // 카메라 y축 최대 이동 범위

    private Vector3 dragOrigin; // 드래그 시작점

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