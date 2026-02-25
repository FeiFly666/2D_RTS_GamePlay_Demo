using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEconomy
{
    private FactionAI AI;

    BuildingAction requestBuilding;

    public AIEconomy(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        if (AI.strategy.requestBuilding == null) return;

        requestBuilding = AI.strategy.requestBuilding;

        if (!AI.faction.CanAfford(requestBuilding.goldCost, requestBuilding.woodCost)) return;

        Vector3Int setPosition = SetBuildingPosition();

        if(setPosition.x != -1500)
        {
            BuildingManager.Instance.ConfirmPlacementForAI(requestBuilding, AI.unitSide ,setPosition);

            Physics2D.SyncTransforms();

            AI.strategy.requestBuilding = null;
        }

    }

    public Vector3Int SetBuildingPosition()
    {
        Vector3 startPosition = AI.basePosition;

        Vector3Int startCell = TilemapManager.Instance.WalkableTilemap.WorldToCell(startPosition);

        BoundsInt bounds = TilemapManager.Instance.GetBuildingAreaForAI(AI.unitSide);

        Vector3Int maxPos = bounds.max;
        Vector3Int minPos = bounds.min;

        Vector3Int buildingSize = requestBuilding.buildingSize;
        Vector3Int offset = requestBuilding.buildingOffset;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        int scanLimit = 1500;
        int scanCount = 0;

        while (queue.Count > 0 && scanLimit > scanCount)
        {
            Vector3Int currentPos = queue.Dequeue();
            scanCount++;

            if (CanBuild(currentPos, buildingSize, minPos, maxPos))
            {
                float random = Random.Range(0, 1f);
                if(random > 0.3f)//留点空间
                {
                    //currentPos为左下角，需要减去偏移量算出中心点 ，操了改了3小时才看见这
                    return currentPos - offset;
                }
            }

            Vector3Int[] neighbors = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
            int randStart = Random.Range(0, 4);

            for(int i = 0; i<4;i++)
            {
                Vector3Int dir = neighbors[(i + randStart) % 4];
                Vector3Int neighborPos = currentPos + dir;

                if (neighborPos.x < minPos.x || neighborPos.x > maxPos.x || neighborPos.y < minPos.y || neighborPos.y > maxPos.y) continue;

                if (!visited.Contains(neighborPos) )
                {
                    visited.Add(neighborPos);
                    queue.Enqueue(neighborPos);
                }
            }
        }

        return new Vector3Int(-1500, -1500, -1500);
    }
    private bool CanBuild(Vector3Int bottomLeft, Vector3Int size, Vector3Int minPos, Vector3Int maxPos)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkPos = new Vector3Int(bottomLeft.x + x, bottomLeft.y + y, 0);

                if (checkPos.x < minPos.x || checkPos.x >= maxPos.x || checkPos.y < minPos.y || checkPos.y >= maxPos.y) return false;

                Node node = TilemapManager.Instance.FindNode(checkPos);

                if (node == null || !node.IsWalkable) return false;

                if (!TilemapManager.Instance.CanPlaceBuilding(checkPos, AI.unitSide)) return false;

            }
        }
        return true;
    }

}
