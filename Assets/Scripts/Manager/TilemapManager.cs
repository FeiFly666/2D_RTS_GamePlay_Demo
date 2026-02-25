using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static CommonUtils;
using UnityEditor.PackageManager.Requests;

public class TilemapManager : MonoSingleton<TilemapManager>
{
    public Tilemap WalkableTilemap;
    public Tilemap PlacementTilemap;
    public Tilemap UnreachaableTilemap;
    public Tilemap boundTilemap;

    public Tilemap BuildinfAreaTilemap;

    public Tilemap[] SideBuildingArea;
    private Vector3Int buildingOffset;
    [Header("建造层瓦片")]
    public TileBase buildBaseTile;
    [Header("寻路网格缩放（基于tilemap）")]
    [Tooltip("寻路网格相对大小（用于寻路）")]
    public int resolutionScale;
    [Tooltip("BFS最大搜索格数（用于确定终点）")]
    public int maxSearch = 1000;

    public bool isLoading = false;

    private bool isNeedRefresh = false;
    private float nextRefreshTime;
    private PathFinding PathFinding;
    public PathShare pathShare;
    private Dictionary<BuildingUnit, List<Vector2Int>> cachebuilidngArea = new Dictionary<BuildingUnit, List<Vector2Int>>();
    private Dictionary<UnitSide, int[,]> buildingAreaCounter = new Dictionary<UnitSide, int[,]>();

    protected override void OnStart()
    {
        base.OnStart();
        InitPathfing();
        InitBuildingCounter();

        pathShare = new PathShare();

        GameObject go = new GameObject("PathFindingController");
        go.transform.parent = transform;
        go.AddComponent<PathRequestController>();

    }
    public PathFinding GetPathFinding()
    {
        return PathFinding;
    }
    private void InitBuildingCounter()
    {
        int sideNum = SideBuildingArea.Length;

        var bounds = this.WalkableTilemap.cellBounds;

        int margin = 10;

        int width = bounds.size.x + margin * 2;
        int height = bounds.size.y + margin * 2;

        buildingOffset = new Vector3Int(-bounds.xMin + margin, -bounds.yMin + margin);

        for (int i = 0; i < sideNum; i++)
        {
            buildingAreaCounter[(UnitSide)i] = new int[width, height];
        }
    }
    public void InitPathfing()
    {
        PathFinding = new PathFinding(resolutionScale);
    }
    public void NotifyingBuildingChanged(Bounds buildingBounds)
    {
        Debug.Log($"建筑建造，网格更新 更新范围：{buildingBounds.min.x} - {buildingBounds.max.x}  {buildingBounds.min.y} - {buildingBounds.max.y}");
        StartCoroutine(UpdateNodesInArea(buildingBounds));
        foreach (var unit in GameManager.Instance.liveHumanUnits)
        {
            if (unit != null) unit.ClearLocalCache();
        }
    }
    IEnumerator UpdateNodesInArea(Bounds bounds)
    {

        yield return new WaitForSeconds(0.01f);

        PathFinding.UpdateNodesInArea(bounds);

        

        //PathFinding.RefreshAreaIDs();
        PathFinding.FastRepairAreaAfterBuildingDeath(bounds);
        //isNeedRefresh = true;
    }
    public void RefreshAreaIDS() => PathFinding.RefreshAreaIDs();
    public void UpdateAllNodes()
    {
        PathFinding.UpdateAllNodes();
        PathFinding.RefreshAreaIDs();
    }

    public void FindPathFrameByFrame(Vector3 startPosition, Vector3 endPosition)
    {
        StartCoroutine(PathFinding.FindPathCoroutine(startPosition, endPosition, OnPathFindingFinished));
    }
    private void OnPathFindingFinished(List<Node> path, bool success)
    {
        PathRequestController.Instance.FinishedProcessing(path, success);
    }
    public Node FindNode(Vector3 position) => PathFinding.FindNode(position);

    public Node FindNode(int x, int y) => PathFinding.FindNode(x, y);

