using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementProcess
{
    private GameObject PlacementOutline;

    private BuildingAction BuildingAction;

    private Vector3Int[] highlightArea;

    [SerializeField] private Sprite highlightTile;

    public BuildingAction buildingAction => BuildingAction;

    public UnitSide buildingSide;
    

    public PlacementProcess(BuildingAction action, UnitSide side)
    {
        PlacementOutline = new GameObject("PlacementOutline");

        BuildingAction = action;

        highlightTile = Resources.Load<Sprite>("Image/highlightTile");

        highlightArea = new Vector3Int[] { };

        this.buildingSide = side;
    }
    public void Update()
    {
        Vector3 worldPos = CommonUtils.GetWorldMousePosition();

        if(highlightArea != null)
        {
            HighlightArea(PlacementOutline.transform.position);
        }

        PlacementOutline.transform.position = SnapToGrid(worldPos);
    }

    private void HighlightArea(Vector3 outlinePos)
    {
        ClearHighlightArea();
        InitHightlightArea(outlinePos);

        foreach(var position in highlightArea)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = highlightTile;

            TilemapManager.Instance.PlacementTilemap.SetTile(position,tile);

            TilemapManager.Instance.PlacementTilemap.SetTileFlags(position, TileFlags.None); //‘ –Ì–ﬁ∏ƒÕﬂ∆¨ Ù–‘

            if (TilemapManager.Instance.CanPlaceBuilding(position, buildingSide)) 
            {
                TilemapManager.Instance.PlacementTilemap.SetColor(position, new Color(1f, 1f, 1f, 0.75f));
            }
            else
            {
                TilemapManager.Instance.PlacementTilemap.SetColor(position, new Color(1f, 0, 0, 0.75f));
            }
        }
    }

    public void ClearHighlightArea()
    {
        if(highlightArea != null)
        {
            foreach(var pos in highlightArea)
            {
                TilemapManager.Instance.PlacementTilemap.SetTile(pos, null);
            }
            highlightArea = null;
        }
    }
    public void DestroyOutline()
    {
        GameObject.Destroy(PlacementOutline.gameObject);
    }

    private void InitHightlightArea(Vector3 outlinePos)
    {
        Vector3Int buildingSize = buildingAction.buildingSize;
        Vector3 leftBottomPos = outlinePos + buildingAction.buildingOffset;

        highlightArea = new Vector3Int[buildingSize.x * buildingSize.y];

        for(int i = 0; i< buildingSize.x; i++)
        {
            for(int j = 0; j< buildingSize.y; j++)
            {
                highlightArea[i + buildingSize.x * j] = new Vector3Int(
                        (int)leftBottomPos.x + i,
                        (int)leftBottomPos.y + j
                    ) ;
            }
        }
    }

    public bool CanPlaceBuilding(out Vector3 placePosition)
    {
        foreach(var pos in highlightArea)
        {
            if (!TilemapManager.Instance.CanPlaceBuilding(pos, buildingSide)) 
            {
                placePosition = Vector3.zero;
                return false;
            }
        }
        placePosition = PlacementOutline.transform.position;
        return true;
    }

    private Vector3Int SnapToGrid(Vector3 worldPos) => new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), 0);
}
