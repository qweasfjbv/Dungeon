using EnemyAI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyAI.BehaviorTree
{

    public class GoblinMagicBT : BTree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;
        [SerializeField] private GameObject magicPrefab;

        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                new Selector(new List<Node>
                {
                     new Sequence(new List<Node>
                    {
                        new IsDead(transform, enemyStat),
                        new Disappear(transform, enemyStat)
                    }) ,
                     new IsBlocked(transform, enemyStat)
                }),
                new Sequence(new List<Node>
                {
                    new Search(transform, searchRange, Constants.TAG_ENEMY)
                }),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new IsAttackable(transform, attackRange),
                        new MagicAttack(transform, enemyStat, magicPrefab)
                    }),
                    new LinearTrack(transform, enemyStat, attackRange),
                })
            });


            return root;
        }

    }


    public class MagicAttack : Node
    {
        private Transform transform;
        private Animator animator;
        private EnemyStat stat;
        private GameObject magicEffect;
        public MagicAttack(Transform transform, EnemyStat stat, GameObject magicEffect)
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
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_ATK);

                GameObject eff = EffectGenerator.Instance.InstanceEffect(magicEffect, tr.transform.position, Quaternion.identity);
                eff.GetComponent<BaseMagicEffect>().SetDamage(stat.Attack, Constants.TAG_ENEMY);
            }

            return NodeState.Success;
        }

    }

}