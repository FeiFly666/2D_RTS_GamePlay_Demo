
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
[System.Serializable]
public class AITactical
{
    private FactionAI AI;

    public UnitGroup attackGroup;
    public List<HumanUnit> groupMembers = new List<HumanUnit>();

    public UnitSide lastTargetSide;
    public AITactical(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        ManageWorkers();
        ManageSoldiers();
        if (AI.prepareForAttack || AI.attack)
        {
            ManageWar();
        }
    }
    private void ManageWorkers()
    {
        var allWorkers = AI.faction.workers;

        List<Worker> idleWorkers = AI.faction.IdleWorkers;

        int needToWorkNum = Mathf.Max(idleWorkers.Count - 2, 0);

        if (allWorkers.Count <= 5)
        {
            needToWorkNum = Mathf.Max(idleWorkers.Count - 1, 0);
        }

        //if (needToWorkNum == 0) return;

        int chopTreeNum = 0;

        foreach (var worker in allWorkers)
        {
            if (worker.ResourceAreaID != -1)
            {
                chopTreeNum++;
                break;
            }
        }

        for (int i = 0; i < needToWorkNum; i++)
        {
            if (idleWorkers.Count <= 0) break;
            AssignTaskToWorker(idleWorkers[i], chopTreeNum);
        }
        if (AI.faction.IsAnyBuildingInConstruction())//有建筑建造、有工人在修建筑，让工人先去建造建筑
        {
            BuildingUnit building = AI.faction.GetOneBuildingInConstruction();
            if (building.GetBuildingProcess()?.IsEmpty == false)
            {
                return;
            }
            int n = allWorkers.Count <= 5 ? 1 : 2;
            int findNum = 0;

            bool hasHealWorker = false;
            foreach (var worker in allWorkers)
            {
                if (worker.target == null) continue;
                if (worker.target.unitSide == worker.unitSide && worker.target is BuildingUnit b && b.stats.currentHP != b.stats.FullHP)
                {
                    worker.target = building;
                    worker.targetID = building.uniqueID;
                    findNum++;
                    hasHealWorker = true;
                    if (findNum >= n)
                    {
                        break;
                    }
                }
            }
            if (!hasHealWorker)
            {
                foreach (var worker in allWorkers)
                {
                    worker.TransitionTo(UnitStateType.Idle);
                    worker.target = building;
                    worker.targetID = building.uniqueID;
                    findNum++;
                    if (findNum >= n)
                    {
                        break;
                    }
                }
            }
        }
    }
    private void AssignTaskToWorker(Worker worker, int chopNum)
    {
        if (worker == null) return;
        var ntree = GetNearestTree(worker);

        if (chopNum == 0)
        {
            if (ntree != null)
            {
                worker.SetClickTarget(ntree);
                return;
            }
        }

        GoldMine mine = GetAvailableGoldMines();
        if (mine != null)
        {
            worker.SetClickTarget(mine);
            return;
        }

        if (ntree != null)
        {
            worker.SetClickTarget(ntree);
            return;
        }
    }
    private void ManageWar()
    {
        //ai判断可以开始组队
        if (AI.prepareForAttack)
        {
            if (attackGroup == null)
            {
                attackGroup = new UnitGroup();
                groupMembers.Clear();
            }

            for (int i = groupMembers.Count - 1; i >= 0; i--)
            {
                HumanUnit soldier = groupMembers[i];

                if (soldier == null || soldier.isDead)
                {
                    groupMembers.RemoveAt(i);
                    continue;
                }
            }

            if (attackGroup.members.Count < AI.nextAttackNum)
            {
                var idleCombatants = AI.faction.IdleNoWorkerHumans;

                if (idleCombatants.Count == 0) return;
                int i = Random.Range(0, idleCombatants.Count);

                if (!idleCombatants[i].isBuildingUnit && !idleCombatants[i].HasRegisterTarget && !attackGroup.members.Contains(idleCombatants[i]))
                {
                    attackGroup.AddNewMember(idleCombatants[i]);
                    if (attackGroup.leader == null)
                        attackGroup.leader = idleCombatants[i];
                    groupMembers.Add(idleCombatants[i]);
                }
            }

        }
        if (attackGroup == null) return;
        //组队完成开始进攻
        if (attackGroup.members.Count == AI.nextAttackNum && !AI.attack)
        {
            AI.attack = true;

            FactionData targetFaction = GameManager.Instance.factions[(int)AI.targetSide];

            BuildingUnit targetBuilding = targetFaction.buildings[Random.Range(0, targetFaction.buildings.Count)];

            Node targetNode = TilemapManager.Instance.FindNearestAvailableNode(targetBuilding.transform.position, attackGroup.leader.gameObject, false);

            Vector3 targetPos = targetNode.GetNodePosition();

            attackGroup.FormGroupMoving(targetPos, targetBuilding);

            AI.attackTimes++;

            //为下一次进攻做准备
            AI.prepareForAttack = false;
            lastTargetSide = AI.targetSide;
            RandomNextAttackNum();
            if (LevelOption.Instance.enemyMode == EnemyMode.Free)
            {
                RandomTargetSide();
            }
        }

        //进攻
        if (AI.attack)
        {
            //Debug.LogError(1);
            for (int i = groupMembers.Count - 1; i >= 0; i--)
            {
                HumanUnit soldier = groupMembers[i];

                if (soldier == null || soldier.isDead)
                {
                    groupMembers.RemoveAt(i);
                    continue;
                }
            }

            if (groupMembers.Count == 0)
            {
                attackGroup = null;
                AI.attack = false;
                return;
            }

            FactionData targetFaction = GameManager.Instance.factions[(int)lastTargetSide];

            /*if(attackGroup.targetID == -1)
            {
                BuildingUnit targetBuilding = playerFaction.buildings[Random.Range(0, playerFaction.buildings.Count)];

                Node targetNode = TilemapManager.Instance.FindNearestAvailableNode(targetBuilding.transform.position, attackGroup.leader.gameObject, false);

                if (targetNode == null) Debug.LogError(2);

                if (targetNode == null)
                    return;

                Vector3 targetPos = targetNode.GetNodePosition();

                attackGroup.FormGroupMoving(targetPos, targetBuilding);
            }*/

            //Debug.LogError(2);
            for (int i = groupMembers.Count - 1; i >= 0; i--)
            {
                HumanUnit soldier = groupMembers[i];

                soldier.SetHomePosition(soldier.transform.position);

                //Debug.LogError(3);
                if (!soldier.ai.IsUnitInGroup && soldier.target == null)
                {
                    // 给他一个指令：根据进攻重心再次索敌
                    RedetectNewTarget(soldier, targetFaction);
                }
            }

        }
    }
    public void RandomNextAttackNum()
    {
        AI.nextAttackNum = Random.Range(15, 35 + 10 * AI.attackTimes);
        AI.nextAttackNum = Mathf.Min(200, AI.nextAttackNum);

        if (AI.faction.BuildingTypeCount[(int)BuildingType.Static] < 1)
            AI.nextAttackNum = Mathf.Min(AI.faction.TotalPeopleNum - 10, AI.nextAttackNum);
    }
    public void RandomTargetSide()
    {
        int targetSide = Random.Range(0, GameManager.Instance.sideNum);
        if ((UnitSide)targetSide == AI.unitSide || GameManager.Instance.factions[targetSide].buildings.Count <= 0)
        {
            targetSide = (int)GameManager.Instance.playerSide;
        }

        AI.targetSide = (UnitSide)targetSide;
    }
    public void ResumeAttackGroup()
    {
        attackGroup = new UnitGroup();

        foreach (var unit in groupMembers)
        {
            if (unit == null || unit.isDead) continue;
            attackGroup.AddNewMember(unit);
            if (attackGroup.leader == null)
                attackGroup.leader = unit;
        }
    }
    public void ResumeGroupMoving(BuildingUnit targetBuilding)
    {
        bool needGroupMove = true;
/*        foreach (var unit in groupMembers)
        {
            if (unit.target != null && unit.IsTargetDetected(unit.target))
            {
                Debug.Log(unit.uniqueID);
                needGroupMove = false;
                break;
            }
        }*/
        if (attackGroup.members.Count == 0) return;
        if (AI.attack)
        {
            if (needGroupMove)
            {

                Node targetNode = TilemapManager.Instance.FindNearestAvailableNode(targetBuilding.transform.position, attackGroup.leader.gameObject, false);

                Vector3 targetPos = targetNode.GetNodePosition();

                attackGroup.FormGroupMoving(targetPos, targetBuilding);
            }
        }
    }
    private void RedetectNewTarget(HumanUnit u, FactionData targetFaction)
    {
        //Debug.LogError(1);
        u.isReturningHome = false;
        //先自主索敌（躲避human更新时间差）
        Unit betterTarget = u.targetSelector?.SetNewTarget(u);
        if (betterTarget != null)
        {
            u.target = betterTarget;
            u.targetID = betterTarget.uniqueID;
            u.lastTargetInDetectionTime = Time.time;

            u.TransitionTo(UnitStateType.Move);
            return;
        }

        Unit minBuilding = null;
        int targetID = -1;
        float minDis = Mathf.Infinity;

        foreach(var building in targetFaction.buildings)
        {
            float dis = (building.transform.position - u.transform.position).sqrMagnitude;
            if (dis < minDis)
            {
                if (!u.targetSelector.IsTargetReachable(u, building)) continue;
                minBuilding = building;
                targetID = building.uniqueID;
                minDis = dis;
            }
        }
        if(minBuilding != null )
        {
            u.target = minBuilding;
            u.targetID = targetID;
            u.lastTargetInDetectionTime = Time.time;

            u.TransitionTo(UnitStateType.Move);
        }
    }
    private void ManageSoldiers()
    {
        var idleCombatants = AI.faction.IdleNoWorkerHumans;

        if (idleCombatants.Count == 0) return;

        GoldMine mine = GetAvailableGoldMines();
        if (mine == null) return;

        //防抖，一个士兵找到就退出
        int i = Random.Range(0, idleCombatants.Count);

        if (idleCombatants[i] == null) return;

        if (mine.CanInside)
        {
            if (!idleCombatants[i].isBuildingUnit && !idleCombatants[i].HasRegisterTarget)
            {
                if(attackGroup != null)
                {
                    if (attackGroup.members.Contains(idleCombatants[i]))
                        return;
                }
                idleCombatants[i].SetClickTarget(mine);
            }
        }
/*        foreach (var soldier in idleCombatants)
        {
            if (mines[0].CanInside)
            {
                soldier.SetClickTarget(mines[0]);
                break;
            }
        }*/
    }
    public void OnBuildingHurt(Unit unit)
    {
        var idleCombatants = AI.faction.IdleNoWorkerHumans;
        if(idleCombatants.Count == 0) { return; }

        if (unit == null) return;

        int j = Random.Range(0, idleCombatants.Count);

        if(j >= idleCombatants.Count) return;

        if (!idleCombatants[j].isBuildingUnit && !idleCombatants[j].HasRegisterTarget)
        {
            if (attackGroup != null)
            {
                if (attackGroup.members.Contains(idleCombatants[j]))
                    return;
            }
            idleCombatants[j].SetClickTarget(unit);
        }
    }

