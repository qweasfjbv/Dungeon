using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnemyUI.BehaviorTree
{
    public class EnemyBT : Tree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float trackSpeed;

        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                    new Sequence(new List<Node>
                    {
                        new Search(transform, searchRange),
                        new Move(transform, moveSpeed)
                    }),
                    new Sequence(new List<Node>
                    {
                        new Track(transform, attackRange, trackSpeed),
                        new Attack(transform)
                    })
                });


            return root;
        }
    }

    public class Search : Node {

        private Transform transform;
        private int searchRange;

        public Search(Transform transform, int searchRange)
        {
            // 필요한 초기화
            this.transform = transform;
            this.searchRange = searchRange;
        }

        public override NodeState Evaluate()
        {
            var cols = Physics2D.OverlapCircleAll(transform.position, searchRange);
            foreach (var col in cols)
            {
                if (col.CompareTag("Monster"))
                {
                    parent.parent.SetNodeData("BossObject", col.gameObject);
                    return NodeState.Fail;
                }
            }
            return NodeState.Success;
        }


    }
    public class Move : Node
    {

        private Transform transform;
        private float moveSpeed;

        public Move(Transform transform, float moveSpeed)
        {
            this.transform = transform;
            this.moveSpeed = moveSpeed;
        }

        public override NodeState Evaluate()
        {
            transform.position += new Vector3(moveSpeed, 0, 0);
            Debug.Log("Moving");
            return NodeState.Running;
        }

    }
    public class Track : Node
    {
        private Transform transform;
        private int attackRange;
        private float trackSpeed;

        public Track(Transform transform, int attackRange, float trackSpeed)
        {
            this.transform = transform;
            this.attackRange = attackRange;
            this.trackSpeed = trackSpeed;
        }

        public override NodeState Evaluate()
        {
            // BossObject 변수 받고 따라가기
            var boss = (GameObject)GetNodeData("BossObject");

            Vector3 dir = boss.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;


            // 성공 -> Seq의 다음노드 실행
            if (dis2 < attackRange* attackRange) return NodeState.Success;

            dir.Normalize();
            this.transform.position += trackSpeed * dir;
            Debug.Log("Tracking");
            return NodeState.Running;
        }

    }


    public class Attack : Node
    {
        private Transform transform;
        public Attack(Transform transform)
        {
            this.transform = transform;
        }


        public override NodeState Evaluate()
        {
            var tr = (GameObject)GetNodeData("BossObject");
            Debug.Log("Attack");
            return NodeState.Success;
        }

    }
}