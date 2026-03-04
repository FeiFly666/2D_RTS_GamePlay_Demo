using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DeliverState :IUnitState
{
    private Worker u;
    public DeliverState(Worker unit) { this.u = unit; }

    public void Enter()
    {
        u.OnExitWorkState();
        u.woodGO.SetActive(true);
        u.anim.SetBool("Deliver", true);
    }
    public void Update()
    {
        BuildingUnit nearestBase = PrepareToDeliver();
        if (nearestBase == null)
        {
            u.TransitionTo(UnitStateType.Idle);
            return;
        }

        u.target = nearestBase;
        u.targetID = nearestBase.uniqueID;
        if(!u.anim.GetBool("Move"))
            u.anim.SetBool("Move", true);

        Node targetNode = TilemapManager.Instance.GetClosestInteractableNode(u.target, u.transform.position, u.gameObject);
        if (targetNode == null) return;

        Vector3 targetPos = targetNode.GetNodePosition();

        float dis = (targetPos - u.transform.position).sqrMagnitude;

        if (dis > 1f)
        {
            //if (u.ai.GetPathFinalNode() == null || u.ai.GetPathFinalNode().GetNodePosition() != targetPos)
            if(!u.ai.IsPathVaild())
                u.MoveToDestinationFrame(targetPos);
        }
        else
        {
            u.AddWoodToFaction();

            u.woodGO.SetActive(false);
            u.anim.SetBool("Deliver", false);

            u.target = null;
            u.targetID = -1;

            u.TransitionTo(UnitStateType.Idle);
        }
    }

    private BuildingUnit PrepareToDeliver()
    {
        u.OnExitWorkState();
        BuildingUnit nearestBase = u.faction.GetNearestAllyBase(u);

        return nearestBase;
    }
    public void Exit()
    {
        u.anim.SetBool("Move", false);
    }
}