    private GoldMine GetAvailableGoldMines()
    {
        foreach(var m in AI.faction.goldMines)
        {
            if(m.buildingState == BuildingState.ConstructionFinished && m.humanInsideData.Count < m.maxUnitNum)
            {
                if(AI.faction.currentPeopleNum > 20)
                {
                    return m;
                }
                else if(m.humanInsideData.Count < 1)
                {
                    return m;
                }
            }
        }
        return null;
    }

    private ResourceUnit GetNearestTree(Worker worker)
    {
        /*ResourceUnit resourceUnit = null;
        float minDis = Mathf.Infinity;
        foreach(var tree in GameManager.Instance.resources)
        {
            if (tree.resourceLeftNum == 0) continue;

            float dis = (origin - tree.transform.position).sqrMagnitude;

            if(dis < minDis)
            {
                minDis = dis;
                resourceUnit = tree;
            }
        }*/

        Vector3 origin = worker.transform.position;
        int closestArea = -1;
        float minDis = Mathf.Infinity;
        foreach (var kv in GameManager.Instance.areaResources)
        {
            int AreaID = kv.Key;
            if (kv.Value.Count == 0)
                continue;

            int idx = Random.Range(0, kv.Value.Count);

            ResourceUnit unit = kv.Value[idx];

            if (unit == null || unit.resourceLeftNum == 0) continue;

            float dis = (origin - unit.transform.position).sqrMagnitude;

            if (dis < minDis)
            {
                minDis = dis;
                closestArea = AreaID;
            }
        }
        ResourceUnit closestUnit = null;
        minDis = Mathf.Infinity;
        if(closestArea == -1)
            return closestUnit;

        foreach(var r in GameManager.Instance.areaResources[closestArea])
        {
            if (r.resourceLeftNum <= 0 || r == null) continue;
            if(!r.CanAddWorker) continue;
            if (!worker.targetSelector.IsTargetReachable(worker, r)) continue;
            float dis = (origin - r.transform.position).sqrMagnitude;

            if (dis < minDis)
            {
                minDis = dis;
                closestUnit = r;
            }
        }

        return closestUnit;
    }
}
