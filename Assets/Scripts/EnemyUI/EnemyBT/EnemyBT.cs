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
        [SerializeField] private int trackRange;
        [SerializeField] private int attackRange;

        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                    new Sequence(new List<Node>
                    {
                        new Search(transform, searchRange, trackRange),
                        new Move(transform)
                    }),
                    new Sequence(new List<Node>
                    {
                        new Track(transform, attackRange),
                        new Attack(transform)
                    })
                });


            return root;
        }
    }

    public class Search : Node {

        private Transform transform;
        private int searchRange;
        private int trackRange;

        public Search(Transform transform, int searchRange, int trackRange)
        {
            // 필요한 초기화
            this.transform = transform;
            this.searchRange = searchRange;
            this.trackRange = trackRange;
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


        public Move(Transform transform)
        {
            this.transform = transform;
        }

        public override NodeState Evaluate()
        {
            // 움직이는 로직 구현
            transform.position += new Vector3(0.1f, 0, 0);

            return NodeState.Running;
        }

    }
    public class Track : Node
    {
        private Transform transform;
        private int attackRange;

        public Track(Transform transform, int attackRange)
        {
            this.transform = transform;
            this.attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {
            return NodeState.Fail;
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
            Debug.Log(tr.transform.position);
            return NodeState.Success;
        }

    }
}