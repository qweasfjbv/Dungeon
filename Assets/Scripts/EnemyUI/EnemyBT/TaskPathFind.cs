using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public class TaskPathFind : Node
    {
        public TaskPathFind()
        {
            // 필요한 초기화
        }

        public override NodeState Evaluate()
        {
            // 움직이는 로직 구현

            return NodeState.Running;
        }
    }
}
