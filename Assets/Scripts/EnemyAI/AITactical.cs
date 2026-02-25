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

        List<Worker> idleWorkers = allWorkers
            .Where(w => w.stateMachine.CurrentState is IdleState && !w.ai.IsPathVaild())
            .ToList();

        if (idleWorkers.Count <= 2) return;

        int needToWorkNum = idleWorkers.Count - 2;
        

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
        var mines = GetAvailableGoldMines();
        if(chopNum == 0)
        {
            var ntree = GetNearestTree(worker.transform.position);
            if (ntree != null)
            {
                worker.SetClickTarget(ntree);
                return;
            }
        }
        if (mines.Count > 0)
        {
            worker.SetClickTarget(mines[0]);
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
        var idleCombatants = AI.faction.humans
            .Where(h => h.role != UnitRole.Worker && h.stateMachine.CurrentState is IdleState)
            .ToList();

        if (idleCombatants.Count == 0) return;

        var mines = GetAvailableGoldMines();
        if (mines.Count == 0) return;

        //防抖，一个士兵找到就退出
        foreach (var soldier in idleCombatants)
        {
            if (mines[0].CanInside)
            {
                soldier.SetClickTarget(mines[0]);
                break;
            }
        }
    }

    private List<GoldMine> GetAvailableGoldMines()
    {
        return AI.faction.buildings
            .OfType<GoldMine>()
            .Where(m => m.buildingState == BuildingState.ConstructionFinished && m.CanInside)
            .ToList();
    }

    private ResourceUnit GetNearestTree(Vector3 origin)
    {
        return GameManager.Instance.resources
            .Where(r =>  r.resourceLeftNum > 0)
            .OrderBy(r => (r.transform.position - origin).sqrMagnitude)
            .FirstOrDefault();
    }
}
