using System.Collections.Generic;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public class EnemyStat
    {
        private float moveSpeed;
        private float attack;
        private float hp;

        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float Hp { get => hp; set => hp = value; }

        public EnemyStat(float moveSpeed, float attack, float hp)
        {
            this.moveSpeed = moveSpeed;
            this.attack = attack;
            this.hp = hp;
        }
    }
    public class EnemyBT : BTree
    {
        [SerializeField] private int searchRange;
        [SerializeField] private int attackRange;

        private EnemyStat enemyStat = new EnemyStat(0.05f, 3, 10);



        private Vector2Int destination;

        public void SetValues(Vector2Int dest)
        {
            destination = dest;
        }
        public void OnDamaged(float damage)
        {
            Debug.Log("ENEMY DAMAGED!");
            enemyStat.Hp -= damage;
        }

        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                    new Sequence(new List<Node>
                    {
                        new IsDead(transform, enemyStat),
                        new Disappear(transform)
                    }) ,
                    new Sequence(new List<Node>
                    {
                        new Search(transform, searchRange),
                        new Move(transform, destination, enemyStat)
                    }),
                    new Sequence(new List<Node>
                    {
                        new IsAttacking(transform),
                        new Track(transform, attackRange, enemyStat),
                        new Attack(transform)
                    })
                });


            return root;
        }

        public void MoveDebuff(float w)
        {
            enemyStat.MoveSpeed /= w;
        }
        public void MoveBuff(float w)
        {
            enemyStat.MoveSpeed *= w;
        }
    }

    public class IsDead : Node{

        private Transform transform;
        private EnemyStat stat;

        public IsDead(Transform transform, EnemyStat stat)
        {
            this.stat = stat;
            this.transform = transform;
        }

        public override NodeState Evaluate()
        {
            if (stat.Hp <= 0)
            {

                if (!transform.GetComponent<Animator>().GetBool("Die"))
                {
                    transform.GetComponent<Animator>().SetBool("Walk", false);
                    transform.GetComponent<Animator>().SetTrigger("Die");
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

        public Disappear(Transform transform)
        {
            this.transform = transform;
        }

        public override NodeState Evaluate()
        {
            if (!transform.GetComponent<Animator>().GetBool("Die"))
            {
                GameObject.Destroy(transform.gameObject);
            }
            return NodeState.Success;
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
                    parent.SetNodeData("pathfindFlag", true);
                    return NodeState.Failure;
                }
            }
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
                dest);
        }

        public override NodeState Evaluate()
        {
            if (GetNodeData("pathfindFlag") != null)
            {
                RemoveNodeData("pathfindFlag"); 
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            dest);
                currentPointIndex = 0;
            }

            animator.SetBool("Walk", true);
            if (path == null || path.Count == 0) return NodeState.Success;

            Vector2 currentTarget = path[currentPointIndex];

            var step = stat.MoveSpeed* new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));


            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);

            if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
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

            Vector3 dir = boss.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;

            animator.SetFloat("X", dir.x);
            animator.SetFloat("Y", dir.y);

            // 성공 -> Seq의 다음노드 실행
            if (dis2 < attackRange * attackRange) {
                animator.SetBool("Walk", false);
                return NodeState.Success; 
            }

            animator.SetBool("Walk", true);
            dir.Normalize();
            rigid.MovePosition(transform.position + stat.MoveSpeed * dir);

            return NodeState.Running;
        }

    }

    public class Attack : Node
    {
        private Transform transform;
        private Animator animator;
        public Attack(Transform transform)
        {
            this.transform = transform;
            this.animator = transform.GetComponent<Animator>();
        }


        public override NodeState Evaluate()
        {
            if (!animator.GetBool("Attack"))
                animator.SetTrigger("Attack");

            var tr = (GameObject)GetNodeData("BossObject");
            return NodeState.Success;
        }

    }

 
}