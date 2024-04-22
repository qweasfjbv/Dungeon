using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceEffect : MonoBehaviour
{

    private float damage;
    private string animName = "IceCrystal";

    private bool isSet = false;

    public void SetDamage(float damage)
    {
        this.damage = damage;

        isSet = true;
    }

    void Update()
    {
        if (isSet && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(animName))
        {
            float animTime = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;

            if (animTime >= 1.0f)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Human"))
        {
            EnemyBT enemy = other.GetComponent<EnemyBT>();  // 'Enemy' 스크립트를 참조
            if (enemy != null)
            {
                enemy.OnDamaged(damage);  // 적에게 피해를 입힘
            }
        }
    }


}
