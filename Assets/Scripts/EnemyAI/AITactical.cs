
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
[System.Serializable]
public class AITactical
{
    private FactionAI AI;

    private UnitGroup attackGroup;
    public List<HumanUnit> groupMembers = new List<HumanUnit>();
    public AITactical(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        ManageWorkers();
        ManageSoldiers();
        if(AI.prepareForAttack || AI.attack)
        {
            ManageWar();
        }
    }
    private void ManageWorkers()
    {
        var allWorkers = AI.faction.workers;

        List<Worker> idleWorkers = AI.faction.IdleWorkers;

        int needToWorkNum = Mathf.Max(idleWorkers.Count - 2, 0);

        if (allWorkers.Count <= 4)
        {
            needToWorkNum = Mathf.Max(idleWorkers.Count - 1, 0);
        }

        if (needToWorkNum == 0) return;

        for (int i = 0; i < needToWorkNum; i++)
        {
            int chopTreeNum = 0;
            foreach (var worker in allWorkers)
            {
                if (worker.ResourceAreaID != -1)
                {
                    chopTreeNum++;
                    break;
                }
            }
            AssignTaskToWorker(idleWorkers[i],chopTreeNum);
        }
    }
    private void AssignTaskToWorker(Worker worker,int chopNum)
    {
        if(worker == null) return;
        var ntree = GetNearestTree(worker.transform.position);

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
        if(AI.prepareForAttack)
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
        //组队完成开始进攻
        if(attackGroup.members.Count == AI.nextAttackNum && !AI.attack)
        {
            AI.attack = true;

            FactionData playerFaction = GameManager.Instance.factions[(int)GameManager.Instance.playerSide];

            BuildingUnit targetBuilding = playerFaction.buildings[Random.Range(0, playerFaction.buildings.Count)];

            Node targetNode = TilemapManager.Instance.FindNearestAvailableNode(targetBuilding.transform.position, attackGroup.leader.gameObject, false);

            Vector3 targetPos = targetNode.GetNodePosition();

            attackGroup.FormGroupMoving(targetPos, targetBuilding);

            AI.attackTimes++;

            //为下一次进攻做准备
            AI.prepareForAttack = false;
            AI.nextAttackNum = Random.Range(15 + 5 * Mathf.Max(AI.attackTimes - 3 , 0), 35 + 10 * AI.attackTimes);

            AI.nextAttackNum = Mathf.Min(120, AI.nextAttackNum);
        }

        //进攻
        if(AI.attack)
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

            FactionData playerFaction = GameManager.Instance.factions[(int)GameManager.Instance.playerSide];

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
                    RedetectNewTarget(soldier, playerFaction);
                }
            }

        }
    }
    private void RedetectNewTarget(HumanUnit u, FactionData playerFaction)
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

        foreach(var building in playerFaction.buildings)
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

    private GoldMine GetAvailableGoldMines()
    {
        foreach(var m in AI.faction.goldMines)
        {
            if(m.buildingState == BuildingState.ConstructionFinished && m.humanInsideData.Count < m.maxUnitNum)
            {
                return m;
            }
        }
        return null;
    }

    private ResourceUnit GetNearestTree(Vector3 origin)
    {
        float minDis = Mathf.Infinity;
        ResourceUnit resourceUnit = null;

        foreach(var tree in GameManager.Instance.resources)
        {
            if (tree.resourceLeftNum == 0) continue;

            float dis = (origin - tree.transform.position).sqrMagnitude;

            if(dis < minDis)
            {
                minDis = dis;
                resourceUnit = tree;
            }
        }

        return resourceUnit;
       /*return GameManager.Instance.resources
            .Where(r =>  r.resourceLeftNum > 0)
            .OrderBy(r => (r.transform.position - origin).sqrMagnitude)
            .FirstOrDefault();*/
    }
}
