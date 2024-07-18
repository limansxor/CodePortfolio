using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace NGle.Solitaire.RunGame
{
    public class PathFindLogic
    {
        public class Node
        {
            // COST 와 PrevCount 는 분리하는게 맞다 가는 비용 하고 최단 거리 인지는 분리해야 한다.
            private int _cost;
            public int cost { get { return _cost; } }
            private int _turnCount;
            public int turnCount { get { return _turnCount; } }
            private Vector2Int _myIndex;
            public Vector2Int myIndex { get { return _myIndex; } }

            private Node _prevNode;
            public Node prevNode { get { return _prevNode; } }
            private int _prevCount;
            public int prevCount { get { return _prevCount; } }

            public Node(Vector2Int index, Node prevNode)
            {
                _myIndex = index;

                _prevNode = prevNode;

                if (_prevNode == null)
                {
                    // 이전 노드가 없으면 시작지점이기 때문에 Count는 0이다.
                    _prevCount = 0;
                }
                else
                {
                    // 이전 노드가 있다면 이전노드의 '이전노드 갯수' + 1을 해준다.
                    // 목표지점에 해당하는 노드는 최종적으로
                    // 시작지점에서 목표지점까지의 노드 수가 담기게 된다.
                    _prevCount = _prevNode.prevCount + 1;
                    _cost = prevNode.cost + 1;
                }
            }

            public void SetCost(int cost)
            {
                if (_prevNode != null)
                {
                    _cost = cost + _prevNode.cost;
                }
                else
                {
                    _cost = 0;
                }

            }

            public void AddTurnCount()
            {
                if (_prevNode != null)
                {
                    _turnCount = _prevNode.turnCount + 1;
                }
                else
                {
                    _turnCount = 0;
                }
            }

        }

        public class Result
        {
            private int _turnCount;
            public int turnCount { get { return _turnCount; } }

            private List<Vector2Int> _paths;
            public List<Vector2Int> paths { get { return _paths; } }

            public Result(int turnCount, List<Vector2Int> paths)
            {
                _turnCount = turnCount;

                _paths = paths;
            }

        }


        private bool[,] _checkRoad = null;
        public bool[,] checkRoad { get { return _checkRoad; } }

        private Node _bestNode = null;
        public Node bestNode { get { return _bestNode; } }

        private int[,] direction = new int[,]
    {

        { -1,  0 },
        {  1,  0 },
        {  0, -1 },
        {  0,  1 }


    };

        private List<int[,]> directions;

        public PathFindLogic()
        {
            directions = new List<int[,]>();
        }

        private void ClearCheckRoad(int widthCount, int hightCount)
        {
            _checkRoad = new bool[widthCount, hightCount];
            for (int i = 0; i < widthCount; ++i)
            {
                for (int j = 0; j < hightCount; ++j)
                    checkRoad[i, j] = false;
            }
        }

        private bool Turned(Node current, Node next)
        {
            if (current.prevNode != null)
            {
                Node prev = current.prevNode;
                if (prev.myIndex.x == current.myIndex.x && current.myIndex.x == next.myIndex.x)
                {
                    return false;
                }
                else if (prev.myIndex.y == current.myIndex.y && current.myIndex.y == next.myIndex.y)
                {
                    return false;
                }

                else
                {
                    return true;
                }
            }
            else
            {
                //  Debug.Log("첫번째 노드 입니다 꺾이지 않은 것으로 판단 );
                return false;
            }
        }

        private bool CheckMapWay(int[,] blockInfos, Vector2Int index, Vector2Int targetIndex)
        {

            if (blockInfos[index.x, index.y] != -1)
            {
                if (index.x == targetIndex.x && index.y == targetIndex.y) return true;
                return false;
            }
            return true;
        }

        public Result BFS(int[,] blockInfos, Vector2Int startIndex, Vector2Int targetIndex)
        {
            ClearCheckRoad(blockInfos.GetLength(0), blockInfos.GetLength(1));

            _bestNode = null;

            List<Node> list = new List<Node>();
            list.Add(new Node(startIndex, null));
            checkRoad[startIndex.x, startIndex.y] = true;

            while (list.Count > 0)
            {
                Node node = list[0];
                list.RemoveAt(0);

                checkRoad[node.myIndex.x, node.myIndex.y] = true;
                //  Debug.Log("탐색중인 인덱스 " + node.myIndex  +"  Cost" + node.Cost);

                if (node.myIndex.x == targetIndex.x && node.myIndex.y == targetIndex.y)
                {
                    if (_bestNode == null || _bestNode.prevCount > node.prevCount) // 새로운 노드의 수가 기존 노브 보다 적게 움직였다면 // || 
                    {
                        _bestNode = node;

                        break;
                    }
                }


                for (int i = 0; i < direction.GetLength(0); i++)
                {
                    // 현재 노드 위치에서 +좌 우 상 하 
                    int dx = node.myIndex.x + direction[i, 0];
                    int dy = node.myIndex.y + direction[i, 1];

                    Vector2Int nextIndex = new Vector2Int(dx, dy);

                    Node searchNode = new Node(nextIndex, node);

                    if (CheckRange(nextIndex, blockInfos.GetLength(0), blockInfos.GetLength(1)) && CheckMapWay(blockInfos, nextIndex, targetIndex) && !checkRoad[dx, dy])
                    {
                        // checkRoad[dx, dy] = true;

                        if (Turned(node, searchNode))
                        {
                            searchNode.AddTurnCount();
                            if (searchNode.turnCount > 2)
                            {
                                continue;
                            }

                            searchNode.SetCost(100); // 꺾기는 것에 비용을 많이 지불 

                            list.Add(searchNode);
                        }

                        else
                        {
                            list.Add(searchNode);
                        }
                    }

                }


                List<Node> sortList = new List<Node>();
                while (list.Count > 0)
                {
                    int min = int.MaxValue;
                    int itemSel = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (min > list[i].cost)
                        {
                            min = list[i].cost;
                            itemSel = i;
                        }
                    }
                    sortList.Add(list[itemSel]);
                    list.RemoveAt(itemSel);
                }
                list = sortList;

            }

            int turnCount = -1;

            List<Vector2Int> paths = new List<Vector2Int>();

            if (bestNode == null)
            {
                //   Debug.Log("없어 질 수 없습니. ");
                turnCount = 999;
            }
            else
            {
                turnCount = 0;

                while (bestNode.prevCount > 0)
                {
                    //     Debug.LogFormat(" -> [{0},{1}] {2}\t", bestNode.myIndex.x, bestNode.myIndex.y, bestNode.Cost);

                    if (Turned(_bestNode.prevNode, _bestNode)) turnCount++;

                    paths.Add(new Vector2Int(_bestNode.myIndex.x, _bestNode.myIndex.y));
                    _bestNode = _bestNode.prevNode;
                }

                paths.Add(startIndex);  // 시작점 까지 포함 해야 한다.
                paths.Reverse(); // 돌려야 처음 찍은 지점 부터 표시 할 수 있다.

            }

            return new Result(turnCount, paths);
        }

        private bool CheckRange(Vector2Int index, int widthCount, int hightCount)
        {
            return (0 <= index.x && index.x < widthCount) &&
                (0 <= index.y && index.y < hightCount);
        }
    }
}