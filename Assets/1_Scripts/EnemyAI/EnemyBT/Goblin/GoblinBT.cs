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
                        new Attack(transform, enemyStat)
                    })
                });


            return root;
        }

    }


    public class LinearTrack : Node
    {
        private Transform transform;
        private int attackRange;
        private Animator animator;
        private Rigidbody2D rigid;
        private EnemyStat stat;

        public LinearTrack(Transform transform, int attackRange, EnemyStat stat)
        {

            this.stat = stat;
            this.transform = transform;
            this.attackRange = attackRange;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
        }

        public override NodeState Evaluate()
        {
            // ���󰡴� ���߿� �׾��ٸ� Failure ��ȯ.
            // NDATA_TARGET�� �������Ƿ� Search���� �ٽ� ã�ƾ���
            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);

            if (enemy == null || enemy.CompareTag(Constants.TAG_DYING))
            {
                RemoveNodeData(Constants.NDATA_TARGET);
                RemoveNodeData(Constants.NDATA_TRACK);
                return NodeState.Failure;
            }


            // Enemy.BT�� Search�� �����ϱ� ���� ���
            // Track�� �����ߴ� �˷��� failure return -> Selector���� ������ ����
            if (GetNodeData(Constants.NDATA_TRACK) == null)
            {
                parent.parent.SetNodeData(Constants.NDATA_TRACK, true);
            }



            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;

            animator.SetFloat("X", dir.x);
            animator.SetFloat("Y", dir.y);

            // AttackRange�ȿ� ���� ���
            // Success ��ȯ -> Seq�� ���� Atk����
            if (dis2 < attackRange * attackRange)
            {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Success;
            }

            // AttackRange�� ������ �ʾ����� Track�� �ϰ��ִ� ���
            // ��� �̵�
            BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            dir.Normalize();
            rigid.MovePosition(transform.position + stat.MoveSpeed * dir);

            return NodeState.Running;
        }

    }
}