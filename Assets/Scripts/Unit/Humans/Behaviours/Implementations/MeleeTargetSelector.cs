using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;


public class MeleeTargetSelector : ITargetSelector 
{
    private static Collider2D[] scanBuffer = new Collider2D[40];
    public Unit SetNewTarget(HumanUnit self)
    {
        if (self.lastAttacker != null)
        {
            if ( Time.time - self.lastAttackTime > 5|| self.lastAttacker.isDead)
            {
                self.lastAttacker = null;
            }
            else if (IsTargetReachable(self, self.lastAttacker))
            {
                return self.lastAttacker;
            }
        }

        int num = Physics2D.OverlapCircleNonAlloc(self.detectPosition, self.dectectRadius, scanBuffer, GameManager.EnemyMasks[(int)self.unitSide]);
        
        Unit closestEnemy = null;
        float closestEnemyDistance = Mathf.Infinity;
        //优先找兵
        for (int i = 0; i < num; i++)
        {
            HumanUnit enemy = scanBuffer[i].GetComponent<HumanUnit>();
            if (enemy == null || enemy.isDead || enemy.unitSide == self.unitSide || enemy.isBuildingUnit) continue;
            if (!IsTargetReachable(self, enemy)) { continue; }

            float distance = (enemy.transform.position - self.transform.position).sqrMagnitude;

            if (distance < closestEnemyDistance)
            {
                closestEnemy = enemy;
                closestEnemyDistance = distance;
            }
        }
        if (closestEnemy != null)
        {
            return closestEnemy;
        }

        //寻找建筑
        Unit closestBuilding = null;
        float closestDistance = Mathf.Infinity;
        List<Unit> staticBuildings = new List<Unit>();

        for (int i = 0; i < num; i++)
        {
            BuildingUnit building = scanBuffer[i].GetComponent<BuildingUnit>();
            if (building == null || building.isDead || building.unitSide == self.unitSide) continue;
            if (!IsTargetReachable(self,building)) continue;
            if (building.buildingType == BuildingType.Static)
            {
                staticBuildings.Add(building); continue;
            }

            float distance = (building.transform.position - self.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBuilding = building;
            }
        }
        if (closestBuilding != null)
        {
            return closestBuilding;
        }

        //非玩家操控方可以自主锁定静态建筑
        if (self.unitSide != GameManager.Instance.playerSide)
        {
            foreach (var building in staticBuildings)
            {
                float distance = (building.transform.position - self.transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBuilding = building;
                }
            }
            if (closestBuilding != null)
            {
               return closestBuilding;
            }
        }
        return null;
    }
    public bool IsTargetReachable(HumanUnit self, Unit targetUnit, bool isForced = false)
    {
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

