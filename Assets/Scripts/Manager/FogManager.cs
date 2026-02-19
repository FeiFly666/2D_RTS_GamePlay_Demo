using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogManager : MonoSingleton<FogManager>
{
    public Tilemap fogMap;
    public TileBase blackTile;
    [Header("更新频率")]
    [SerializeField] private float updateFrequency = 0.4f;
    private float updateTime;

    private PathFinding PathFinding;

    private Dictionary<int, Vector2Int[]> circularOffsetsCache = new Dictionary<int, Vector2Int[]>();
    void Start()
    {
        PathFinding = TilemapManager.Instance.GetPathFinding();
        if(GameManager.Instance.isNeedFog)
        {
            InitFOW();
        }
    }
    private Vector2Int[] GetCircularOffsets(int r)
    {
        if (circularOffsetsCache.ContainsKey(r)) return circularOffsetsCache[r];

        List<Vector2Int> offsets = new List<Vector2Int>();
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    offsets.Add(new Vector2Int(x, y));
                }
            }
        }

        Vector2Int[] resultArray = offsets.ToArray();
        circularOffsetsCache.Add(r, resultArray);
        return resultArray;
    }
    public void InitFOW()
    {
        fogMap.ClearAllTiles();
        HashSet<Vector3Int>  toShow = new HashSet<Vector3Int>();
        HashSet<Vector3Int>  toHide = new HashSet<Vector3Int>();

        foreach (var node in PathFinding.grid)
        {
            Vector3 worldPos = new Vector3(node.CenterX, node.CenterY, 0);
            Vector3Int tilePos = fogMap.WorldToCell(worldPos);

            if (!node.isChecked)
            {
                toHide.Add(tilePos);
            }
            else
            {
                toShow.Add(tilePos);
            }
        }
        ExecuteUpdate(toShow);
        ExecuteUpdate(toHide, blackTile);
    }
    private void Update()
    {
        if(Time.time - updateTime > updateFrequency)
        {
            updateTime = Time.time;
            BatchRevealFog();
        }
    }
    private void BatchRevealFog()
    {
        if (PathFinding == null) return;

        RevealFogInRadius();
    }

    public void RevealFogInRadius()
    {

        HashSet<Vector3Int> tilesToClear = new HashSet<Vector3Int>();

        var units = GameManager.Instance.liveHumanUnits;

        foreach(var unit in units)
        {
            if (unit.unitSide != GameManager.Instance.playerSide) continue;
            Node centerNode = TilemapManager.Instance.FindNode(unit.detectPosition);

            int width = PathFinding.grid.GetLength(0);
            int height = PathFinding.grid.GetLength(1);

            if (centerNode == null) continue;

            int r = Mathf.CeilToInt(unit.dectectRadius / TilemapManager.Instance.GetNodeSize().x);

            Vector2Int[] offsets = GetCircularOffsets(r);
            foreach (var offset in offsets)
            {
                int gx = centerNode.GridX + offset.x;
                int gy = centerNode.GridY + offset.y;

                if (gx >= 0 && gx < width && gy >= 0 && gy < height)
                {
                    Node node = PathFinding.grid[gx, gy];

                    if (!node.isChecked)
                    {
                        node.isChecked = true;

                        Vector3 nodeWorldPos = new Vector3(node.CenterX, node.CenterY, 0);
                        tilesToClear.Add(fogMap.WorldToCell(nodeWorldPos));
                    }
                }
            }
        }
        if(tilesToClear.Count > 0) 
        {
            ExecuteUpdate(tilesToClear);
        }
        
    }
    private void ExecuteUpdate(HashSet<Vector3Int> posSet, TileBase tile = null)
    {
        int count = posSet.Count;
        Vector3Int[] positions = new Vector3Int[count];
        TileBase[] tiles = new TileBase[count];

        int i = 0;
        foreach (var pos in posSet)
        {
            positions[i] = pos;
            tiles[i] = tile;
            i++;
        }
        fogMap.SetTiles(positions, tiles);
    }
}
