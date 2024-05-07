using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GoblinMagicBT : BTree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;
        [SerializeField] private GameObject icePrefab;
        

        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                    new Sequence(new List<Node>
                    {
                        new IsDead(transform, enemyStat),
                        new Disappear(transform, enemyStat)
                    }) ,
                    new Sequence(new List<Node>
                    {
                        new Search(transform, searchRange, Constants.TAG_ENEMY, enemyStat)
                    }),
                    new Sequence(new List<Node>
                    {
                        new IsAttacking(transform),
                        new LinearTrack(transform, attackRange, enemyStat),
                        new IceMagicAttack(transform, enemyStat, icePrefab)
                    })
                });


            return root;
        }

    }


    public class IceMagicAttack : Node
    {
        private Transform transform;
        private Animator animator;
        private EnemyStat stat;
        private GameObject magicEffect;
        public IceMagicAttack(Transform transform, EnemyStat stat, GameObject magicEffect)
        {
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.stat = stat;
            this.magicEffect = magicEffect;
        }


        public override NodeState Evaluate()
        {
            if (GetNodeData(Constants.NDATA_ATK) == null)
            {
                var tr = (GameObject)GetNodeData(Constants.NDATA_TARGET);
                if (tr == null || tr.CompareTag(Constants.TAG_DYING))
                {
                    RemoveNodeData(Constants.NDATA_TARGET);
                    return NodeState.Failure;
                }

                parent.parent.SetNodeData(Constants.NDATA_ATK, true);
                EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_ATK);

                // IceEffect, ThunderEffect는 디버깅용으로 만든 클래스입니다.
                // TODO : IceEffect와 ThunderEffect의 공통부모를 만들어서 호출.
                // -> MagitAttack노드에 생성자에 인자 하나 추가하고 재활용 가능합니다.
                GameObject eff = EffectGenerator.Instance.InstanceEffect(magicEffect, tr.transform.position, Quaternion.identity);
                eff.GetComponent<IceEffect>().SetDamage(stat.Attack);

            }

            return NodeState.Success;
        }

    }
