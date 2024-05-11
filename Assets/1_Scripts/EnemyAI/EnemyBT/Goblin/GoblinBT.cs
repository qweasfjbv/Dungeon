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
            // ���󰡴� ���߿� �׾��ٸ� Failure ��ȯ.
            // NDATA_TARGET�� �������Ƿ� Search���� �ٽ� ã�ƾ���
            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);


            // Enemy.BT�� Search�� �����ϱ� ���� ���
            // Track�� �����ߴ� �˷��� failure return -> Selector���� ������ ����
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

            // AttackRange�� ������ �ʾ����� Track�� �ϰ��ִ� ���
            // ��� �̵�
            BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            dir.Normalize();
            rigid.MovePosition(transform.position + stat.MoveSpeed * dir);

            return NodeState.Running;
        }

    }
}