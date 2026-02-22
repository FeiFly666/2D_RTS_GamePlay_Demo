using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFinding
{
    private Node[,] Grid;

    public Node[,] grid => Grid;

    public int MainWeight = 14;
    public int SecondaryWeight = 11;

    public int resolutionScale = 2;
    

    private int Width;
    private int Height;

    private Vector3Int GridOffset;
    public Vector3 cellSize;

    private Vector3 worldOrigin;

    //private GenericPriorityQueue<Node> OpenNodes;
    private FastPriorityQueue<Node> OpenNodes;
    private HashSet<Node> CloseNodes = new HashSet<Node>();
    private HashSet<Node> touchedNodes = new HashSet<Node>();

    public List<Node> CurrentPath = null;

    private const int NodesPerFrame = 1800;

    public PathFinding(int resolutionScale)
    {
        /* OpenNodes = new GenericPriorityQueue<Node>((a, b) => {
             int res = a.fCost.CompareTo(b.fCost);
             if (res == 0) res = a.hCost.CompareTo(b.hCost);
             return res;
         });*/
        OpenNodes = new FastPriorityQueue<Node>((a, b) =>
        {
            int res = a.fCost.CompareTo(b.fCost);
            if (res == 0) res = a.hCost.CompareTo(b.hCost);
            return res;
        });
        this.resolutionScale = resolutionScale;
        var bounds = TilemapManager.Instance.WalkableTilemap.cellBounds;

        Vector3 tilemapCelleSize = TilemapManager.Instance.WalkableTilemap.cellSize;

        GridOffset = bounds.min;
        worldOrigin = TilemapManager.Instance.WalkableTilemap.CellToWorld(GridOffset);

        Width = bounds.size.x * resolutionScale; 
        Height = bounds.size.y * resolutionScale;

        cellSize = new Vector3(tilemapCelleSize.x / resolutionScale, tilemapCelleSize.y / resolutionScale);

        Grid = new Node[Width, Height];

        InitGrid(GridOffset);
    }

    private void InitGrid(Vector3Int offset)
    {
        Vector3 cellsize = TilemapManager.Instance.WalkableTilemap.cellSize;

        for(int i = 0; i < Width; i++)
        {
            for(int j = 0; j < Height; j++)
            {
                int tileX = Mathf.FloorToInt(i / (float)resolutionScale) + offset.x;
                int tileY = Mathf.FloorToInt(j / (float)resolutionScale) + offset.y;

                float centerX = offset.x + (i + 0.5f) * cellSize.x;
                float centerY = offset.y + (j + 0.5f) * cellSize.y;

                Vector3 tilePos = new Vector3(centerX,centerY); 

                bool isWalkeable = TilemapManager.Instance.CanWalkAtTile(tilePos, this.cellSize);

                var node = new Node(i,j,centerX, centerY, isWalkeable); 

                Grid[i, j] = node;
            }
        }
        RefreshAreaIDs();

    }
    public void UpdateNodesInArea(Bounds bounds)
    {
        Node minNode = FindNode(bounds.min + new Vector3(0.01f, 0.01f, 0));
        Node maxNode = FindNode(bounds.max - new Vector3(0.01f, 0.01f, 0));

        if (minNode == null || maxNode == null) return;

        for(int i = minNode.GridX; i<= maxNode.GridX; i++)
        {
            for(int j  = minNode.GridY; j<= maxNode.GridY; j++)
            {
                Node node = Grid[i, j];

                node.IsWalkable = TilemapManager.Instance.CanWalkAtTile(new Vector3(node.CenterX,node.CenterY), this.cellSize);

            }
        }
    }
    public void UpdateAllNodes()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Node node = Grid[i, j];
                node.IsWalkable = TilemapManager.Instance.CanWalkAtTile(new Vector3(node.CenterX, node.CenterY), this.cellSize);
            }
        }
    }

    public byte[] GetNodeCheckStatus()
    {
        int Width = grid.GetLength(0);
        int Height = grid.GetLength(1);
        byte[] status = new byte[Width * Height];

        for(int i = 0;i< Width; i++)
            for(int j = 0; j<Height; j++)
            {
                status[j*Width + i] = (byte)(grid[i,j].isChecked ? 1 : 0);
            }
        return status;
    }
    public void LoadNodeCheckStatus(byte[] status)
    {
        int Width = grid.GetLength(0);
        int Height = grid.GetLength(1);
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
            {
                grid[i, j].isChecked = status[j * Width + i] == 1;
            }
    }

    public Node FindNode(Vector3 position)
    {
        float offsetX = position.x - worldOrigin.x;
        float offsetY = position.y - worldOrigin.y;

        int gridX = Mathf.FloorToInt(offsetX / cellSize.x);
        int gridY = Mathf.FloorToInt(offsetY / cellSize.y);

        if (gridX >= 0 && gridY >= 0 && gridX < Width && gridY < Height)
        {
            return Grid[gridX, gridY];
        }
        return null;

    }
    public Node FindNode(int x, int y)
    {
        if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1)) return null;
        return Grid[x, y];
    }
    public List<Node> FindPath(Vector3 startPosition, Vector3 endPosition, int maxIterations = 1500)
    {
        int iterations = 0;

        Node startNode = FindNode(startPosition);
        Node endNode = FindNode(endPosition); 

        if(startNode == null || endNode == null)
        {
            ResetNode();
            return null;
        }
        if(endNode != null && !endNode.IsWalkable)
        {
            ResetNode();
            return null;
        }

        //OpenNodes.Push(startNode);
        if (!startNode.isInOpen)
        {
            OpenNodes.Enqueue(startNode);
            startNode.isInOpen = true;
        }
        else
        {
            OpenNodes.UpdateItem(startNode);
        }

        while (OpenNodes.Count > 0)
        {
            iterations++;
            if(iterations > maxIterations)
            {
                ResetNode();
                CurrentPath = null;
                return null;
            }

            //Node currentNode = OpenNodes.Pop();
            Node currentNode = OpenNodes.Dequeue();
            currentNode.isInOpen = false;

            if(currentNode == endNode)
            {
                var path = RetracePath(startNode, endNode);
                ResetNode();
                CurrentPath = path;
                return path;
            }
            else
            {
                CloseNodes.Add(currentNode);

                List<Node> newNodes = FindGridNeighbors(currentNode);

                foreach(var node in newNodes)
                {
                    if(!CloseNodes.Contains(node))
                    {
                        int newGCost = CalculateDistanceBetweenTwoNodes(currentNode, node) + currentNode.gCost;

                        if(!node.isTouched || newGCost < node.gCost)
                        {
                            node.gCost = newGCost;

                            int distance = CalculateDistanceBetweenTwoNodes(node, endNode);
                            node.hCost = distance;
                            node.fCost = node.gCost + node.hCost;

                            node.parentNode = currentNode;

                            node.parentNode = currentNode;
                            if (!node.isTouched)
                            {
                                node.isTouched = true;
                                touchedNodes.Add(node); // 记下来，后面重置用
                            }

                            //OpenNodes.Push(node);
                            if (!node.isInOpen)
                            {
                                OpenNodes.Enqueue(node);
                                node.isInOpen = true;
                            }
                            else
                            {
                                OpenNodes.UpdateItem(node);
                            }

                        }

                    }
                }
            }
        }
        ResetNode();
        CurrentPath = null;
        return new List<Node>();
    }

    public IEnumerator FindPathCoroutine(Vector3 startPosition, Vector3 endPosition, System.Action<List<Node>, bool> callback)
    {

        Node startNode = FindNode(startPosition);
        Node endNode = FindNode(endPosition);

        bool pathSuccess = false;
        List<Node> finalPath = new List<Node>();

        int iterationsThisFrame = 0;

        if (startNode == null || endNode == null)
        {
            ResetNode();
            callback?.Invoke(null, pathSuccess);
            yield break;
        }
        if (endNode != null && !endNode.IsWalkable || startNode.AreaID != endNode.AreaID)
        {
            ResetNode();
            callback?.Invoke(null, pathSuccess);
            yield break;
        }

        //OpenNodes.Push(startNode);
        if (!startNode.isInOpen)
        {
            OpenNodes.Enqueue(startNode);
            startNode.isInOpen = true;
        }
        else
        {
            OpenNodes.UpdateItem(startNode);
        }

        while (OpenNodes.Count > 0)
        {
            Node currentNode = OpenNodes.Dequeue();
            currentNode.isInOpen = false;

            if (currentNode == endNode)
            {
                var path = RetracePath(startNode, endNode);
                ResetNode();
                CurrentPath = path;
                finalPath = path;
                pathSuccess = true;
                break;
            }
            else
            {
                CloseNodes.Add(currentNode);

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        int checkX = currentNode.GridX + i;
                        int checkY = currentNode.GridY + j;

                        if (checkX >= 0 && checkY >= 0 && checkX < Width && checkY < Height)
                        {
                            Node theNeighborNode = grid[checkX, checkY];

                            if (theNeighborNode.IsWalkable)
                            {
                                if (Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1)
                                {
                                    bool canWalkHorizontal = Grid[currentNode.GridX + i, currentNode.GridY].IsWalkable;
                                    bool canWalkVertical = Grid[currentNode.GridX, currentNode.GridY + j].IsWalkable;

                                    if (!canWalkHorizontal || !canWalkVertical)
                                    {
                                        continue;
                                    }
                                }
                                if (!CloseNodes.Contains(theNeighborNode))
                                {
                                    int newGCost = CalculateDistanceBetweenTwoNodes(currentNode, theNeighborNode) + currentNode.gCost;

                                    if (!theNeighborNode.isTouched || newGCost < theNeighborNode.gCost)
                                    {
                                        theNeighborNode.gCost = newGCost;

                                        int distance = CalculateDistanceBetweenTwoNodes(theNeighborNode, endNode);
                                        theNeighborNode.hCost = distance;
                                        theNeighborNode.fCost = theNeighborNode.gCost + theNeighborNode.hCost;

                                        theNeighborNode.parentNode = currentNode;
                                        if (!theNeighborNode.isTouched)
                                        {
                                            theNeighborNode.isTouched = true;
                                            touchedNodes.Add(theNeighborNode);
                                        }

                                        //OpenNodes.Push(theNeighborNode);
                                        if (!theNeighborNode.isInOpen)
                                        {
                                            OpenNodes.Enqueue(theNeighborNode);
                                            theNeighborNode.isInOpen = true;
                                        }
                                        else
                                        {
                                            OpenNodes.UpdateItem(theNeighborNode);
                                        }
                                    }

                                }

                            }
                        }
                    }
                }
                iterationsThisFrame++;
                if(iterationsThisFrame >= NodesPerFrame)
                {
                    iterationsThisFrame = 0;
                    yield return null;
                }
            }
        }
        ResetNode();
        callback?.Invoke(finalPath,pathSuccess);
    }
    private void ResetNode()
    {
        foreach (var node in touchedNodes)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.fCost = 0;
            node.isTouched = false;
            node.parentNode = null;
            node.HeapIndex = -1;
            node.isInOpen = false;
        }
        touchedNodes.Clear();
        OpenNodes.Clear();
        CloseNodes.Clear();
    }
    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            if(currentNode.parentNode != null)
            {
                currentNode = currentNode.parentNode;
            }
            else
            {
                break;
            }
        }
        path.Reverse();

        return path;
    }
    public void RefreshAreaIDs()
    {
        int currentAreaID = 1;
        foreach (var node in Grid) { node.AreaID = 0; }

        foreach (var node in Grid)
        {
            if (node.IsWalkable && node.AreaID == 0)
            {
                FillArea(node, currentAreaID);
                currentAreaID++;
            }
        }
    }

    private void FillArea(Node startNode, int id)
    {
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(startNode);
        startNode.AreaID = id;

        while (queue.Count > 0)
        {
            Node curr = queue.Dequeue();
            foreach (var neighbor in FindGridNeighbors(curr))
            {
                if (neighbor.IsWalkable && neighbor.AreaID == 0)
                {
                    neighbor.AreaID = id;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
    private List<Node> FindGridNeighbors(Node node)
    {
        List<Node> neighborNodes = new List<Node>();

        for(int i = -1; i<=1;i++)
        {
            for(int j = -1; j<=1;j++)
            {
                if (i == 0 && j == 0) continue;

                int checkX = node.GridX + i;
                int checkY = node.GridY + j;

                if (checkX >= 0 && checkY >= 0 && checkX < Width && checkY < Height)
                {
                    Node theNeighborNode = grid[checkX, checkY];

                    if(theNeighborNode.IsWalkable)
                    {
                        if (Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1)
                        {
                            bool canWalkHorizontal = Grid[node.GridX + i, node.GridY].IsWalkable;
                            bool canWalkVertical = Grid[node.GridX, node.GridY + j].IsWalkable;

                            if (!canWalkHorizontal || !canWalkVertical)
                            {
                                continue;
                            }
                        }
                        neighborNodes.Add(theNeighborNode);
                    }
                }
            }
        }

        return neighborNodes;

    }

    private int CalculateDistanceBetweenTwoNodes(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if(distanceX > distanceY)
        {
            return distanceY * MainWeight + (distanceX - distanceY) * SecondaryWeight;
        }
        else
        {
            return distanceX * MainWeight + (distanceY - distanceX) * SecondaryWeight;
        }
    }
    
}