    public void AddBuildingArea(Vector2Int buildingCenter, Vector3Int buildingSize, UnitSide side, int delta, BuildingUnit builiding)//未验证
    {
        int extendX = (buildingSize.x - 1) * 3;
        int extendY = (buildingSize.y - 1) * 3;
        int extend = Mathf.Min(extendX, extendY);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int startPos = buildingCenter;
        queue.Enqueue(startPos);
        visited.Add(startPos);

        if (delta < 0)
        {
            if (cachebuilidngArea.TryGetValue(builiding, out List<Vector2Int> Area))
            {
                foreach (var pos in Area)
                {
                    int ix = pos.x + buildingOffset.x;
                    int iy = pos.y + buildingOffset.y;
                    UpdateSideTilemap(side, (Vector3Int)pos, ix, iy, delta);
                }
                return;
            }

        }
        cachebuilidngArea[builiding] = new List<Vector2Int>();
        cachebuilidngArea[builiding].Add(startPos);

        int width = buildingAreaCounter[side].GetLength(0);
        int height = buildingAreaCounter[side].GetLength(1);

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            Vector3Int tilePos = new Vector3Int(currentPos.x, currentPos.y, 0);

            if (!WalkableTilemap.HasTile(tilePos)) continue;
            if (UnreachaableTilemap.HasTile(tilePos)) continue;

            if (Mathf.Abs(currentPos.x - buildingCenter.x) > extend || Mathf.Abs(currentPos.y - buildingCenter.y) > extend)
                continue;

            int ix = currentPos.x + buildingOffset.x;
            int iy = currentPos.y + buildingOffset.y;
            if (ix >= 0 && ix < width && iy >= 0 && iy < height)
            {
                UpdateSideTilemap(side, tilePos, ix, iy, delta);
            }

            Vector2Int[] neighbors = {
            new Vector2Int(currentPos.x + 1, currentPos.y),
            new Vector2Int(currentPos.x - 1, currentPos.y),
            new Vector2Int(currentPos.x, currentPos.y + 1),
            new Vector2Int(currentPos.x, currentPos.y - 1)
        };

            foreach (var next in neighbors)
            {
                if (!visited.Contains(next))
                {
                    if (visited.Contains(next))
                        continue;

                    if (Mathf.Abs(next.x - buildingCenter.x) > extend ||
                        Mathf.Abs(next.y - buildingCenter.y) > extend)
                        continue;

                    Vector3Int nextTile = new Vector3Int(next.x, next.y, 0);

                    if (!WalkableTilemap.HasTile(nextTile))
                        continue;

                    if (UnreachaableTilemap.HasTile(nextTile))
                        continue;

                    visited.Add(next);
                    queue.Enqueue(next);
                    cachebuilidngArea[builiding].Add(next);
                }
            }
        }

    }

    private void UpdateSideTilemap(UnitSide side, Vector3Int pos, int arryX, int arryY, int x)
    {
        int[,] counter = buildingAreaCounter[side];
        Tilemap Tm = SideBuildingArea[(int)side];

        int oldCount = counter[arryX, arryY];
        counter[arryX, arryY] = Mathf.Max(oldCount + x, 0);
        int newCount = counter[arryX, arryY];

        if (oldCount == 0 && newCount > 0)
        {
            Tm.SetTile(pos, buildBaseTile);
        }
        else if (oldCount > 0 && newCount == 0)
        {
            Tm.SetTile(pos, null);
        }
    }
    //BFS搜索
    public Node FindNearestAvailableNode(Vector3 targetWorldPos, GameObject requester, bool checkNeighbors)
    {
        Node centerNode = PathFinding.FindNode(targetWorldPos);

        if (centerNode == null) { return null; }
        if (IsNodeAndNeighborsFree(centerNode, requester, checkNeighbors)) return centerNode;

        Node requesterNode = PathFinding.FindNode(requester.transform.position);
        if (requesterNode == null) return null;
        int allowedAreaID = requesterNode.AreaID;

        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(centerNode);
        visited.Add(centerNode);

        //float initialDistToTarget = Vector3.Distance(requester.transform.position, targetWorldPos);

        HumanUnit human = requester.GetComponent<HumanUnit>();

        while (queue.Count > 0)
        {
            List<Node> candidates = new List<Node>();
            Queue<Node> nextQueue = new Queue<Node>();

            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();

                if (checkNeighbors)
                {
                    if (!currentNode.IsWalkable) { continue; }
                }
                if (currentNode.IsWalkable && currentNode.AreaID == allowedAreaID && IsNodeAndNeighborsFree(currentNode, requester, checkNeighbors))
                {
                    if (!checkNeighbors)
                    {
                        float attackRange = human.attackRadius;

                        if (!IsInRange(currentNode.GetNodePosition(), targetWorldPos, attackRange))
                            continue;
                    }
                    candidates.Add(currentNode);
                }
                foreach (Node neighbor in GetAllNeighbors(currentNode))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        nextQueue.Enqueue(neighbor);
                    }
                }
            }
            if (candidates.Count > 0)
            {
                Node bestNode = null;
                float bestScore = float.MaxValue;

                foreach (var node in candidates)
                {
                    float disToSelf = Vector3.Distance(node.GetNodePosition(), requester.transform.position);
                    float attackRange = human.attackRadius;
                    float disToTarget = Vector3.Distance(node.GetNodePosition(), targetWorldPos);

                    float rangeBias = Mathf.Abs(disToTarget - attackRange);
                    float currentScore = 0;
                    if (checkNeighbors)
                    {
                        currentScore = disToSelf;
                    }
                    else
                    {
                        currentScore = disToSelf * 0.3f + rangeBias * 1.0f;
                        float perturbation = (requester.GetInstanceID() % 10) * 0.05f;

                        currentScore += perturbation;
                    }
                    if (currentScore < bestScore + 0.2f)
                    {
                        bestScore = currentScore;
                        bestNode = node;
                    }

                }
                if (bestNode != null)
                    return bestNode; // 返回这一层里最适合单位的格子
            }
            queue = nextQueue;
        }
        return null;
    }

    private List<Node> GetAllNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int newX = node.GridX + x;
                int newY = node.GridY + y;
                if (newX >= 0 && newX < PathFinding.grid.GetLength(0) && newY >= 0 && newY < PathFinding.grid.GetLength(1))
                {
                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        bool canWalkHorizontal = PathFinding.grid[node.GridX + x, node.GridY].IsWalkable;
                        bool canWalkVertical = PathFinding.grid[node.GridX, node.GridY + y].IsWalkable;

                        if (!canWalkHorizontal || !canWalkVertical)
                        {
                            continue;
                        }
                    }
                    neighbors.Add(PathFinding.grid[newX, newY]);
                }
            }
        }

        return neighbors;

    }

    public List<Node> GetAllWalkableNeighbors(Node node)
    {
        List<Node> list = GetAllNeighbors(node);
        List<Node> result = new List<Node>();
        foreach (Node neighbor in list)
        {
            if (neighbor.IsWalkable && !neighbor.IsQccupied)
            {
                result.Add(neighbor);
            }
        }
        return result;
    }
    public Vector3 GetNodeSize()
    {
        return PathFinding.cellSize;
    }

    public bool CheckBlockBetween2Nodes(Vector2 positionA, Vector2 positionB)
    {
        Node nodeA = FindNode(positionA);
        Node nodeB = Instance.FindNode(positionB);


        if (nodeA == null || nodeB == null) return false;

        int x = nodeA.GridX;
        int y = nodeA.GridY;
        int x2 = nodeB.GridX;
        int y2 = nodeB.GridY;

        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (!PathFinding.grid[x, y].IsWalkable) return false;

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return true;
    }

    public Node GetClosestInteractableNode(GameObject target, Vector3 requesterPos, GameObject requester)
    {
        Collider2D col = target.GetComponent<Collider2D>();

        Vector3 searchOrigin;
        if (col != null)
        {
            searchOrigin = col.ClosestPoint(requesterPos);
        }
        else
        {
            searchOrigin = target.transform.position;
        }
        return FindNearestAvailableNode(searchOrigin, requester, false);
    }
    public bool CanPlaceBuilding(Vector3Int position, UnitSide side)
    {
        return SideBuildingArea[(int)side].HasTile(position) && !IsPlaceOverUnreachableArea(position) && !boundTilemap.HasTile(position);
    }

    public bool IsNodeAndNeighborsFree(Node targetNode, GameObject requester, bool checkNeighbors)
    {
        if (!targetNode.IsWalkable) return false;

        if (targetNode.occupant != null && targetNode.occupant != requester) return false;

        if (checkNeighbors)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    int nx = targetNode.GridX + x;
                    int ny = targetNode.GridY + y;
                    if (nx >= 0 && ny >= 0 && nx < PathFinding.grid.GetLength(0) && ny < PathFinding.grid.GetLength(1))
                    {
                        Node neigbor = PathFinding.grid[nx, ny];
                        if (neigbor.occupant != null && neigbor.occupant != requester) return false;

                    }
                }
            }
        }
        return true;
    }

    private bool IsPlaceOverUnreachableArea(Vector3Int position)
    {
        return UnreachaableTilemap.HasTile(position) || IsPlaceOverObstacle(position);
    }

    private bool IsPlaceOverObstacle(Vector3Int position)
    {
        Vector3 tileSize = WalkableTilemap.cellSize;

        

        Collider2D[] colliders = Physics2D.OverlapBoxAll(position + tileSize * .5f, tileSize * .9f, 0);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == "Unit" || collider.gameObject.tag == "Building")
            {
                return true;
            }
        }
        return false;
    }


    public bool CanWalkAtTile(Vector3 worldPos, Vector3 nodeSize)
    {
        Vector3Int tilePos = WalkableTilemap.WorldToCell(worldPos);

        bool hasWalkableTile = WalkableTilemap.HasTile(tilePos);
        bool hasUnreachableTile = UnreachaableTilemap.HasTile(tilePos);
        if (hasUnreachableTile || !hasWalkableTile) return false;

        //int buildingLayerMask = LayerMask.GetMask("Unit");

        Collider2D[] collider = Physics2D.OverlapBoxAll(worldPos, nodeSize * 0.9f, 0);
        if (collider != null)
        {
            foreach (var col in collider)
            {
                if (col.CompareTag("Building"))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public BoundsInt GetBuildingAreaForAI(UnitSide side)
    {
        return SideBuildingArea[(int)side].cellBounds;
    }


    public byte[] SaveCheckStatus() => PathFinding.GetNodeCheckStatus();

    public void LoadCheckStatus(byte[] data) => PathFinding.LoadNodeCheckStatus(data);

    private void OnDrawGizmos()
    {
        if (PathFinding != null)
        {
            Vector3 tileCellSize = TilemapManager.Instance.WalkableTilemap.cellSize;

            float resolutionScale = PathFinding.resolutionScale;

            Vector3 nodeDrawSize = new Vector3(tileCellSize.x / resolutionScale, tileCellSize.y / resolutionScale, 0.1f);
            if (PathFinding.CurrentPath != null)
            {
                Gizmos.color = Color.black;
                foreach (Node node in PathFinding.CurrentPath)
                {
                    Gizmos.DrawCube(new Vector3(node.CenterX, node.CenterY), nodeDrawSize * 0.9f);
                }
            }
            foreach (Node node in PathFinding.grid)
            {
               /* float hue = (node.AreaID * 0.618033988749895f) % 1.0f;
                Gizmos.color = Color.HSVToRGB(hue, 0.8f, 0.9f);
                Gizmos.DrawCube(new Vector3(node.CenterX, node.CenterY), nodeDrawSize * 0.9f);
                continue;*/
                if (node.IsWalkable == false)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(node.CenterX, node.CenterY), nodeDrawSize * 0.9f);
                }
                if (node.IsQccupied)
                {
                    if(node.occupant.GetComponent<ResourceUnit>() != null)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawCube(new Vector3(node.CenterX, node.CenterY), nodeDrawSize * 0.9f);
                }
            }
        }
    }

}
    

