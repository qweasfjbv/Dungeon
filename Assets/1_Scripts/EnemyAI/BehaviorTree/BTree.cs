using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnemyAI.BehaviorTree
{
    [Serializable]
    public class Buff
    {
        static readonly int maxValue = 100;

        private Define.BuffType type;
        private int bValue;
        private float duration;

        public Define.BuffType Type { get { return type; } }    
        public int Value { get { return bValue; } }
        public float Duration { get { return duration; } set => duration = value; }
        // 곱연산을 위해 구해줍니다.
        public float Percent { get { return (maxValue + bValue) / (float)maxValue; } }

        public Buff(Define.BuffType type, int value, float duration)
        {
            this.type = type;
            this.bValue = value;
            this.duration = duration;
        }
        // duration이 끝나면 true반환

        public bool Tick()
        {
            duration -= Time.deltaTime;
            return duration <= 0;
        }


    }


    [Serializable]
    public class EnemyStat
    {
        private float originSpd;
        private float originAtk;
        private float maxHp;
        private float originDf;
        private float originCT;

        private float moveSpeed;
        private float attack;
        private float hp;
        private float attackCooltime;
        private float defense;

        private float[] values = new float[System.Enum.GetValues(typeof(Define.BuffType)).Length];
        private List<Buff> buffs = new List<Buff>();

        public float MaxHp { get => maxHp; set => hp = value; }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float Hp { get => hp; set => hp = value; }
        public float Attack { get => attack; set => attack = value; }
        public float Cooltime { get => attackCooltime; }


        // TODO : 생성자에서 ID와 LV받고 초기화하는걸로 수정
        public EnemyStat(float moveSpeed, float attack, float hp, float attackCooltime, float defense)
        {
            this.originSpd = this.moveSpeed=  moveSpeed;
            this.originAtk = this.attack =  attack;
            this.maxHp = this.hp = hp;
            this.originCT = this.attackCooltime = attackCooltime;
            this.originDf = this.defense = defense;
        }
        
        // 매 프레임마다 호출. duration 확인후 버프 제거
        public void OnUpdate()
        {
            for(int i=0; i<buffs.Count; i++)
            {
                if (buffs[i].Tick())
                {
                    buffs.RemoveAt(i);
                    UpdateStat();
                }
            }
        }

        // 스탯 업데이트
        public void UpdateStat()
        {
            Array.Fill(values, 1);
            for (int i = 0; i < buffs.Count; i++)
            {
                values[(int)buffs[i].Type] *= buffs[i].Percent;
            }

            attack = originAtk * values[(int)Define.BuffType.Atk];
            defense = originDf * values[(int)Define.BuffType.Def];
            moveSpeed = originSpd * values[(int)Define.BuffType.Spd];

        }

        public void AddBuff(Define.BuffType type, int value, float duration)
        {
            buffs.Add(new Buff(type, value, duration));
            UpdateStat();
        }

        public void RemoveAllBuffs()
        {
            buffs.Clear();
            UpdateStat();
        }

    }

    public abstract class BTree : MonoBehaviour
    {
        private Node root = null;

        [SerializeField]
        protected EnemyStat enemyStat = new EnemyStat(0.05f, 3, 10, 2.0f, 1f);


        private void OnEnable()
        {
            root = SetupRoot();
        }

        private void OnDisable()
        {
            enemyStat.RemoveAllBuffs();
        }

        private void Update()
        {
            enemyStat.OnUpdate();
            if (root != null) root.Evaluate();
        }


        public abstract Node SetupRoot();

        public void AddBuff(Define.BuffType type, int value, float duration)
        {
            enemyStat.AddBuff(type, value, duration);
        }

        public static void SetAnimatior(Animator anim, string name)
        {
            anim.SetBool(Constants.ANIM_PARAM_ATK, false);
            anim.SetBool(Constants.ANIM_PARAM_DIE, false);
            anim.SetBool(Constants.ANIM_PARAM_DMG, false);
            anim.SetBool(Constants.ANIM_PARAM_WALK, false);
            anim.SetBool(Constants.ANIM_PARAM_IDLE, false);

            anim.SetBool(name, true);

        }

        public bool OnDamaged(float damage, Define.AtkType type)
        {
            Debug.Log("Damaged");

            // TODO : 타입에 따른 처리 필요
            SoundManager.Instance.PlayEffectSound(Define.EffectSoundType.Hit);
            enemyStat.Hp -= damage;

            if (enemyStat.Hp > 0)
            {
                var animator = transform.GetComponent<Animator>();
                SetAnimatior(animator, Constants.ANIM_PARAM_DMG);

                return false;
            }
            else return true;
        }
        public void OnRecover(float damage)
        {
            if (enemyStat.Hp > 0)
            {
                enemyStat.Hp = Mathf.Clamp(enemyStat.Hp + damage, 0, enemyStat.MaxHp);
            }
        }

        // TODO : 수정해야합니다.
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
            // 주변에 아무것도 없는 경우
            if (cols.Length == 0) return null;

            GameObject ret = null;
            float minDis = -1;
            

            // 주변에 뭔가 있으면 tagName으로 가장 가까운 적을 찾습니다.            
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