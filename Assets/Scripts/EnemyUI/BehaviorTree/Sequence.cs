using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyUI.BehaviorTree {

    public class Sequence : Node
    {

        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool isChildRunning = false;
            foreach (Node child in children)
            {
                switch (child.Evaluate()) {
                    case NodeState.Fail:
                        state = NodeState.Fail;
                        return state;
                    case NodeState.Success:
                        continue;
                    case NodeState.Running:
                        isChildRunning = true;
                        return state;
                    default:
                        state = NodeState.Success;
                        return state;
                }
            }

            if (isChildRunning) state = NodeState.Running;
            else state = NodeState.Success;

            return state;
        }
    }

}
