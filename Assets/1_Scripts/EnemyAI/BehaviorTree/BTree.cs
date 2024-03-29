using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

namespace EnemyUI.BehaviorTree
{
    public abstract class BTree : MonoBehaviour
    {

        private Node root = null;

        private void OnEnable()
        {
            root = SetupRoot();
        }
        private void Update()
        {
            if (root != null) root.Evaluate();
        }


        public abstract Node SetupRoot();

    }

}