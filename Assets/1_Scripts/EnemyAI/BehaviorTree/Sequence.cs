using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyAI.BehaviorTree {

    public class Sequence : Node
    {

        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            foreach (Node child in children)
            {
                switch (child.Evaluate()) {
                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;
                    case NodeState.Success:
                        continue;
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                }
            }

            state = NodeState.Success;

            return state;
        }
    }

}
