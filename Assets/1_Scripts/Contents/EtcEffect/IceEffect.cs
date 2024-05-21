using EnemyAI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceEffect : BaseMagicEffect
{

    private string animName = Constants.ICECRIS_ANIM_NAME;

    void Update()
    {
        if (isSet && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(animName))
        {
            float animTime = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;

            if (animTime >= 1.0f)
            {
                Destroy(gameObject);
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
