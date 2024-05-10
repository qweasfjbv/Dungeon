using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Indicator : MonoBehaviour
{
    public Transform target; // ���� ��ġ�� ����ų ���
    public RectTransform indicator; // UI ȭ��ǥ
    private Camera mainCamera;


    private void OnEnable()
    {
        mainCamera = Camera.main; // ���� ī�޶� �Ҵ�
        indicator.gameObject.SetActive(false); // �ʱ⿡�� ȭ��ǥ�� ��Ȱ��ȭ
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
            indicator.gameObject.SetActive(true); // ȭ��ǥ Ȱ��ȭ
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position);
            Vector3 fromCenter = screenPosition - new Vector3(Screen.width / 2, Screen.height / 2, 0);
            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg - 90; // -90�� �߰��Ͽ� ȭ��ǥ�� ���� ���ϵ��� ����
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

            indicator.anchoredPosition = fromCenter; // ȭ�� �߾����κ��� 90% �Ÿ��� ��ġ
        }
        else
        {
            indicator.gameObject.SetActive(false); // ȭ��ǥ ��Ȱ��ȭ
        }
    }
}
