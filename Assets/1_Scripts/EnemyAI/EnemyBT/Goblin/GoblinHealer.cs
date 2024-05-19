using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnemyAI.BehaviorTree
{
    public class GoblinHealer : BTree
    {

        [SerializeField] private int searchRange;
        [SerializeField] private int healRange;

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
                    new HealSearch(transform, searchRange, Constants.TAG_MONSTER)
                }),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new IsAttackable(transform, healRange),
                        new Heal(transform, enemyStat)
                    }),
                    new LinearTrack(transform, enemyStat, healRange),
                })
            });


            return root;
        }

    }

    public class HealSearch: Node {

        private Transform transform;
        private int searchRange;
        private string tagName;

        public HealSearch(Transform transform, int searchRange, string tagName)
        {
            // �ʿ��� �ʱ�ȭ
            this.transform = transform;
            this.searchRange = searchRange;
            this.tagName = tagName;
        }


        public override NodeState Evaluate()
        {
            // ���� ����� �� ã��
            var target = (GameObject)GetNodeData(Constants.NDATA_TARGET);
            GameObject nearGo = null;

            if (target == null || target.CompareTag(Constants.TAG_DYING))
            {
                RemoveNodeData(Constants.NDATA_TRACK);
                RemoveNodeData(Constants.NDATA_TARGET);
                nearGo = BTree.SearchEnemy(transform, Physics2D.OverlapCircleAll(transform.position, searchRange), tagName);

            }
            else return NodeState.Failure;
            // �̹� ���� ã�� ��� Ž�� ����



            if (nearGo != null)
            {
                parent.parent.SetNodeData(Constants.NDATA_TARGET, nearGo);
                parent.SetNodeData(Constants.NDATA_PATH, true);
                return NodeState.Failure;
            }



            // Ž�� ����
            return NodeState.Success;
        }


    }


    public class Heal : Node
    {
        private Transform transform;
        private Animator animator;
        private EnemyStat stat;
        public Heal(Transform transform, EnemyStat stat)
        {
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.stat = stat;
        }

    }


}