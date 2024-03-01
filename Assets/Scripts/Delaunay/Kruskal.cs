using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;


namespace Delaunay
{
    public static class Kruskal
    {
        public static List<Edge> MinimumSpanningTree(IEnumerable<Edge> graph)
        {
            List<Edge> ans = new List<Edge>();

            List<Edge> edges = new List<Edge>(graph);
            edges.Sort(Edge.LengthCompare);

            HashSet<Vertex> points = new HashSet<Vertex>();
            foreach (var edge in edges)
            {
                points.Add(edge.a);
                points.Add(edge.b);
            }

            Dictionary<Vertex, Vertex> parents = new Dictionary<Vertex, Vertex>();
            foreach (var point in points)
                parents[point] = point;

            Vertex UnionFind(Vertex x)
            {
                if (parents[x] != x)
                    parents[x] = UnionFind(parents[x]);
                return parents[x];
            }

            foreach (var edge in edges)
            {
                var x = UnionFind(edge.a);
                var y = UnionFind(edge.b);
                if (x != y)
                {
                    ans.Add(edge);
                    parents[x] = y;
                }
                // 랜덤한 엣지 추가. 항상 선형의 던전을 생성하는건 지루함.
                else if (Random.Range(0, 6) == 0)
                {
                    ans.Add(edge);
                }
            }

            return ans;
        }
    }
}