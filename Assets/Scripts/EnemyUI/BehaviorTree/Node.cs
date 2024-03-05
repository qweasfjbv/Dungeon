using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

namespace EnemyUI.BehaviorTree
{
    public enum NodeState { 
    Fail,
    Running,
    Success
    }

    public class Node
    {

        public NodeState state;
        public Node parent;
        protected List<Node> children= new List<Node>();

        private Dictionary<string, object> nodeData = new Dictionary<string, object>();

        public Node(){
            parent = null;
        }

        public Node(List<Node> children)
        {
            foreach (var child in children)
                Attach(child);
        }

        /// <summary>
        /// 해당 노드에 인자로 받은 노드를 Child로 입력
        /// </summary>
        private void Attach(Node child)
        {
            child.parent = this;
            children.Add(child);
        }

        
        public virtual NodeState Evaluate () => NodeState.Fail;

        public void SetNodeData(string key, object value)
        {
            nodeData[key] = value;
        }

        public object GetNodeData(string key)
        {
            object ret = null;

            if (nodeData.TryGetValue(key, out ret)) return ret;

            Node cur = this.parent;

            while (cur.parent != null)
            {
                ret = cur.GetNodeData(key);
                if (ret != null) return ret;
                cur = cur.parent;
            }

            return null;
        }

        public bool RemoveNodeData(string key)
        {
            if (nodeData.ContainsKey(key))
            {
                nodeData.Remove(key); return true;
            }

            Node cur = this.parent;

            while (cur.parent != null)
            {
                if (cur.RemoveNodeData(key)) return true;
                cur = cur.parent;
            }

            return false;
        }
    }

}
