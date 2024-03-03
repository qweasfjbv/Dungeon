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

        public override bool Equals(object obj)
        {
            return obj is Vertex v && v.x == x && v.y == y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override string ToString()
        {
            return $"Vertex({this.x}, {this.y})";
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
