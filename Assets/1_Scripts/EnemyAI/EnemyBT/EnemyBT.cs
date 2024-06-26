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

            // 공격 쿨타임을 잽니다.
            if (GetNodeData(Constants.NDATA_ATK) != null)
            {
                coolTime += Time.deltaTime;
            }

            // 쿨타임만큼 기다렸으면 다시 Atk이 가능합니다.
            // NDATA_ATK을 지워서 공격할 수 있도록 합니다.
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





            // 데미지를 받은 상태 (애니메이션이 실행중인 상태)
            if (animator.GetBool(Constants.ANIM_PARAM_DMG))
            {
                // 애니메이션이 끝났다면 IDLE로 바꿔주고 Failure 반환
                // 
                damageTrigger += Time.deltaTime;
                if (damageTrigger >= Constants.DMG_ANIM_TIME)
                {
                    damageTrigger = 0f;
                    BTree.SetAnimatior(animator, Constants.ANIM_PARAM_IDLE);
                    return NodeState.Failure;
                }


                // 애니메이션이 진행중이라면 Success반환
                // -> Selector에서 다음 노드로 진행이 안되므로 Move가 안됨
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
            // 죽는 경우 애니메이션 실행 후 Success 반환
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
            // 죽었을때 Destroy로 삭제
            // TODO : 카드 Pooling 구현하고 나서 SetActive(false)로 교체
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
            // 필요한 초기화
            this.transform = transform;
            this.searchRange = searchRange;
            this.tagName = tagName;
        }

        public override NodeState Evaluate()
        {
            // 가장 가까운 적 찾기
            var target = (GameObject)GetNodeData(Constants.NDATA_TARGET);
            GameObject nearGo = null;

            if (target == null || target.CompareTag(Constants.TAG_DYING))
            {
                RemoveNodeData(Constants.NDATA_TRACK);
                RemoveNodeData(Constants.NDATA_TARGET);
                nearGo = BTree.SearchEnemy(transform, Physics2D.OverlapCircleAll(transform.position, searchRange), tagName);
            }
            else return NodeState.Failure;
            // 이미 적을 찾은 경우 탐색 성공


            // 근처에 적이 있으면 Target을 설정, Path 재설정을 해야한다고 알려줍니다.
            // Failure를 리턴하기 때문에 적이 사라진 후에 Move에서 Path를 재설정하고,
            // Track하느라 움직인 경로에서 다시 Path가 재설정됩니다.
            if (nearGo != null)
            {
                parent.parent.SetNodeData(Constants.NDATA_TARGET, nearGo);
                parent.SetNodeData(Constants.NDATA_PATH, true);
                return NodeState.Failure;
            }



            // 탐색 실패
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

            // 경로를 재설정하라는 DATA를 받으면 재설정하고 관련 변수를 초기화합니다.
            if (GetNodeData(Constants.NDATA_PATH) != null)
            {
                RemoveNodeData(Constants.NDATA_PATH); 
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)dest.y, (int)dest.x));
                currentPointIndex = 0;
            }

            // path가 없는 경우 바로 return
            if (path == null || path.Count == 0)
            {
                return NodeState.Success;
            }

            // 애니메이션 실행
            if (!animator.GetBool(Constants.ANIM_PARAM_WALK))
            {
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);
            }

            Vector2 currentTarget = path[currentPointIndex];
            var step = stat.MoveSpeed* new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));

            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);

            // 웨이포인트들을 따라가게 만들기 위해
            // 어느 거리 안으로 들어오면 다음 목적지를 웨이포인트를 향해 갑니다.
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


            // 공격 범위 확인
            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;
            if (dis2 > attackRange * attackRange)
            {
                return NodeState.Failure;
            }


            // 애니메이션 확인
            if (animator.GetBool(Constants.ANIM_PARAM_ATK))
            {
                return NodeState.Failure;
            }

            // 공격 쿨타임 확인
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


            // 성능을 위해 JPS를 매 프레임 호출하지 않습니다.
            // NDATA_TRACK이 설정되어있지 않으면 경로가 없으므로 경로를 생성합니다.
            if (GetNodeData(Constants.NDATA_TRACK) == null)
            {
                path = MapGenerator.Instance.PreprocessPath(new Vector2Int((int)transform.position.y, (int)transform.position.x),
            new Vector2Int((int)enemy.transform.position.y, (int)enemy.transform.position.x));
                currentPointIndex = 0;
                parent.parent.SetNodeData(Constants.NDATA_TRACK, true);
            }


            // Attack Range안에 들어온 경우입니다.
            // Success를 반환해서 Attack노드를 실행하도록 합니다.
            Vector3 dir = enemy.transform.position - transform.position;
            float dis2 = dir.x * dir.x + dir.y * dir.y;



            if (dis2 < attackRange * attackRange)
            {
                animator.SetBool(Constants.ANIM_PARAM_WALK, false);
                return NodeState.Failure;
            }



            // 도착했지만 적이 없는 경우이므로 NDATA_TRACK을 제거합니다.
            if (currentPointIndex >= path.Count)
            {
                RemoveNodeData(Constants.NDATA_TRACK);
                return NodeState.Failure;
            }

            


            // 물리 이동 및 애니메이션 구현입니다.
            Vector2 currentTarget = path[currentPointIndex];
            var step = stat.MoveSpeed * new Vector3(currentTarget.x - transform.position.x, currentTarget.y - transform.position.y, 0).normalized;
            rigid.MovePosition(transform.position + new Vector3(step.x, step.y, 0));
            animator.SetFloat("X", step.x);
            animator.SetFloat("Y", step.y);
            BTree.SetAnimatior(animator, Constants.ANIM_PARAM_WALK);


            // Move와 동일하게 웨이포인트 근처에서 웨이포인트를 바꿔줍니다.
            // Index의 범위는 위에서 확인하므로 여기서는 따로 확인하지 않습니다.
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
                // 타겟 확인
                var tr = (GameObject)GetNodeData(Constants.NDATA_TARGET);
                if (tr == null || tr.CompareTag(Constants.TAG_DYING))
                {
                    RemoveNodeData(Constants.NDATA_TARGET);
                    return NodeState.Failure;
                }

                // 때렸다는 flag를 띄우면 search 노드에서 쿨타임을 세줍니다
                parent.parent.parent.SetNodeData(Constants.NDATA_ATK, true);
                BTree.SetAnimatior(animator, Constants.ANIM_PARAM_ATK);


                // AtkRange에 따라 Slash/Bow 구분 가능
                if (tr.GetComponent<BTree>() != null && tr.GetComponent<BTree>().OnDamaged(stat.Attack, Define.AtkType.Slash))
                {
                    // 적을 때려서 죽였을 때 실행하는 부분입니다.
                    // 추가적인 기능을 구현할 수 있을 것 같아 남겨뒀습니다.
                    // ex. 적을 죽이면 일정 시간동안 데미지 상승
                }

                return NodeState.Success;
            }
            else return NodeState.Failure;

        }

    }

 
}