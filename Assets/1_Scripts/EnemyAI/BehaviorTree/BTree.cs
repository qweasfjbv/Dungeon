using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public abstract class BTree : MonoBehaviour
    {
        // Constructs

        private Node root = null;

        private void OnEnable()
        {
            root = SetupRoot();
        }
        private void Update()
        {
            if (root != null) root.Evaluate();
        }


        public abstract Node SetupRoot();


        // Contents
        // TODO : 각 Tree로 옮겨야함
        [SerializeField]
        protected EnemyStat enemyStat = new EnemyStat(0.05f, 3, 10, 2.0f);

        public static void SetAnimatior(Animator anim, string name)
        {
            Debug.Log(name);
            anim.SetBool("Attack", false);
            anim.SetBool("Die", false);
            anim.SetBool("Damage", false);
            anim.SetBool("Walk", false);
            anim.SetBool("Idle", false);

            anim.SetBool(name, true);

        }

        public bool OnDamaged(float damage)
        {
            SoundManager.Instance.PlayEffectSound(Define.EffectSoundType.Hit);
            enemyStat.Hp -= damage;
            if (enemyStat.Hp > 0)
            {
                var animator = transform.GetComponent<Animator>();
                SetAnimatior(animator, "Damage");

                return false;
            }
            else return true;
        }
        public void OnRecover(float damage)
        {
            if (enemyStat.Hp > 0)
            {
                enemyStat.Hp += damage;
            }
        }

        public void MoveDebuff(float w)
        {
            enemyStat.MoveSpeed /= w;
        }
        public void MoveBuff(float w)
        {
            enemyStat.MoveSpeed *= w;
        }

    }

}