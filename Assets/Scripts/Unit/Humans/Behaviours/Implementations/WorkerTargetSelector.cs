using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;

public class WorkerTargetSelector : ITargetSelector
{
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

        //资源区域ID不为-1说明worker已经被指定去采集物资，故只判断区域内附近的资源，资源为空时ID调回-1
        if (worker.ResourceAreaID != -1)
        {
            List<ResourceUnit> areaResources = GameManager.Instance.areaResources[worker.ResourceAreaID];

            ResourceUnit resource = null;
            int resourceID = -1;
            float closestDis = Mathf.Infinity;
            //找可用的同资源区域的资源
            foreach (var currentResource in areaResources)
            {
                if (currentResource == null || !IsTargetReachable(self, currentResource)) continue;
                if (!currentResource.CanAddWorker) continue;

                if (!IsTargetReachable(self, currentResource)) continue;

                float distance = (self.transform.position - currentResource.transform.position).sqrMagnitude;
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
            List<BuildingUnit> allyBuildings = GameManager.Instance.factions[(int)worker.unitSide].buildings;

            BuildingUnit unfinishedBuilding = null;

            float closestDis = Mathf.Infinity;

            BuildingUnit damagedBuilding = null;
            float closestDis2 = Mathf.Infinity;

            //找未完成的建筑 + 受伤建筑
            foreach (var currentBuilding in allyBuildings)
            {
                if (currentBuilding == null) continue;

                if (currentBuilding is GoldMine goldMine && goldMine.buildingState == BuildingState.ConstructionFinished) continue;//不想修bug，所以金矿坑不能修理

                if (!IsTargetReachable(self, currentBuilding)) continue;

                float distance = (self.transform.position - currentBuilding.transform.position).sqrMagnitude;

                if (currentBuilding.buildingState == BuildingState.InConstruction)
                {
                    if (distance < closestDis)
                    {
                        unfinishedBuilding = currentBuilding;
                        closestDis = distance;
                    }
                }
                else if(!currentBuilding.stats.IsFullHP)
                {
                    if (distance < closestDis2)
                    {
                        damagedBuilding = currentBuilding;
                        closestDis2 = distance;
                    }
                }
            }

            if(unfinishedBuilding != null)
            {
                return unfinishedBuilding;
            }

/*            //找未满血的建筑
            foreach (var currentBuilding in allyBuildings)
            {
                if (currentBuilding == null) continue;
                if (currentBuilding.buildingState != BuildingState.ConstructionFinished) continue;
                if (currentBuilding.stats.IsFullHP) continue;

                if (!IsTargetReachable(self, currentBuilding)) continue;

                float distance = (self.transform.position - currentBuilding.transform.position).sqrMagnitude;
                if (distance < closestDis)
                {
                    unfinishedBuilding = currentBuilding;
                    closestDis = distance;
                }

            }*/

            if (damagedBuilding != null)
            {
                return damagedBuilding;
            }
        }
        //资源区域无可用资源/无需要worker进行修理建造的建筑
    
        return null;
    }

    public bool IsTargetReachable(HumanUnit self, Unit targetUnit, bool isForced = false)
    {
        if (self == null || self.isDead) return false;

        if (self is Worker w)
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
