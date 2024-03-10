using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JSP
{
    public class JSP
    {
        const int COST_STRAIGHT = 10;
        const int COST_DIAGONAL = 14;

        private int[,] map;
        private Vector2Int startPoint;
        private Vector2Int endPoint;

        SortedSet<JSPNode> openList;

        private bool[,] closed;

        public JSP(int[,] map, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.map = map;
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            openList = new SortedSet<JSPNode>(new NodeComparer());
            closed = new bool[this.map.GetLength(0), this.map.GetLength(1)];

            for(int i=0; i<map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++) closed[i, j] = false;

        }

        public List<Vector2> PathFind()
        {
            // 초기화
            openList.Clear();

            openList.Add(new JSPNode(null, startPoint, JSPDir.None, 0, CalcHeuri(startPoint, endPoint)));

            while (openList.Count > 0)
            {
                JSPNode curNode = openList.First();
                openList.Remove(curNode);

                if(curNode.pos.x == endPoint.x && curNode.pos.y == endPoint.y)
                {
                    // 찾은 경우

                }

                // 가지치기
                switch (curNode.dir) {
                    case JSPDir.Up:
                        break;
                    case JSPDir.Down:
                        break;
                    case JSPDir.Left:
                        break;
                    case JSPDir.Right:
                        break;
                    case JSPDir.UpRight:
                        break;
                    case JSPDir.DownRight:
                        break;
                    case JSPDir.UpLeft:
                        break;
                    case JSPDir.DownLeft:
                        break;
                    default:
                        break;

                }

            }


            return null;
        }

        private void SearchStraight(Vector2Int pos, JSPDir dir)
        {
            switch (dir) {
                case JSPDir.Right:
                    for (int i = pos.x; i < map.GetLength(0); i++)
                    {

                    }
                    
                    break;
                
            }
        }

        private bool JSPCheck(Vector2Int pos, JSPDir dir){
            //BoundCheck
            if (pos.x < 0 || pos.y < 0 || pos.x >= map.GetLength(0) || pos.y >= map.GetLength(1)) return false;
            if (closed[pos.x, pos.y]) return false;

            // ForceNeighbor
            // TODO : 방향에 따라 Force이웃 찾기

            return false;
        }

        private int CalcHeuri(Vector2Int a, Vector2Int b)
        {
            int x = Mathf.Abs(a.x - b.x);
            int y = Mathf.Abs(a.y - b.y);


            return COST_STRAIGHT * Mathf.Abs(y - x) + COST_DIAGONAL * Mathf.Min(x, y);
        }

        private class NodeComparer : IComparer<JSPNode>
        {
            public int Compare(JSPNode x, JSPNode y)
            {
                var result = x.GetExpectedCost().CompareTo(y.GetExpectedCost());

                if (result == 0) return 1;
                return result;
            }
        }

    }

}
