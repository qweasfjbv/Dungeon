using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {

            foreach (Node child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Fail:
                        state = NodeState.Fail;
                        return state;
                    case NodeState.Success:
                        continue;
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.Fail;
            return state;
        }
    }

}