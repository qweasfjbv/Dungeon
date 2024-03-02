using Delaunay;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Delaunay
{
    public class BowyerWatson : MonoBehaviour
    {
        // 모든 점을 포함하는 삼각형
        private static Triangle CalcSuperTriangle(IEnumerable<Vertex> vertices)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach(var v in vertices)
            {
                if(v.x < minX) minX = v.x;
                if(v.x > maxX) maxX = v.x;
                if(v.y < minY) minY = v.y;
                if(v.y > maxY) maxY = v.y;
            }

            int dx = (maxX - minX + 1);

            Vertex v1 = new Vertex(minX - dx - 1, minY - 1);
            Vertex v2 = new Vertex(minX + dx, maxY + (maxY - minY) + 1);
            Vertex v3 = new Vertex(maxX + dx + 1, minY - 1);

            return new Triangle(v1, v2, v3);
        }

        public static HashSet<Triangle> Triangulate(IEnumerable<Vertex> vertices)
        {
            Triangle superTriangle = CalcSuperTriangle(vertices);
            HashSet<Triangle> triangulation = new HashSet<Triangle>() { superTriangle };

            foreach (var vertex in vertices)
            {
                HashSet<Triangle> badTriangles = new HashSet<Triangle>();
                foreach (var triangle in triangulation)
                {
                    if (triangle.CircumCircleContains(vertex))
                        badTriangles.Add(triangle);
                }

                HashSet<Edge> polygon = new HashSet<Edge>();
                foreach (var badTriangle in badTriangles)
                {
                    foreach (var edge in badTriangle.edges)
                    {
                        bool isShared = false;
                        foreach (var otherTriangle in badTriangles)
                        {
                            if (badTriangle == otherTriangle)
                                continue;
                            if (otherTriangle.HasEdge(edge))
                                isShared = true;
                        }
                        if (!isShared)
                            polygon.Add(edge);
                    }
                }

                triangulation.ExceptWith(badTriangles);

                foreach (var edge in polygon)
                {
                    triangulation.Add(new Triangle(vertex, edge.a, edge.b));
                }
            }

            triangulation.RemoveWhere((Triangle t) => t.HasVertexFrom(superTriangle));

            return triangulation;
        }
    }

}