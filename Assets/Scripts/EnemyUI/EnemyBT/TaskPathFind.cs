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
            // �ʿ��� �ʱ�ȭ
        }

        public override NodeState Evaluate()
        {
            // �����̴� ���� ����

            return NodeState.Running;
        }
    }
}
