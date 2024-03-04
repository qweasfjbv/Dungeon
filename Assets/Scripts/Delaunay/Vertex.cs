using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class Vertex
    { 
        public readonly int x;
        public readonly int y;

        public Vertex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator <(Vertex lv, Vertex rv)
        {
            return (lv.x < rv.x) || ((lv.x == rv.x) && (lv.y < rv.y));
        }

        public static bool operator >(Vertex lv, Vertex rv)
        {
            return (lv.x > rv.x) || ((lv.x == rv.x) && (lv.y > rv.y));
        }
    }

}
