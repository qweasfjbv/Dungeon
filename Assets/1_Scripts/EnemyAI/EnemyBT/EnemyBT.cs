using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnemyAI.BehaviorTree
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
                    new Search(transform, searchRange, Constants.TAG_MONSTER),
                    new Move(transform, destination, enemyStat)
                }),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new IsAttackable(transform, attackRange),
                        new Attack(transform, enemyStat)
                    }),
                    new Track(transform, enemyStat, attackRange),
                })
            });


            return root;
        }

    }


    public class IsBlocked : Node
    {
        private Animator animator;
        private EnemyStat stat;

        private float damageTrigger = 0f;
        private float atkTrigger = 0f;
        private float coolTime = 0f;

        public IsBlocked(Transform transform , EnemyStat stat)
        {
            this.animator = transform.GetComponent<Animator>();
            this.stat = stat;
        }


        public override NodeState Evaluate()
        {

            // ���� ��Ÿ���� ��ϴ�.
            if (GetNodeData(Constants.NDATA_ATK) != null)
            {
                coolTime += Time.deltaTime;
            }

            // ��Ÿ�Ӹ�ŭ ��ٷ����� �ٽ� Atk�� �����մϴ�.
            // NDATA_ATK�� ������ ������ �� �ֵ��� �մϴ�.
            if (coolTime >= stat.Cooltime)
            {
                coolTime = 0;
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                RemoveNodeData(Constants.NDATA_ATK);
            }




            if (animator.GetBool(Constants.ANIM_PARAM_ATK))
            {
                atkTrigger += Time.deltaTime;
                if (atkTrigger >= Constants.DMG_ANIM_TIME)
                {
                    atkTrigger = 0f;
                    BTree.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                    return NodeState.Failure;
                }

                return NodeState.Success;
            }
            else atkTrigger = 0f;





            // �������� ���� ���� (�ִϸ��̼��� �������� ����)
            if (animator.GetBool(Constants.ANIM_PARAM_DMG))
            {
                // �ִϸ��̼��� �����ٸ� IDLE�� �ٲ��ְ� Failure ��ȯ
                // 
                damageTrigger += Time.deltaTime;
                if (damageTrigger >= Constants.DMG_ANIM_TIME)
                {
                    damageTrigger = 0f;
                    BTree.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                    return NodeState.Failure;
                }


                // �ִϸ��̼��� �������̶�� Success��ȯ
                // -> Selector���� ���� ���� ������ �ȵǹǷ� Move�� �ȵ�
                return NodeState.Success;
            }
            else damageTrigger = 0f;

            return NodeState.Failure;

        }
    }

    public class IsDead : Node
    {

        private Transform transform;
        private EnemyStat stat;


        public IsDead(Transform transform, EnemyStat stat)
        {
            this.stat = stat;
            this.transform = transform;
        }

        public override NodeState Evaluate()
        {
            // �״� ��� �ִϸ��̼� ���� �� Success ��ȯ
            if (stat.Hp <= 0)
            {
                transform.tag = Constants.TAG_DYING;
                if (!transform.GetComponent<Animator>().GetBool(Constants.ANIM_PARAM_DIE))
                {
                    BTree.SetAnimatior(transform.GetComponent<Animator>(), Constants.ANIM_PARAM_DIE);
                }

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
                    SliderController.Instance.RestoreBlood(1f);
                    transform.gameObject.SetActive(false);
                }
            }
            return NodeState.Success;
        }
    }



    public class Search : Node {

        private Transform transform;
        private int searchRange;
        private string tagName;

        public Search(Transform transform, int searchRange, string tagName)
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


            // ��ó�� ���� ������ Target�� ����, Path �缳���� �ؾ��Ѵٰ� �˷��ݴϴ�.
            // Failure�� �����ϱ� ������ ���� ����� �Ŀ� Move���� Path�� �缳���ϰ�,
            // Track�ϴ��� ������ ��ο��� �ٽ� Path�� �缳���˴ϴ�.
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
            if (path == null || path.Count == 0)
            {
                return NodeState.Success;
            }

            // �ִϸ��̼� ����
            if (!animator.GetBool(Constants.ANIM_PARAM_WALK))
            {
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            }

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
                {
                    EventManager.Instance.EnemyPassed();
                    transform.gameObject.SetActive(false);
                }
            }
            return NodeState.Running;
        }

    }

    public class IsAttackable : Node {

        private Transform transform;
        private Animator animator;

        private float attackRange;

        public IsAttackable(Transform transform, float attackRange)
        {
            animator = transform.GetComponent<Animator>();
            this.transform = transform;
            this.attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {

            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);
            if (enemy == null || enemy.CompareTag(Constants.TAG_DYING)) return NodeState.Failure;


            // ���� ���� Ȯ��
            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;
            if (dis2 > attackRange * attackRange)
            {
                return NodeState.Failure;
            }


            // �ִϸ��̼� Ȯ��
            if (animator.GetBool(Constants.ANIM_PARAM_ATK))
            {
                return NodeState.Failure;
            }

            // ���� ��Ÿ�� Ȯ��
            if (GetNodeData(Constants.NDATA_ATK) != null) return NodeState.Failure;


            return NodeState.Success;
        }
    }

    public class Track : Node
    {
        private Transform transform;
        private Animator animator;
        private Rigidbody2D rigid;
        private EnemyStat stat;
        private List<Vector2> path;
        private float attackRange;

        private int currentPointIndex = 0;

        public Track(Transform transform, EnemyStat stat, float attackRange)
        {

            this.stat = stat;
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
            this.attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {
            var enemy = (GameObject)GetNodeData(Constants.NDATA_TARGET);


            // ������ ���� JPS�� �� ������ ȣ������ �ʽ��ϴ�.
            // NDATA_TRACK�� �����Ǿ����� ������ ��ΰ� �����Ƿ� ��θ� �����մϴ�.
            if (GetNodeData(Constants.NDATA_TRACK) == null)
            {
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)enemy.transform.position.y, (int)enemy.transform.position.x));
                currentPointIndex = 0;
                parent.parent.SetNodeData(Constants.NDATA_TRACK, true);
            }


            // Attack Range�ȿ� ���� ����Դϴ�.
            // Success�� ��ȯ�ؼ� Attack��带 �����ϵ��� �մϴ�.
            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;



            if (dis2 < attackRange * attackRange)
            {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Failure;
            }



            // ���������� ���� ���� ����̹Ƿ� NDATA_TRACK�� �����մϴ�.
            if (currentPointIndex >= path.Count)
            {
                RemoveNodeData(Constants.NDATA_TRACK);
                return NodeState.Failure;
            }

            


            // ���� �̵� �� �ִϸ��̼� �����Դϴ�.
            Vector2 currentTarget = path[currentPointIndex];
            var step = stat.MoveSpeed * new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));
            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);
            BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);


            // Move�� �����ϰ� ��������Ʈ ��ó���� ��������Ʈ�� �ٲ��ݴϴ�.
            // Index�� ������ ������ Ȯ���ϹǷ� ���⼭�� ���� Ȯ������ �ʽ��ϴ�.
            if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
            {
                currentPointIndex++;
            }

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

                // ���ȴٴ� flag�� ���� search ��忡�� ��Ÿ���� ���ݴϴ�
                parent.parent.parent.SetNodeData(Constants.NDATA_ATK, true);
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_ATK);


                // AtkRange�� ���� Slash/Bow ���� ����
                if (tr.GetComponent<BTree>() != null && tr.GetComponent<BTree>().OnDamaged(stat.Attack, Define.AtkType.Slash))
                {
                    // ���� ������ �׿��� �� �����ϴ� �κ��Դϴ�.
                    // �߰����� ����� ������ �� ���� �� ���� ���ܵ׽��ϴ�.
                    // ex. ���� ���̸� ���� �ð����� ������ ���
                }

                return NodeState.Success;
            }
            else return NodeState.Failure;

        }

    }

 
}