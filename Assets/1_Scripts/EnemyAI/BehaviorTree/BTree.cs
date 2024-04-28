using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
        // TODO : �� Tree�� �Űܾ���
        [SerializeField]
        protected EnemyStat enemyStat = new EnemyStat(0.05f, 3, 10, 2.0f);

        public static void SetAnimatior(Animator anim, string name)
        {
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

        public static GameObject SearchEnemy(Transform transform, Collider2D[] cols, string tagName)
        {
            if (cols.Length == 0) return null;

            GameObject ret = null;
            float minDis = -1;
            foreach (Collider2D col in cols)
            {
                if(!col.CompareTag(tagName)) continue;
                if (ret == null) { ret = col.gameObject; minDis = UtilFunctions.VectorDistanceSq(ret.transform.position, transform.position); continue; }

                var tmpDis = UtilFunctions.VectorDistanceSq(col.gameObject.transform.position, transform.position);

                if (minDis > tmpDis)
                {
                    minDis = tmpDis;
                    ret = col.gameObject;
                }
                

            }
            return ret;

        }

    }

}