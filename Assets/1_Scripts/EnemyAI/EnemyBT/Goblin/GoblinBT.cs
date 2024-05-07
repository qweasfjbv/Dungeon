using System.Collections.Generic;
using UnityEngine;

namespace EnemyUI.BehaviorTree
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
            // 따라가는 도중에 죽었다면 Failure 반환.
            // NDATA_TARGET을 지웠으므로 Search에서 다시 찾아야함
            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);
            if (enemy == null || enemy.CompareTag(Constants.TAG_DYING))
            {
                RemoveNodeData(Constants.NDATA_TARGET);
                return NodeState.Failure;
            }

            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;

            animator.SetFloat("X", dir.x);
            animator.SetFloat("Y", dir.y);

            // AttackRange안에 들어온 경우
            // Success 반환 -> Seq에 의해 Atk실행
            if (dis2 < attackRange * attackRange)
            {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Success;
            }

            // AttackRange에 들어오지 않았지만 Track은 하고있는 경우
            // 계속 이동
            EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            dir.Normalize();
            rigid.MovePosition(transform.position + stat.MoveSpeed * dir);

            return NodeState.Running;
        }

    }
}