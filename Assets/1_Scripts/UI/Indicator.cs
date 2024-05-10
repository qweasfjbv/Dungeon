using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Indicator : MonoBehaviour
{
    public Transform target; // 적의 위치를 가리킬 대상
    public RectTransform indicator; // UI 화살표
    private Camera mainCamera;


    private void OnEnable()
    {
        mainCamera = Camera.main; // 메인 카메라 할당
        indicator.gameObject.SetActive(false); // 초기에는 화살표를 비활성화
    }

    public void SetIndicator(Transform target, RectTransform indi)
    {
        this.target = target;
        indicator = indi;
    }

    void Update()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.position);
        bool isOffScreen = screenPoint.x <= 0 || screenPoint.x >= 1 || screenPoint.y <= 0 || screenPoint.y >= 1;

        if (isOffScreen)
        {
            indicator.gameObject.SetActive(true); // 화살표 활성화
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position);
            Vector3 fromCenter = screenPosition - new Vector3(Screen.width / 2, Screen.height / 2, 0);
            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg - 90; // -90을 추가하여 화살표가 위를 향하도록 조정
            indicator.localEulerAngles = new Vector3(0, 0, angle);

            fromCenter.Normalize();

            
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);

            if (Mathf.Abs(cos) < Settings.MAXCOS)
            {
                fromCenter.x = Mathf.Sign(fromCenter.x) * Screen.width / 2;
                fromCenter.y = Mathf.Sign(fromCenter.y) * Screen.height / 2 * Mathf.Abs(cos) /Settings.MAXCOS;

                fromCenter *= 0.95f;
            }
            else
            {

                fromCenter.x = Mathf.Sign(fromCenter.x) * Screen.width / 2 * Mathf.Abs(sin) / Settings.MAXSIN;
                fromCenter.y = Mathf.Sign(fromCenter.y) * Screen.height / 2;

                fromCenter *= 0.9f;
            }

            indicator.anchoredPosition = fromCenter; // 화면 중앙으로부터 90% 거리에 위치
        }
        else
        {
            indicator.gameObject.SetActive(false); // 화살표 비활성화
        }
    }
}
