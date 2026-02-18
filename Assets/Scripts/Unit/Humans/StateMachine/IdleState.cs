using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;
using static UnityEngine.GraphicsBuffer;

public class IdleState : IUnitState
{
    private HumanUnit u;

    public IdleState(HumanUnit unit) { this.u = unit; }
    public void Enter()
    {
        //u.ai.ClearPath();
    }
    public void Update()
    {
        if (u.isBuildingUnit)
        {
            Unit target = u.targetSelector?.SetNewTarget(u);
            if (target != null && !target.isDead)
            {
                u.target = target;
                u.targetID = target.uniqueID;
                u.lastTargetInDetectionTime = Time.time;
            }
            if (u.combatBehaviour.CanAttack(u, u.target))
            {
                u.TransitionTo(UnitStateType.Attack);
            }
            return;
        }

        if (u.isForcingTarget && u.target != null)
        {
            u.TransitionTo(UnitStateType.Move);
            return;
        }

        if (u.ai.IsPathVaild())
        {
            u.TransitionTo(UnitStateType.Move);
            return;
        }

        if (u.ai.isForcingMoving)
        {
            u.ai.isForcingMoving = false;
            return;
        }
        u.target = u.targetSelector?.SetNewTarget(u);
        if (u.target != null && !u.target.isDead)
        {
            u.targetID = u.target.uniqueID;
            u.TransitionTo(UnitStateType.Move);
            u.lastTargetInDetectionTime += Time.time;
        }
        else
        {
            if (u is Worker worker)
            {
                if (worker.holdResourceNum > 0)
                {
                    u.TransitionTo(UnitStateType.Deliver);
                }
            }
        }

    }

    public void Exit()
    {

    }

}
