using System.Collections.Generic;
using UnityEngine;

namespace EnemyAI.BehaviorTree
{
    public class GoblinBT : BTree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;

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
                        new Attack(transform, enemyStat)
                    }),
                    new LinearTrack(transform, enemyStat, attackRange),
                })
            });


            return root;
        }

    }


    public class LinearTrack : Node
    {
        private Transform transform;
        private Animator animator;
        private Rigidbody2D rigid;
        private EnemyStat stat;
        private float attackRange;

        public LinearTrack(Transform transform, EnemyStat stat, float attackRange)
        {

            this.stat = stat;
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
            this.attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {
            // 따라가는 도중에 죽었다면 Failure 반환.
            // NDATA_TARGET을 지웠으므로 Search에서 다시 찾아야함
            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);


            // Enemy.BT의 Search를 재사용하기 위해 사용
            // Track을 시작했다 알려야 failure return -> Selector에서 막히지 않음
            if (GetNodeData(Constants.NDATA_TRACK) == null)
            {
                parent.parent.SetNodeData(Constants.NDATA_TRACK, true);
            }

            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;



            if (dis2 < attackRange * attackRange)
            {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Failure;
            }


            animator.SetFloat("X", dir.x);
            animator.SetFloat("Y", dir.y);

            // AttackRange에 들어오지 않았지만 Track은 하고있는 경우
            // 계속 이동
            BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            dir.Normalize();
            rigid.MovePosition(transform.position + stat.MoveSpeed * dir);

            return NodeState.Running;
        }

    }
}