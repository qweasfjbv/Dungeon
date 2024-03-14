using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace JPS
{
    public class JumpPointSearch
    {
        const int COST_STRAIGHT = 10;
        const int COST_DIAGONAL = 14;

        readonly private int[,] map;

        private Vector2Int startPoint;
        private Vector2Int endPoint;

        SortedSet<JPSNode> openList;
        List<JPSNode> closedList;

        public JumpPointSearch(int[,] map)
        {
            this.map = map;

            openList = new SortedSet<JPSNode>(new NodeComparer());
            closedList = new List<JPSNode>();
        }

        public List<Vector2Int> PathFind(Vector2Int sp, Vector2Int ep)
        {
            // 초기화

            this.startPoint = sp;
            this.endPoint = ep;

            if (map[endPoint.x, endPoint.y] == (int)Define.GridType.None || map[startPoint.x, startPoint.y] == (int)Define.GridType.None)
            {
                return new List<Vector2Int>();
            }

            List<Vector2Int> ans = new List<Vector2Int>();
            openList.Clear();
            closedList.Clear();

            AddToOpenList(new JPSNode(null, startPoint, JPSDir.None, 0, CalcHeuri(startPoint, endPoint)));

            while (openList.Count > 0)
            {
                EditorApplication.isPaused = true;

                JPSNode curNode = openList.First();
                closedList.Add(curNode);
                openList.Remove(curNode);

                if (curNode.pos.x == endPoint.x && curNode.pos.y == endPoint.y)
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
                    SearchLine(curNode, curNode.pos, JPSDir.Up);
                    SearchLine(curNode, curNode.pos, JPSDir.Right);
                    SearchLine(curNode, curNode.pos, JPSDir.Left);
                    SearchLine(curNode, curNode.pos, JPSDir.Down);
                    SearchLine(curNode, curNode.pos, JPSDir.UpRight);
                    SearchLine(curNode, curNode.pos, JPSDir.UpLeft);
                    SearchLine(curNode, curNode.pos, JPSDir.DownRight);
                    SearchLine(curNode, curNode.pos, JPSDir.DownLeft);

                }
                else
                    SearchLine(curNode, curNode.pos, curNode.dir);



            }


            return ans;
        }
        /// <summary>
        /// 현재 위치에서 한 방향으로 Jump Point 탐색
        /// </summary>
        /// <param name="parentNode">부모노드</param>
        /// <param name="pos">탐색 시작 위치. 보조탐색 시 부모노드의 위치와 다를 수 있음</param>
        /// <param name="dir">탐색 방향</param>
        private bool SearchLine(JPSNode parentNode, Vector2Int pos, JPSDir dir)
        {
            Vector2Int checkPoint;
            int check;
            JPSDir destDir;
            bool found = false;

            switch (dir)
            {
                case JPSDir.Right:
                    for (int i = pos.x + 1; i < map.GetLength(0); i++)
                    {
                        checkPoint = pos + new Vector2Int(i - pos.x, 0);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return false;
                    }
                    break;
                case JPSDir.Left:
                    for (int i = pos.x - 1; i >= 0; i--)
                    {
                        checkPoint = pos + new Vector2Int(i - pos.x, 0);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return false;
                    }
                    break;
                case JPSDir.Up:
                    for (int i = pos.y + 1; i < map.GetLength(1); i++)
                    {
                        checkPoint = pos + new Vector2Int(0, i - pos.y);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return false;
                    }
                    break;
                case JPSDir.Down:
                    for (int i = pos.y - 1; i >= 0; i--)
                    {
                        checkPoint = pos + new Vector2Int(0, i - pos.y);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);
                        if (check == 1)
                        {
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                            return true;
                        }
                        else if (check == -1) return false;
                    }
                    break;

                case JPSDir.UpRight:

                    SecondarySearch(parentNode, pos, JPSDir.Up, JPSDir.Right);
                    for (int i = 1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(i, i);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);


                        if (check == 1)
                        {
                            found = true;
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                        }
                        else if (check == 0)
                        {
                            JPSNode tmpNode = new JPSNode(parentNode, checkPoint, dir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            if (SecondarySearch(tmpNode, checkPoint, JPSDir.Up, JPSDir.Right))
                                closedList.Add(tmpNode);
                        }
                        else break;

                    }
                    return found;

                case JPSDir.UpLeft:

                    SecondarySearch(parentNode, pos, JPSDir.Up, JPSDir.Left);
                    for (int i = 1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(-i, i);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);


                        if (check == 1)
                        {
                            found = true;
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                        }
                        else if (check == 0)
                        {
                            JPSNode tmpNode = new JPSNode(parentNode, checkPoint, dir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            if (SecondarySearch(tmpNode, checkPoint, JPSDir.Up, JPSDir.Left))
                                closedList.Add(tmpNode);
                        }
                        else break;

                    }
                    return found;

                case JPSDir.DownRight:
                    SecondarySearch(parentNode, pos, JPSDir.Down, JPSDir.Right);
                    for (int i = 1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(i, -i);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);

                        if (check == 1)
                        {
                            found = true;
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                        }
                        else if (check == 0)
                        {
                            JPSNode tmpNode = new JPSNode(parentNode, checkPoint, dir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            if (SecondarySearch(tmpNode, checkPoint, JPSDir.Down, JPSDir.Right))
                                closedList.Add(tmpNode);
                        }
                        else break;

                    }
                    return found;
                case JPSDir.DownLeft:
                    SecondarySearch(parentNode, pos, JPSDir.Down, JPSDir.Left);
                    for (int i = 1; ; i++)
                    {
                        checkPoint = pos + new Vector2Int(-i, -i);
                        check = JPSCheck(parentNode, checkPoint, dir, out destDir);


                        if (check == 1)
                        {
                            found = true;
                            AddToOpenList(new JPSNode(parentNode, checkPoint, destDir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint)));
                        }
                        else if (check == 0)
                        {
                            JPSNode tmpNode = new JPSNode(parentNode, checkPoint, dir, parentNode.GetPassedCost() + CalcHeuri(parentNode.pos, checkPoint), CalcHeuri(checkPoint, endPoint));

                            if (SecondarySearch(tmpNode, checkPoint, JPSDir.Down, JPSDir.Left))
                                closedList.Add(tmpNode);
                        }
                        else break;

                    }
                    return found;
            }

            return false;
        }

        /// <summary>
        /// 보조탐색하면서 FOrcedNeighbor 있는지 확인하는 함수
        /// </summary>
        private bool SecondarySearch(JPSNode node, Vector2Int pos, JPSDir dir1, JPSDir dir2)
        {
            var search1 = SearchLine(node, pos, dir1);
            var search2 = SearchLine(node, pos, dir2);

            return search1 || search2;
        }

        /// <summary>
        /// 해당 노드 에서 ForceNeighbor 검사
        /// 범위를 벗어나면 -1, FN이 없으면 0, 있으면 1 리턴
        /// </summary>
        private int JPSCheck(JPSNode node, Vector2Int pos, JPSDir dir, out JPSDir destDir)
        {


            destDir = JPSDir.None;

            //if (checkedMap[pos.x, pos.y]) return 0;
            //else checkedMap[pos.y, pos.y] = true;


            //BoundCheck
            if (!IsMovable(pos)) return -1;

            if (pos.x == endPoint.x && pos.y == endPoint.y)
            {
                destDir = dir;
                AddToOpenList(new JPSNode(node, pos, dir, 0, 0));
                return 1;
            }

            // ForceNeighbors
            switch (dir)
            {
                case JPSDir.Right:
                    if (IsMovable(pos + new Vector2Int(1, 1)) && map[pos.x, pos.y + 1] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpRight;
                        return 1;
                    }
                    if (IsMovable(pos + new Vector2Int(1, -1)) && map[pos.x, pos.y - 1] == (int)Define.GridType.None)
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
                    if (IsMovable(pos + new Vector2Int(-1, 1)) && map[pos.x - 1, pos.y] == (int)Define.GridType.None)
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (IsMovable(pos + new Vector2Int(1, 1)) && map[pos.x + 1, pos.y] == (int)Define.GridType.None)
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
                    if (map[pos.x - 1, pos.y] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, 1)))
                    {
                        destDir = JPSDir.UpLeft;
                        return 1;
                    }
                    if (map[pos.x, pos.y - 1] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(1, -1)))
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
                    if (map[pos.x - 1, pos.y] == (int)Define.GridType.None && IsMovable(pos + new Vector2Int(-1, -1)))
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

            return 0;
        }

        private void AddToOpenList(JPSNode node)
        {
            if(openList.Where(n=> n.pos.x == node.pos.x && n.pos.y == node.pos.y).Count() > 0)
            {
                return;
            }

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
                if (result == 0) result = x.pos.x.CompareTo(y.pos.x);
                if (result == 0) result = x.pos.y.CompareTo(y.pos.y);

                return result;
            }
        }

    }

}
