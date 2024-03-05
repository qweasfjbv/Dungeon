using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public class EnemyBT : Tree
    {
        public override Node SetupRoot()
        {
            Node root = new TaskPathFind();

            return root;
        }
    }

}