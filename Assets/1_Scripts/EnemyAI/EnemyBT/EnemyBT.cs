using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace EnemyUI.BehaviorTree
{
    [Serializable]
    public class EnemyStat
    {
        private float moveSpeed;
        private float attack;
        private float hp;
        private float attackCooltime;

        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float Hp { get => hp; set => hp = value; }
        public float Attack { get => attack; set => attack = value; }
        public float Cooltime { get => attackCooltime; }

        public EnemyStat(float moveSpeed, float attack, float hp, float attackCooltime)
        {
            this.moveSpeed = moveSpeed;
            this.attack = attack;
            this.hp = hp;
            this.attackCooltime = attackCooltime;
        }
    }
    public class EnemyBT : BTree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;

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
                        new Search(transform, searchRange, "Monster", enemyStat),
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

            // damage 받았을떄 success return해서 move는 안가게
            if (transform.GetComponent<Animator>().GetBool("Damage"))
            {
                damageTrigger += Time.deltaTime;
                if(damageTrigger >= 0.4f)
                {
                    damageTrigger = 0f;
                    EnemyBT.SetAnimatior(transform.GetComponent<Animator>(), "Idle");
                    return NodeState.Failure;
                }

                return NodeState.Success;
            }
            if (stat.Hp <= 0)
            {
                if (!transform.GetComponent<Animator>().GetBool("Die"))
                {
                    EnemyBT.SetAnimatior(transform.GetComponent<Animator>(), "Die");
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
            if (animator.GetBool("Die") && stat.Hp <= 0)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die Blend Tree") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
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
        string tagName;
        private Animator animator;
        private EnemyStat stat;
        private float coolTime = 0f;

        public Search(Transform transform, int searchRange, string tagName, EnemyStat stat)
        {
            // 필요한 초기화
            this.transform = transform;
            this.searchRange = searchRange;
            this.tagName = tagName;
            this.stat = stat;
            animator = transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            if (GetNodeData("attackFlag") != null)
            {
                coolTime += Time.deltaTime;

            }

            if (coolTime >= stat.Cooltime)
            {
                coolTime = 0;
                EnemyBT.SetAnimatior(animator, "Idle");
                RemoveNodeData("attackFlag");
            }

            var cols = Physics2D.OverlapCircleAll(transform.position, searchRange);
            foreach (var col in cols)
            {
                if (col.CompareTag(tagName))
                {
                    parent.parent.SetNodeData("BossObject", col.gameObject);
                    parent.SetNodeData("pathfindFlag", true);
                    return NodeState.Failure;
                }
            }

            if (GetNodeData("isTracked") != null || GetNodeData("attackFlag") != null)
            { return NodeState.Failure; }
            //

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

            if (GetNodeData("pathfindFlag") != null)
            {

                RemoveNodeData("pathfindFlag"); 
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)dest.y, (int)dest.x));
                currentPointIndex = 0;
            }



            if (path == null || path.Count == 0) return NodeState.Success;

            EnemyBT.SetAnimatior(animator, "Walk");

            Vector2 currentTarget = path[currentPointIndex];

            var step = stat.MoveSpeed* new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));


            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);

            if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
            {
                currentPointIndex++;
                if (currentPointIndex >= path.Count)
                {
                    // 도착
                    transform.gameObject.SetActive(false);
                }
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
            var isAtt= animator.GetBool("Attack");
            if (isAtt)
            {

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack Blend Tree") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    EnemyBT.SetAnimatior(animator, "Idle");
                }
                return NodeState.Failure;
            }
            else { 
                
                return NodeState.Success; 

            }
        }
    }

    public class Track : Node
    {
        private Transform transform;
        private int attackRange;
        private Animator animator;
        private Rigidbody2D rigid;
        private EnemyStat stat;
        private List<Vector2> path;

        private int currentPointIndex = 0;

        public Track(Transform transform, int attackRange, EnemyStat stat)
        {

            this.stat = stat;
            this.transform = transform;
            this.attackRange = attackRange;
            this.animator = transform.GetComponent<Animator>();
            this.rigid = transform.GetComponent<Rigidbody2D>();
        }

        public override NodeState Evaluate()
        {

            // BossObject 변수 받고 따라가기
            var boss = (GameObject)GetNodeData("BossObject");

            if(boss == null || boss.CompareTag("Dying"))
            {
                // 보스를 처치한 경우
                // 다시 가던길 가면됨
                RemoveNodeData("isTracked");
                return NodeState.Failure;
            }
            Vector3 dir = boss.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;

            //
            if (GetNodeData("isTracked") == null)
            {
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)boss.transform.position.y, (int)boss.transform.position.x));
                currentPointIndex = 0;
                parent.parent.SetNodeData("isTracked", true);
            }


            // 성공 -> Seq의 다음노드 실행
            if (dis2 < attackRange * attackRange) {
                animator.SetBool("Walk", false);
                return NodeState.Success; 
            }

            EnemyBT.SetAnimatior(animator, "Walk");



            // 실제 이동 구현부분
            if (currentPointIndex >= path.Count) {

                RemoveNodeData("isTracked");
                return NodeState.Failure; }


            Vector2 currentTarget = path[currentPointIndex];

            var step = stat.MoveSpeed * new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;


            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));


            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);

            if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
            {
                currentPointIndex++;
                if (currentPointIndex >= path.Count)
                {
                    // 도착
                    RemoveNodeData("isTracked");
                    return NodeState.Failure;
                }
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
            if (GetNodeData("attackFlag") == null)
            {
                parent.parent.SetNodeData("attackFlag", true);

                EnemyBT.SetAnimatior(animator, "Attack");
                // 죽으면 지워야됨

                var tr = (GameObject)GetNodeData("BossObject");

                if (tr == null)
                {
                    RemoveNodeData("BossObject");
                    return NodeState.Failure;
                }

                
                // 떄리는 로직
                // 때리고 죽었으면
                if (tr.GetComponent<GoblinBT>() != null && tr.GetComponent<GoblinBT>().OnDamaged(stat.Attack))
                {
                    tr.tag = "Dying";
                    RemoveNodeData("BossObject");
                }
                if (tr.GetComponent<EnemyBT>() != null && tr.GetComponent<EnemyBT>().OnDamaged(stat.Attack))
                {
                    tr.tag = "Dying";
                    RemoveNodeData("BossObject");
                }
            }

            return NodeState.Success;
        }

    }

 
}