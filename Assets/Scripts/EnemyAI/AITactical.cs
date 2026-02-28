using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITactical
{
    private FactionAI AI;
    public AITactical(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        ManageWorkers();
        ManageSoldiers();
    }
    private void ManageWorkers()
    {
        var allWorkers = AI.faction.workers;

        List<Worker> idleWorkers = AI.faction.IdleWorkers;

        int needToWorkNum = Mathf.Max(idleWorkers.Count - 2, 0);

        if (idleWorkers.Count <= 4)
        {
            needToWorkNum = idleWorkers.Count - 1;
        }
        

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
        GoldMine mine = GetAvailableGoldMines();
        if(chopNum == 0)
        {
            var ntree = GetNearestTree(worker.transform.position);
            if (ntree != null)
            {
                worker.SetClickTarget(ntree);
                return;
            }
        }
        if (mine != null)
        {
            worker.SetClickTarget(mine);
            return;
        }
        var tree = GetNearestTree(worker.transform.position);
        if (tree != null)
        {
            worker.SetClickTarget(tree);
            return;
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

        if (mine.CanInside)
        {
            if (!idleCombatants[i].isBuildingUnit)
            {
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
        return GameManager.Instance.resources
            .Where(r =>  r.resourceLeftNum > 0)
            .OrderBy(r => (r.transform.position - origin).sqrMagnitude)
            .FirstOrDefault();
    }
}
