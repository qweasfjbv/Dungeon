using EnemyAI.BehaviorTree;
using System.Collections.Generic;
using UnityEngine;


public class CombatHPBarController : MonoBehaviour
{

    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        // �ʱ� ������ �� ��ġ ����
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (transform.CompareTag(Constants.TAG_DYING))
        {

            transform.localScale = new Vector3(0, 0, 0);
            return;
        }

        // HP ���� ���
        float hpRatio = transform.parent.GetComponent<BTree>().GetHpRatio();

        // Foreground ������ ����
        transform.localScale = new Vector3(originalScale.x * hpRatio, originalScale.y, originalScale.z);

        // Foreground ��ġ ���� (�����ʿ��� �������� ��� ��)
        float offset = (originalScale.x - transform.localScale.x) / 2;
        transform.localPosition = new Vector3(originalPosition.x - offset, originalPosition.y, originalPosition.z);
    }


}
