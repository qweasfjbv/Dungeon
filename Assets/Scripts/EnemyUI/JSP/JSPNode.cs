using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSP {

    public enum JSPDir
    {
        Up, Down, Left, Right,
        UpRight, DownRight, 
        UpLeft, DownLeft,
        None
    }

    public class JSPNode
    {
        public JSPNode parent;
        public Vector2 pos;
        public JSPDir dir;
        private float cost;
        private float heuri;


        public JSPNode(JSPNode parent, Vector2 pos, JSPDir dir, float cost, float huri)
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
