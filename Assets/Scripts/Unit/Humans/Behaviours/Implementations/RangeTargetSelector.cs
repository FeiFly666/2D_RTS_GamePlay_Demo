using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;

internal class RangeTargetSelector : ITargetSelector
{
    private static Collider2D[] scanBuffer = new Collider2D[80];
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

        int num = Physics2D.OverlapCircleNonAlloc(self.detectPosition, self.detectRadius, scanBuffer, GameManager.EnemyMasks[(int)self.unitSide]);

        Unit closestEnemy = null;
        float closestEnemyDistance = Mathf.Infinity;

        BuildingUnit closestStaticEnemyBuilding = null;
        float closestStaticBuildingDistance = Mathf.Infinity;

        for (int i = 0; i < num; i++)
        {
            Unit unit = scanBuffer[i].GetComponent<Unit>();
            if (unit == null || unit.isDead || unit.unitSide == self.unitSide) continue;

            if (unit is ResourceUnit) continue;

            if (unit is HumanUnit human)
            {
                if (human.isBuildingUnit) continue;
            }

            float distance = (unit.transform.position - self.transform.position).sqrMagnitude;

            if (unit is BuildingUnit building && building.buildingType != BuildingType.Attack && building.buildingType != BuildingType.Collect)
            {
                if (distance < closestStaticBuildingDistance)
                {
                    if (!IsTargetReachable(self, unit)) continue;
                    closestStaticEnemyBuilding = building;
                    closestStaticBuildingDistance = distance;

                }
                continue;
            }

            if (distance < closestEnemyDistance)
            {
                if (!IsTargetReachable(self, unit)) { continue; }
                closestEnemy = unit;
                closestEnemyDistance = distance;
            }
        }
        if (closestEnemy != null)
        {
            return closestEnemy;
        }


        //非玩家操控方可以自主锁定静态建筑

        if (self.unitSide != GameManager.Instance.playerSide && closestStaticEnemyBuilding != null || self.isBuildingUnit)
        {
            return closestStaticEnemyBuilding;
        }

        return null;

    }
    public bool IsTargetReachable(HumanUnit self, Unit targetUnit, bool isForced = false)
    {
        if (self == null || self.isDead) return false;

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
            endNode = TilemapManager.Instance.GetClosestInteractableNode(targetUnit, self.transform.position, self.gameObject);
        }

        if (startNode == null || endNode == null) return false;
        if (startNode.AreaID == endNode.AreaID) return true;

        return false;

    }
}
