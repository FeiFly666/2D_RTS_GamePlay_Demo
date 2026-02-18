using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;

public class WorkerTargetSelector : ITargetSelector
{
    private static Collider2D[] scanBuffer = new Collider2D[100];
    public Unit SetNewTarget(HumanUnit self)
    {
        if (self.lastAttacker != null)
        {
            if (Time.time - self.lastAttackTime > 5 || self.lastAttacker.isDead)
            {
                self.lastAttacker = null;
            }
            else if (IsTargetReachable(self, self.lastAttacker))
            {

                return self.lastAttacker;
            }
        }

        Worker worker = self as Worker;
        int num = Physics2D.OverlapCircleNonAlloc(self.detectPosition, self.dectectRadius, scanBuffer, 1 << self.gameObject.layer);


        //资源区域ID不为-1说明worker已经被指定去采集物资，故只判断区域内附近的资源，资源为空时ID调回-1
        if (worker.ResourceAreaID != -1)
        {
            List<ResourceUnit> areaResources = GameManager.Instance.resources.FindAll(r => r.resourceAreaID == worker.ResourceAreaID);

            ResourceUnit resource = null;
            int resourceID = -1;
            float closestDis = Mathf.Infinity;
            //找可用的同资源区域的资源
            foreach (var currentResource in areaResources)
            {
                if (currentResource == null || !IsTargetReachable(self, currentResource)) continue;
                if (!currentResource.CanAddWorker) continue;
                float distance = Vector2.Distance(self.transform.position, currentResource.transform.position);
                if (distance < closestDis)
                {
                    resource = currentResource;
                    closestDis = distance;
                }
            }
            if(resource != null)
            {
                resourceID = resource.resourceAreaID;
            }
            worker.ResourceAreaID = resourceID;
            return resource;

        }
        else//资源区域ID为-1，说明worker没有被派去采集物资，此时worker会自动选择未建造完成的建筑，如无未建造完成的则选择非满血状态的建筑进行回血
        {
            BuildingUnit building = null;
            float closestDis = Mathf.Infinity;
            //找未完成的建筑
            for (int i = 0; i < num; i++) 
            {
                BuildingUnit currentBuilding = scanBuffer[i].GetComponent<BuildingUnit>();
                if (currentBuilding == null || !IsTargetReachable(self,currentBuilding)) continue;
                if (currentBuilding.buildingState == BuildingState.ConstructionFinished) continue;

                float distance = Vector2.Distance(self.transform.position, currentBuilding.transform.position);
                if (distance < closestDis)
                {
                    building = currentBuilding;
                    closestDis = distance;
                }
            }
            if(building != null)
            {
                return building;
            }
            //找未满血的建筑
            for (int i = 0; i < num; i++)
            {
                BuildingUnit currentBuilding = scanBuffer[i].GetComponent<BuildingUnit>();
                if (currentBuilding == null || !IsTargetReachable(self, currentBuilding)) continue;
                if (currentBuilding.buildingState != BuildingState.ConstructionFinished) continue;
                if (currentBuilding.stats.IsFullHP) continue;

                float distance = Vector2.Distance(self.transform.position, currentBuilding.transform.position);
                if (distance < closestDis)
                {
                    building = currentBuilding;
                    closestDis = distance;
                }
            }
            if (building != null)
            {
                return building;
            }
        }
        //资源区域无可用资源/无需要worker进行修理建造的建筑
    
        return null;
    }

    public bool IsTargetReachable(HumanUnit self, Unit targetUnit, bool isForced = false)
    {
        if(self is Worker w)
        {
            if (w.woodGO.activeSelf) return false;
        }
        int id = targetUnit.gameObject.GetInstanceID();

        if (self.myUnreachableCache.ContainsKey(id))
        {
            if (Time.time < self.myUnreachableCache[id]) return false;
            else self.myUnreachableCache.Remove(id);
        }
        Node startNode = TilemapManager.Instance.FindNode(self.transform.position);
        Node endNode = null;
        if (targetUnit is HumanUnit)
        {
            endNode = TilemapManager.Instance.FindNode(targetUnit.transform.position);
        }
        else
        {
            endNode = TilemapManager.Instance.GetClosestInteractableNode(targetUnit.gameObject, self.transform.position, self.gameObject);
        }

        if (startNode == null || endNode == null) return false;
        if (startNode.AreaID == endNode.AreaID) return true;
        return false;

    }
}
