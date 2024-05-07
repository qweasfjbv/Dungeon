using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public class EnemyBT : BTree
    {

        [SerializeField] private int searchRange;
        [SerializeField] private float attackRange;

        private Vector2Int destination;

        public void SetValues(Vector2Int dest)
        {
            destination = dest;
        }
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
                        new Search(transform, searchRange, Constants.TAG_MONSTER, enemyStat),
                        new Move(transform, destination, enemyStat)
                    }),
                    new Sequence(new List<Node>
                    {
                        new IsAttacking(transform),
                        new Track(transform, attackRange, enemyStat),
                        new Attack(transform, enemyStat)
                    })
                });


            return root;
        }

    }

    public class IsDead : Node{

        private Transform transform;
        private EnemyStat stat;

        private float damageTrigger = 0f;

        public IsDead(Transform transform, EnemyStat stat)
        {
            this.stat = stat;
            this.transform = transform;
        }

        public override NodeState Evaluate()
        {

            // �������� ���� ���� (�ִϸ��̼��� �������� ����)
            if (transform.GetComponent<Animator>().GetBool(Constants.ANIM_PARAM_DMG))
            {
                // �ִϸ��̼��� �����ٸ� IDLE�� �ٲ��ְ� Failure ��ȯ
                // 
                damageTrigger += Time.deltaTime;
                if(damageTrigger >= 0.4f)
                {
                    damageTrigger = 0f;
                    EnemyBT.SetAnimatior(transform.GetComponent<Animator>(), Constants.ANIM_PARAM_IDLE);
                    return NodeState.Failure;
                }

                // �ִϸ��̼��� �������̶�� Success��ȯ
                // -> Selector���� ���� ���� ������ �ȵǹǷ� Move�� �ȵ�
                return NodeState.Success;
            }

            // �״� ��� �ִϸ��̼� ���� �� Success ��ȯ
            if (stat.Hp <= 0)
            {
                if (!transform.GetComponent<Animator>().GetBool(Constants.ANIM_PARAM_DIE))
                    EnemyBT.SetAnimatior(transform.GetComponent<Animator>(), Constants.ANIM_PARAM_DIE);

                return NodeState.Success;
            }
            else
            {
                return NodeState.Failure;
            }
        }
    }

    public class Disappear : Node
    {
        private Transform transform;
        private EnemyStat stat;
        private Animator animator;

        public Disappear(Transform transform, EnemyStat stat)
        {
            this.transform = transform;
            animator = transform.GetComponent<Animator>();
            this.stat = stat;
        }

        public override NodeState Evaluate()
        {
            // �׾����� Destroy�� ����
            // TODO : ī�� Pooling �����ϰ� ���� SetActive(false)�� ��ü
            if (animator.GetBool(Constants.ANIM_PARAM_DIE) && stat.Hp <= 0)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(Constants.DIE_ANIM_NAME) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    GameObject.Destroy(transform.gameObject);
                }
            }
            return NodeState.Success;
        }
    }

    public class Search : Node {

        private Transform transform;
        private int searchRange;
        private string tagName;
        private Animator animator;
        private EnemyStat stat;
        private float coolTime = 0f;

        public Search(Transform transform, int searchRange, string tagName, EnemyStat stat)
        {
            // �ʿ��� �ʱ�ȭ
            this.transform = transform;
            this.searchRange = searchRange;
            this.tagName = tagName;
            this.stat = stat;
            animator = transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            // ���� ��Ÿ���� ��ϴ�.
            if (GetNodeData(Constants.NDATA_ATK) != null) coolTime += Time.deltaTime;
            
            // ��Ÿ�Ӹ�ŭ ��ٷ����� �ٽ� Atk�� �����մϴ�.
            // NDATA_ATK�� ������ 
            if (coolTime >= stat.Cooltime)
            {
                coolTime = 0;
                EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                RemoveNodeData(Constants.NDATA_ATK);
            }

            // ���� ����� �� ã��
            var cols = Physics2D.OverlapCircleAll(transform.position, searchRange);
            var nearGo = BTree.SearchEnemy(transform, cols, tagName);

            // ��ó�� ���� ������ Target�� ����, Path �缳���� �ؾ��Ѵٰ� �˷��ݴϴ�.
            // Failure�� �����ϱ� ������ ���� ����� �Ŀ� Move���� Path�� �缳���ϰ�,
            // Track�ϴ��� ������ ��ο��� �ٽ� Path�� �缳���˴ϴ�.
            if (nearGo != null)
            {
                parent.parent.SetNodeData(Constants.NDATA_TARGET, nearGo);
                parent.SetNodeData(Constants.NDATA_PATH, true);
                return NodeState.Failure;
            }

            // Track ���̰ų� ATK ���̸� �����̸� �ȵǹǷ� Failure ��ȯ
            if (GetNodeData(Constants.NDATA_TRACK) != null || GetNodeData(Constants.NDATA_ATK) != null) 
                return NodeState.Failure;

            // Success ��ȯ�ؼ� �����̵��� ����
            return NodeState.Success;
        }


    }
    public class Move : Node
    {

        private Transform transform;
        private Animator animator;
        private Rigidbody2D rigid;

        private List<Vector2> path;
        private Vector2Int dest;
        private int currentPointIndex = 0;

        private EnemyStat stat;
        public Move(Transform transform, Vector2Int dest, EnemyStat stat)
        {
            this.stat = stat;
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
            this.dest = dest;
            this.path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
                new Vector2Int((int)dest.y, (int)dest.x));

        }

        public override NodeState Evaluate()
        {

            // ��θ� �缳���϶�� DATA�� ������ �缳���ϰ� ���� ������ �ʱ�ȭ�մϴ�.
            if (GetNodeData(Constants.NDATA_PATH) != null)
            {
                RemoveNodeData(Constants.NDATA_PATH); 
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)dest.y, (int)dest.x));
                currentPointIndex = 0;
            }

            // path�� ���� ��� �ٷ� return
            if (path == null || path.Count == 0) return NodeState.Success;

            // �ִϸ��̼� ����
            if (!animator.GetBool(Constants.ANIM_PARAM_WALK))
                EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);


            Vector2 currentTarget = path[currentPointIndex];
            var step = stat.MoveSpeed* new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));

            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);

            // ��������Ʈ���� ���󰡰� ����� ����
            // ��� �Ÿ� ������ ������ ���� �������� ��������Ʈ�� ���� ���ϴ�.
            if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
            {
                currentPointIndex++;

                if (currentPointIndex >= path.Count)
                    transform.gameObject.SetActive(false);
            }
            return NodeState.Running;
        }

    }

    public class IsAttacking : Node {

        private Transform transform;
        private Animator animator;

        public IsAttacking(Transform transform)
        {
            animator = transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            var isAtt= animator.GetBool(Constants.ANIM_PARAM_ATK);
            if (isAtt)
            {
                // �ִϸ��̼��� ���� ������ ��ٸ�
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(Constants.ATK_ANIM_NAME) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                }
                return NodeState.Failure;
            }
            else
                return NodeState.Success; 
        }
    }

    public class Track : Node
    {
        private Transform transform;
        private float attackRange;
        private Animator animator;
        private Rigidbody2D rigid;
        private EnemyStat stat;
        private List<Vector2> path;

        private int currentPointIndex = 0;

        public Track(Transform transform, float attackRange, EnemyStat stat)
        {

            this.stat = stat;
            this.transform = transform;
            this.attackRange = attackRange;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
        }

        public override NodeState Evaluate()
        {
            var boss = (GameObject)GetNodeData(Constants.NDATA_TARGET);

            // ������ �׾��ų� ����� ���
            // TRACK, TARGET�� �����ϰ� Failure�� ��ȯ�մϴ�.
            if(boss == null || boss.CompareTag(Constants.TAG_DYING))
            {
                RemoveNodeData(Constants.NDATA_TRACK);
                RemoveNodeData(Constants.NDATA_TARGET);
                return NodeState.Failure;
            }

            // ������ ���� JPS�� �� ������ ȣ������ �ʽ��ϴ�.
            // NDATA_TRACK�� �����Ǿ����� ������ ��ΰ� �����Ƿ� ��θ� �����մϴ�.
            if (GetNodeData(Constants.NDATA_TRACK) == null)
            {
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)boss.transform.position.y, (int)boss.transform.position.x));
                currentPointIndex = 0;
                parent.parent.SetNodeData(Constants.NDATA_TRACK, true);
            }

            // Attack Range�ȿ� ���� ����Դϴ�.
            // Success�� ��ȯ�ؼ� Attack��带 �����ϵ��� �մϴ�.
            Vector3 dir = boss.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;
            if (dis2 < attackRange * attackRange) {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Success; 
            }

            // ���������� ���� ���� ����̹Ƿ� NDATA_TRACK�� �����մϴ�.
            if (currentPointIndex >= path.Count) {
                RemoveNodeData(Constants.NDATA_TRACK);
                return NodeState.Failure; }

            // ���� �̵� �� �ִϸ��̼� �����Դϴ�.
            Vector2 currentTarget = path[currentPointIndex];
            var step = stat.MoveSpeed * new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));
            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);
            EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);


            // Move�� �����ϰ� ��������Ʈ ��ó���� ��������Ʈ�� �ٲ��ݴϴ�.
            // Index�� ������ ������ Ȯ���ϹǷ� ���⼭�� ���� Ȯ������ �ʽ��ϴ�.
            if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
                currentPointIndex++;

            return NodeState.Running;
        }

    }

    public class Attack : Node
    {
        private Transform transform;
        private Animator animator;
        private EnemyStat stat;
        public Attack(Transform transform, EnemyStat stat)
        {
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.stat = stat;
        }


        public override NodeState Evaluate()
        {
            if (GetNodeData(Constants.NDATA_ATK) == null)
            {

                // Ÿ�� Ȯ��
                var tr = (GameObject)GetNodeData(Constants.NDATA_TARGET);
                if (tr == null || tr.CompareTag(Constants.TAG_DYING))
                {
                    RemoveNodeData(Constants.NDATA_TARGET);
                    return NodeState.Failure;
                }


                parent.parent.SetNodeData(Constants.NDATA_ATK, true);
                EnemyBT.SetAnimatior(animator, Constants.ANIM_PARAM_ATK);


                // AtkRange�� ���� Slash/Bow ���� ����
                if (tr.GetComponent<BTree>() != null && tr.GetComponent<BTree>().OnDamaged(stat.Attack, Define.AtkType.Slash))
                {
                   // ���� ������ �׿��� �� �����ϴ� �κ��Դϴ�.
                   // �߰����� ����� ������ �� ���� �� ���� ���ܵ׽��ϴ�.
                   // ex. ���� ���̸� ���� �ð����� ������ ���
                }

            }

            return NodeState.Success;
        }

    }

 
}