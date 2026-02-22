using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HumanBehaviourInterface;
using static UnityEngine.GraphicsBuffer;

public class AttackState :IUnitState
{
    private HumanUnit u;
    public AttackState(HumanUnit unit) { this.u = unit; }

    public void Enter()
    {
        u.ai.ClearPath();
    }
    public void Update()
    {
        if (u.combatBehaviour != null)
        {
            if (!u.combatBehaviour.CanAttack(u, u.target))
            {
                u.AnimationFinishTrigger2();
                if (u.isBuildingUnit)
                {
                    u.TransitionTo(UnitStateType.Idle);
                    return;
                }
                else
                {
                    u.TransitionTo(UnitStateType.Move);
                }
                return;
            }

            u.ai.ClearPath();
            u.FlipController(u.GetTargetAimPoint());

            u.PreformAttackAnimation();

            if (!u.isForcingTarget)
            {
                Unit betterTarget = u.targetSelector?.SetNewTarget(u);
                if (betterTarget != null && betterTarget != u.target)
                {
                    u.target = betterTarget;
                    u.targetID = betterTarget.uniqueID;
                    u.lastTargetInDetectionTime = Time.time;
                }
            }
        }
    }
    public void Exit()
    {
        u.AnimationFinishTrigger2();
    }
}
