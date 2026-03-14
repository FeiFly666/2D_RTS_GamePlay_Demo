using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIEconomy
{
    private FactionAI AI;

    BuildingAction requestBuilding;

    private float buildCooldown = 5f;
    private float cooldownStart = 0f;

    public AIEconomy(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        UpdateResourcesBlance();

        UpdateBuilding();
    }
    public void UpdateResourcesBlance()
    {
        if (AI.faction.BuildingTypeCount[(int)BuildingType.Collect] > 0)
        {
            if (AI.faction.GoldNum > 600 && AI.faction.GoldNum > AI.faction.WoodNum * 3)
            {
                //1:3换一些木头
                AI.faction.TrySpendResource(120, 0);
                AI.faction.AddWood(40);
            }
        }
    }
    public void UpdateBuilding()
    {
        if (AI.strategy.requestBuilding == null) return;

        if (Time.time - cooldownStart < buildCooldown) return;

        requestBuilding = AI.strategy.requestBuilding;


        if (!AI.faction.CanAfford(requestBuilding.goldCost, requestBuilding.woodCost)) return;

        //AI.StartCoroutine(SetBuildingPosition(SetBuildingCallBack));

        Vector3Int setPosition = FindBuildPosition(AI.strategy.requestBuilding);

        if (setPosition.x != -1500)
        {
            BuildingManager.Instance.ConfirmPlacementForAI(requestBuilding, AI.unitSide, setPosition);

            Physics2D.SyncTransforms();

            AI.strategy.requestBuilding = null;

            cooldownStart = Time.time;

        }

    }
    Vector3Int FindBuildPosition(BuildingAction building)
    {
        Vector3Int size = building.buildingSize;
        Vector3Int offset = building.buildingOffset;

        BuildingUnit randomCenter;
        if (Random.value < 0.7f)
            randomCenter = AI.faction.buildings[0];
        else
        {
            int idx = Random.Range(0, AI.faction.buildings.Count);
            randomCenter = AI.faction.buildings[idx];
        }
        Vector3Int baseCell = TilemapManager.Instance.WalkableTilemap.WorldToCell(randomCenter.transform.position);
        BoundsInt bounds = TilemapManager.Instance.GetBuildingAreaForAI(AI.unitSide);

        Vector3Int maxPos = bounds.max;
        Vector3Int minPos = bounds.min;
        int extendX = (randomCenter.data.buildingSize.x - 1) * 3;
        int extendY = (randomCenter.data.buildingSize.y - 1) * 3;

        Vector3Int buildMinPos = new Vector3Int(Mathf.Max(baseCell.x - extendX, minPos.x),Mathf.Max( baseCell.y - extendY, minPos.y));
        Vector3Int buildMaxPos = new Vector3Int(Mathf.Min(baseCell.x + extendX, maxPos.x), Mathf.Min(baseCell.y + extendY, maxPos.y));

        //GenerateCandidates(baseCell, radius);
        for (int i = 0; i < 40; i++)
        {
            int x = Random.Range(buildMinPos.x, buildMaxPos.x);
            int y = Random.Range(buildMinPos.y, buildMaxPos.y);
            Vector3Int pos = new Vector3Int(x, y);

            if (!TilemapManager.Instance.CanPlaceBuilding(pos, AI.unitSide)) continue;

            if (CanBuild(pos, size, minPos, maxPos))
                return pos - offset ;

        }

        return new Vector3Int(-1500,-1500,-1500);
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
    /* private void SetBuildingCallBack(Vector3Int pos)
     {
         Vector3Int setPosition = pos;

         if (setPosition.x != -1500)
         {
             BuildingManager.Instance.ConfirmPlacementForAI(requestBuilding, AI.unitSide, setPosition);

             Physics2D.SyncTransforms();

             AI.strategy.requestBuilding = null;

             cooldownStart = Time.time;
         }
     }
     IEnumerator SetBuildingPosition(System.Action<Vector3Int> callback)
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

         int scanLimit = 3000;
         int scanCount = 0;

         int scanLimitForFrame = 500;
         int scanCountThisFrame = 0;

         while (queue.Count > 0 && scanLimit > scanCount)
         {
             Vector3Int currentPos = queue.Dequeue();
             scanCount++;
             scanCountThisFrame++;

             if (CanBuild(currentPos, buildingSize, minPos, maxPos))
             {
                 float random = Random.Range(0, 1f);
                 if(random > 0.3f)//留点空间
                 {
                     //currentPos为左下角，需要减去偏移量算出中心点 ，操了改了3小时才看见这
                     Vector3Int finalPos = currentPos - offset;

                     callback?.Invoke(finalPos);
                     //return currentPos - offset;
                     yield break;
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
             if(scanLimitForFrame <= scanCountThisFrame)
             {
                 scanCountThisFrame = 0;

                 yield return null;

             }
         }
         callback?.Invoke(new Vector3Int(-1500, -1500, -1500));
         yield break;
     }*/

}
