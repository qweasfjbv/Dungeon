using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceEffect : MonoBehaviour
{

    // have to be set
    private float damage;
    private string tagName;

    private bool isSet = false;

    public void SetDamage(float damage)
    {
        this.damage = damage;

        isSet = true;
    }

    // TODO : 상속 할 것들과 안 할 것들을 구분해서 올료야합니다.
    // Abstract 함수로 Onudpate 만들어두고 인자로 animName 넣는 경우도 생각해봐야 합니다.
    // OnTriggerEnter2D는 각자 구현해야 합니다.
    // 생성/ 투사체/폭발에 따라 다르게 구현해야 합니다.

    private string animName = Constants.ICECRIS_ANIM_NAME;

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
        if (other.CompareTag(tagName))
        {
            EnemyBT enemy = other.GetComponent<EnemyBT>();
            if (enemy != null)
            {
                enemy.OnDamaged(damage, Define.AtkType.Ice);
            }
        }
    }


}
