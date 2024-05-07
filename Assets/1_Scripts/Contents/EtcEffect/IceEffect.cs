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

    // TODO : ��� �� �͵�� �� �� �͵��� �����ؼ� �÷���մϴ�.
    // Abstract �Լ��� Onudpate �����ΰ� ���ڷ� animName �ִ� ��쵵 �����غ��� �մϴ�.
    // OnTriggerEnter2D�� ���� �����ؾ� �մϴ�.
    // ����/ ����ü/���߿� ���� �ٸ��� �����ؾ� �մϴ�.

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
