using EnemyAI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderEffect : BaseMagicEffect
{

    private string animName = Constants.THUNDER_ANIM_NAME;

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
                enemy.OnDamaged(damage, Define.AtkType.Thunder);
            }
        }
    }

}
