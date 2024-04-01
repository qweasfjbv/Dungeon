using Delaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloydWarshall
{

    public static (Vertex, Vertex) GetEntrance(HashSet<Delaunay.Vertex> points, List<Edge> edges)
    {
        int cnt = points.Count;

        int[,] dist = new int[cnt, cnt];


        for (int i = 0; i < cnt; i++)
        {
            for (int j = 0; j < cnt; j++)
            {
                dist[i, j] = (i == j) ? 0 : 1000000;
            }
        }

        // 간선 정보로 초기화
        foreach (Edge edge in edges)
        {
            int s = points.ToList().IndexOf(edge.a);
            int t = points.ToList().IndexOf(edge.b);
            dist[s, t] = dist[t, s] = (int)Mathf.Sqrt(Mathf.Pow(edge.a.x - edge.b.x, 2) + Mathf.Pow(edge.a.y - edge.b.y, 2));
        }

        // 플로이드 워셜 알고리즘
        for (int k = 0; k < cnt; k++)
        {
            for (int i = 0; i < cnt; i++)
            {
                for (int j = 0; j < cnt; j++)
                {
                    dist[i, j] = Mathf.Min(dist[i, j], dist[i, k] + dist[k, j]);
                }
            }
        }

        // 가장 거리가 먼 두 정점 찾기
        int maxDist = 0;
        int u = 0, v = 0;
        for (int i = 0; i < cnt; i++)
        {
            for (int j = i + 1; j < cnt; j++)
            {
                Debug.Log("point : " + points.ToList()[i] + ", " + points.ToList()[j] + " : " + dist[i, j]);
                if (dist[i, j] > maxDist)
                {
                    maxDist = dist[i, j];
                    u = i;
                    v = j;
                }
            }
        }

        return (points.ToList()[u], points.ToList()[v]);
    }


}
