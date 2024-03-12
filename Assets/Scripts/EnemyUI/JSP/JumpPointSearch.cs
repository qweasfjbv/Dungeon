using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace JPS
{
    public class JumpPointSearch
    {
        const int COST_STRAIGHT = 10;
        const int COST_DIAGONAL = 14;

        private int[,] map;
        private Vector2Int startPoint;
        private Vector2Int endPoint;

        SortedSet<JPSNode> openList;
        List<JPSNode> closedList;


        public JumpPointSearch(int[,] map, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.map = map;
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            openList = new SortedSet<JPSNode>(new NodeComparer());
            closedList = new List<JPSNode>();


        }

        public List<Vector2Int> PathFind()
        {
            // 초기화

            Debug.Log("PATHFIND : " + startPoint + " to " + endPoint);
            List<Vector2Int> ans = new List<Vector2Int>();
            openList.Clear();

            AddToOpenList(new JPSNode(null, startPoint, JPSDir.None, 0, CalcHeuri(startPoint, endPoint)));

            while(openList.Count > 0)
            {
                JPSNode curNode = openList.First();
                closedList.Add(curNode);
                openList.Remove(curNode);
                
                if(curNode.pos.x == endPoint.x && curNode.pos.y == endPoint.y)
                {
                    while (curNode != null)
                    {
                        ans.Add(new Vector2Int(curNode.pos.x, curNode.pos.y));
                        curNode = curNode.parent;
                    }

                    ans.Reverse();
                    return ans;
                }

                if (curNode.dir == JPSDir.None)
                {
                    Debug.Log("NoneCalled");
                    SearchLine(curNode, curNode.pos, JPSDir.Up);
                    SearchLine(curNode, curNode.pos, JPSDir.Right);
                    SearchLine(curNode, curNode.pos, JPSDir.Left);
                    SearchLine(curNode, curNode.pos, JPSDir.Down);
                    SearchLine(curNode, curNode.pos, JPSDir.UpRight);
                    SearchLine(curNode, curNode.pos, JPSDir.UpLeft);
                    SearchLine(curNode, curNode.pos, JPSDir.DownRight);
                    SearchLine(curNode, curNode.pos, JPSDir.DownLeft);

                    foreach (var opl in openList)
                        Debug.Log("OPENLIST : " + opl.pos);
                }
                else
                {
                    SearchLine(curNode, curNode.pos, curNode.dir);
                }

            }


            return ans;
        }

        /// <summary>
        /// 현재 위치에서 한 방향으로 Jump Point 탐색
        /// </summary>
        /// <param name="node">부모노드</param>
        /// <param name="pos">탐색 시작 위치. 보조탐색 시 부모노드의 위치와 다를 수 있음</param>
        /// <param name="dir">탐색 방향</param>
        private bool SearchLine(JPSNode node, Vector2Int pos, JPSDir dir)
        {
            Vector2Int checkPoint;
            int check;
            JPSDir destDir;
            bool isTrueOnce = false;

            switch (dir) {
                case JPSDir.Right:
                    for (int i = pos.x+1; i < map.GetLength(0); i++)
                    {
                        checkPoint = pos + new Vector2Int(i - pos.x, 0);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return isTrueOnce;
                    }
                    break;
                case JPSDir.Left:
                    for (int i = pos.x - 1; i>= 0; i--)
                    {
                        checkPoint = pos + new Vector2Int(i -pos.x, 0);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node,checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return isTrueOnce;
                    }
                    break;
                case JPSDir.Up:
                    for (int i = pos.y + 1; i < map.GetLength(1); i++)
                    {
                        checkPoint = pos + new Vector2Int(0, i - pos.y);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node,checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return isTrueOnce;
                    }
                    break;
                case JPSDir.Down:
                    for (int i = pos.y - 1; i >= 0; i--)
                    {
                        checkPoint = pos + new Vector2Int(0, i - pos.y);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return isTrueOnce;
                    }
                    break;

                case JPSDir.UpRight:
                    for(int i=1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(i, i);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return isTrueOnce;
                        }
                        else if (check == 0)
                        {
                            // TODO : 한칸 가고 양옆을 확인안하는 경우가 있음
                            // 한 칸 가더라도 양옆 확인해야함.
                            //가지치기 확실하게
                            bool tmp = false;
                            JPSNode tmpNode = new JPSNode(node, checkPoint, JPSDir.UpRight, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint));
                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Up);
                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Right);

                            if (tmp)
                            {
                                //Debug.Log("upright" + tmpNode.pos);
                                closedList.Add(tmpNode);
                            }
                        }
                        else return isTrueOnce;

                    }
                    break;
                case JPSDir.UpLeft:
                    for (int i = 1;; i++)
                    {
                        checkPoint = pos + new Vector2Int(-i, i);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return isTrueOnce;
                        }
                        else if (check == 0)
                        {
                            bool tmp = false;
                            JPSNode tmpNode = new JPSNode(node, checkPoint, JPSDir.UpLeft, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint));
                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Up);
                            tmp =  tmp || SearchLine(tmpNode, checkPoint, JPSDir.Left);

                            if (tmp)
                            {
                                //Debug.Log("upleft" + node.pos + ", " + checkPoint);
                                closedList.Add(tmpNode);
                            }
                        }
                        else return isTrueOnce;

                    }
                    break;
                case JPSDir.DownRight:
                    for (int i = 1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(i, -i);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return isTrueOnce;
                        }
                        else if (check == 0)
                        {
                            bool tmp = false;
                            JPSNode tmpNode = new JPSNode(node, checkPoint, JPSDir.DownRight, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Down);
                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Right);

                            if (tmp)
                            {

                                //Debug.Log("downright" + tmpNode.pos);
                                closedList.Add(tmpNode);
                            }
                        }
                        else return isTrueOnce;

                    }
                    break;
                case JPSDir.DownLeft:
                    for (int i = 1;; i++)
                    {
                        checkPoint = pos + new Vector2Int(-i, -i);
                        check = JPSCheck(checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            isTrueOnce = true;
                            AddToOpenList(new JPSNode(node, checkPoint, destDir, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return isTrueOnce;
                        }
                        else if (check == 0)
                        {
                            bool tmp = false;
                            JPSNode tmpNode = new JPSNode(node, checkPoint, JPSDir.DownLeft, node.GetExpectedCost() + CalcHeuri(node.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Down);
                            tmp = tmp || SearchLine(tmpNode, checkPoint, JPSDir.Left);
                            
                            if (tmp)
                            {
                                //Debug.Log("downleft" + tmpNode.pos);
                                closedList.Add(tmpNode);
                            }
                        }
                        else return isTrueOnce;

                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 해당 노드 에서 ForceNeighbor 검사
        /// 범위를 벗어나면 -1, FN이 없으면 0, 있으면 1 리턴
        /// </summary>
        private int JPSCheck(Vector2Int pos, JPSDir dir, out JPSDir destDir){

            destDir = JPSDir.None;

            //BoundCheck
            if (!IsMovable(pos)) return -1;

            if (pos.x == endPoint.x && pos.y == endPoint.y)
            {
                destDir = dir;
                return 1;
            } 

            // ForceNeighbors
            switch (dir)
            {
                case JPSDir.Right:
                    if (IsMovable(pos + new Vector2Int(1, 1)) && map[pos.x, pos.y+1] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpRight;
                        return 1;
                    }
                    if(IsMovable(pos + new Vector2Int(1, -1)) && map[pos.x, pos.y - 1] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.DownRight;
                        return 1;
                    }
                    break;

                case JPSDir.Left:
                    if (IsMovable(pos + new Vector2Int(-1, 1)) && map[pos.x, pos.y + 1] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (IsMovable(pos + new Vector2Int(-1, -1)) && map[pos.x, pos.y - 1] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.DownLeft;
                        return 1;
                    }
                    break;
                case JPSDir.Up:
                    if (IsMovable(pos + new Vector2Int(-1, 1)) && map[pos.x-1, pos.y] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (IsMovable(pos + new Vector2Int(1, 1)) && map[pos.x+1, pos.y] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpRight;
                        return 1;
                    }
                    break;
                case JPSDir.Down:
                    if (IsMovable(pos + new Vector2Int(-1, -1)) && map[pos.x - 1, pos.y] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.DownLeft;
                        return 1;
                    }
                    if (IsMovable(pos + new Vector2Int(1, -1)) && map[pos.x + 1, pos.y] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.DownRight;
                        return 1;
                    }
                    break;

                case JPSDir.UpRight:
                    if (map[pos.x - 1, pos.y]==(int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, 1)))
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (map[pos.x , pos.y-1] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(1, -1)))
                    {
                        destDir = JPSDir.DownRight;
                        return 1;
                    }
                    break;
                case JPSDir.UpLeft:
                    if (map[pos.x + 1, pos.y] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(1, 1)))
                    {
                        destDir = JPSDir.UpRight;
                        return 1;
                    }
                    if (map[pos.x, pos.y - 1] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, -1)))
                    {
                        destDir = JPSDir.DownLeft;
                        return 1;
                    }
                    break;

                case JPSDir.DownRight:
                    if (map[pos.x, pos.y + 1] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(1, 1)))
                    {
                        destDir = JPSDir.UpRight;
                        return 1;
                    }
                    if (map[pos.x -1 , pos.y] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, -1)))
                    {
                        destDir = JPSDir.DownLeft;
                        return 1;
                    }
                    break;

                case JPSDir.DownLeft:
                    if (map[pos.x, pos.y + 1] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, 1)))
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (map[pos.x + 1, pos.y] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(1, -1)))
                    {
                        destDir = JPSDir.DownRight;
                        return 1;
                    }
                    break;


            }

            //Debug.Log("checkpos : " + pos.x + ", " + pos.y);
            return 0;
        }

        private void AddToOpenList(JPSNode node)
        {
            openList.Add(node);
        }

        private bool IsMovable(Vector2Int pos)
        {

            if (pos.x < 0 || pos.y < 0 || pos.x >= map.GetLength(0) || pos.y >= map.GetLength(1)) return false;
            if (map[pos.x, pos.y] == (int)Define.GridType.None) return false;


            return true;
        }

        private int CalcHeuri(Vector2Int a, Vector2Int b)
        {
            int x = Mathf.Abs(a.x - b.x);
            int y = Mathf.Abs(a.y - b.y);


            return COST_STRAIGHT * Mathf.Abs(y - x) + COST_DIAGONAL * Mathf.Min(x, y);
        }

        private class NodeComparer : IComparer<JPSNode>
        {
            public int Compare(JPSNode x, JPSNode y)
            {
                var result = x.GetExpectedCost().CompareTo(y.GetExpectedCost());

                return result;
            }
        }

    }

}
