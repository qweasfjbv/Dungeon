using EnemyAI.BehaviorTree;
using System.Collections.Generic;
using UnityEngine;


public class CombatHPBarController : MonoBehaviour
{

    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        // 초기 스케일 및 위치 저장
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

        // HP 비율 계산
        float hpRatio = transform.parent.GetComponent<BTree>().GetHpRatio();

        // Foreground 스케일 조정
        transform.localScale = new Vector3(originalScale.x * hpRatio, originalScale.y, originalScale.z);

        // Foreground 위치 조정 (오른쪽에서 왼쪽으로 닳게 함)
        float offset = (originalScale.x - transform.localScale.x) / 2;
        transform.localPosition = new Vector3(originalPosition.x - offset, originalPosition.y, originalPosition.z);
    }


}
