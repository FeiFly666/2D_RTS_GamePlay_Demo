using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorkState :IUnitState
{
    private Worker u;
    public WorkState(Worker unit) { this.u = unit; }

    public void Enter()
    {

    }
    public void Update()
    {
        if (u.target is BuildingUnit building)//建造/修理建筑
        {
            HandleBuilding();
        }
        else if (u.target is ResourceUnit resource)//采集资源
        {
            HandleGathering();
        }
        else//啥事没有，待机
        {
            u.TransitionTo(UnitStateType.Idle);
        }
    }
    private void HandleBuilding()
    {
        BuildingUnit building = u.target as BuildingUnit;
        if (u.currentWorkBuilidng != null && u.currentWorkBuilidng.buildingState == BuildingState.ConstructionFinished && u.currentWorkBuilidng.stats.IsFullHP)
        {
            u.AnimationFinishTrigger3();

            u.target = null;
            u.currentWorkBuilidng = null;

            u.ai.ClearPath();
            u.TransitionTo(UnitStateType.Idle);
            return;
        }

        if (u.currentWorkBuilidng != building)
        {
            u.OnExitWorkState();
            u.currentWorkBuilidng = building;

            if (building.buildingState == BuildingState.InConstruction)
            {
                building.GetBuildingProcess().AddWorker(u);

            }
        }
        if (u.currentWorkBuilidng != null && u.currentWorkBuilidng.buildingState == BuildingState.ConstructionFinished && !u.currentWorkBuilidng.stats.IsFullHP)
        {
            float repairAmount = 10f * u.checkFrequency;
            building.stats.IncreaseHP(repairAmount);
        }
        u.FlipController(building.transform.position);
        u.anim.SetBool("Build", true);
    }
    private void HandleGathering()
    {
        ResourceUnit resource = u.target as ResourceUnit;
        if (u.holdResourceNum >= u.MaxHoldResourceNum)//手中资源已经到达上限
        {
            u.TransitionTo(UnitStateType.Deliver);
            return;
        }
        if (u.currentResource != resource)
        {
            u.OnExitWorkState();
            u.currentResource = resource;
        }
        if (u.mySlot == null)
        {
            Node newSlot = u.currentResource.RequestSlot(u);
            if (newSlot == null)
            {
                u.target = null;
                u.ai.ClearPath();
                u.TransitionTo(UnitStateType.Idle);
                return;
            }
            u.mySlot = newSlot;
        }

        if (Vector2.Distance(u.transform.position, u.mySlot.GetNodePosition()) < 0.4f)
        {
            u.FlipController(u.target.transform.position);
            u.anim.SetBool("Chop", true);
            u.anim.SetBool("Move", false);
        }
        else
        {
            u.anim.SetBool("Chop", false);
            u.anim.SetBool("Move", true);
            if (u.ai.GetPathFinalNode() == null || u.ai.GetPathFinalNode() != u.mySlot)
                u.MoveToDestinationFrame(u.mySlot.GetNodePosition());
        }
    }
    public void Exit()
    {
        u.OnExitWorkState();
    }
}
