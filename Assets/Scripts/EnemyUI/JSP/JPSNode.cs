using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JPS {

    public enum JPSDir
    {
        Up, Down, Left, Right,
        UpRight, DownRight, 
        UpLeft, DownLeft,
        None
    }

    public class JPSNode
    {
        public JPSNode parent;
        public Vector2Int pos;
        public JPSDir dir;
        private float cost;
        private float heuri;


        public JPSNode(JPSNode parent, Vector2Int pos, JPSDir dir, float cost, float huri)
        {
            this.parent = parent;
            this.pos = pos;
            this.dir = dir;
            this.cost = cost;
            this.heuri = huri;
        }

        public float GetExpectedCost()
        {
            return cost + heuri;
        }

    }

}
