using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;
using static UnityEngine.GraphicsBuffer;

public class MoveState : IUnitState
{
    private HumanUnit u;
    public MoveState(HumanUnit unit) { this.u = unit; }

    public void Enter()
    {
        u.anim.SetBool("Move", true);
    }
    public void Update()
    {
        if (u.ai.isForcingMoving)
        {
            if (Vector2.Distance(u.transform.position, u.ai.GetDestination()) < 0.1f)
            {
                u.ai.isForcingMoving = false;
                u.TransitionTo(UnitStateType.Idle);
            }
            return;
        }

        if (u.target != null)
        {
            if (!u.isForcingTarget && u is not Worker)
            {
                Unit betterTarget = u.targetSelector?.SetNewTarget(u);
                if (betterTarget != null && !betterTarget.isDead)
                {
                    u.target = betterTarget;
                    u.targetID = u.target.uniqueID;
                    u.lastTargetInDetectionTime = Time.time;
                }

            }
            if (u.target is BuildingUnit building && building.unitSide == u.unitSide)//工人可以建造/修理建筑
            {
                if (u.combatBehaviour.CanAttack(u, u.target))
                {
                    if (u.ai.IsPathVaild()) { u.ai.ClearPath(); }
                    u.TransitionTo(UnitStateType.Work);
                    return;
                }
            }
            if (u.target is ResourceUnit resource)
            {
                if (u.IsTargetDetected(resource))
                {
                    u.TransitionTo(UnitStateType.Work);
                    return;
                }
            }
            if (u.combatBehaviour.CanAttack(u, u.target))
            {
                if(u.role != UnitRole.Ranged || u.target.transform.position.y - u.transform.position.y > 2)
                {
                    if (!u.IsTargetNoBlock())
                    {
                        ExecuteChaseLogic();
                        return;
                    }
                }
                u.combatBehaviour.ExecuteAttack(u, u.target);
                return;
            }
            ExecuteChaseLogic();
        }
        else
        {
            if (!u.ai.IsPathVaild() && !u.ai.isWaitingPath)
            {
                u.ai.ClearPath();
                u.TransitionTo(UnitStateType.Idle);
            }
        }
    }
    private void ExecuteChaseLogic()
    {
        UpdateChasingTimeOut();
        if (u.CanRequestNewChasingPath())
        {
            u.pathFoundTimer = Time.time;
            if (u.target != null && !u.target.isDead)
            {
                if (u.ai.IsUnitInGroup)
                {
                    if (u.ai.currentGroup.leader == u)
                        u.ai.currentGroup.UpdateGroupChase();
                    return;
                }
                Vector3 currentTargetPos = u.target.transform.position;
                if (u.ai.IsPathVaild())
                {
                    if (Vector2.Distance(currentTargetPos, u.lastEnemyPos) > .2f)
                    {
                        u.RequestNewPath(currentTargetPos);
                    }
                }
                else
                {
                    u.RequestNewPath(currentTargetPos);
                }
            }
        }
    }
    private void UpdateChasingTimeOut()
    {
        if(u.IsTargetDetected(u.target)|| u.isForcingTarget) u.lastTargetInDetectionTime = Time.time;

        if(Time.time - u.lastTargetInDetectionTime >= 5f)
        {
            u.GiveUpChasingTarget();
        }
    }
    public void Exit()
    {
        u.anim.SetBool("Move", false);
    }
}
