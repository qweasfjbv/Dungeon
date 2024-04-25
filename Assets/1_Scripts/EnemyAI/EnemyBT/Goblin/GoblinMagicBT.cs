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
                        new Search(transform, searchRange, "Human", enemyStat)
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
            if (GetNodeData("attackFlag") == null)
            {
                parent.parent.SetNodeData("attackFlag", true);

                EnemyBT.SetAnimatior(animator, "Attack");
                // Á×À¸¸é Áö¿ö¾ßµÊ

                var tr = (GameObject)GetNodeData("BossObject");

                if (tr == null)
                {
                    RemoveNodeData("BossObject");
                    return NodeState.Failure;
                }


                GameObject eff = EffectGenerator.Instance.InstanceEffect(magicEffect, tr.transform.position, Quaternion.identity);
                eff.GetComponent<IceEffect>().SetDamage(stat.Attack);

            }

            return NodeState.Success;
        }

    }
