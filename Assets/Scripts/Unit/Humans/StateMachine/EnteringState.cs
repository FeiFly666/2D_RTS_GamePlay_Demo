using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnteringState : IUnitState
{
    private HumanUnit u;

    private Vector2 targetPos;

    public EnteringState(HumanUnit u) { this.u = u;}

    public void Enter()
    {
        u.anim.SetBool("Move", true);

        targetPos = u.target.transform.position + (Vector3) (u.target as GoldMine).spawnPosition;

        u.MoveToDestinationFrame(targetPos);
    }
    public void Update()
    {
        if(u.target is not GoldMine mine || u.target.isDead || !mine.CanInside)
        {
            u.UnassignTarget();
            u.TransitionTo(UnitStateType.Idle);
            return;
        }
        float sqrDist = (u.transform.position - (Vector3)targetPos).sqrMagnitude;
        if (sqrDist < 0.25f)
        {
            bool success = mine.InsideBuilding(u);
            if (success)
            {
                u.ai.LeaveGroup();
                u.Death();
                return;
            }
            else
            {
                u.UnassignTarget();
                u.TransitionTo(UnitStateType.Idle);
            }
        }
        else
        {
            if (!u.ai.IsPathVaild() && !u.ai.isWaitingPath)
            {
                u.MoveToDestinationFrame(targetPos);
            }
        }
    }

    public void Exit()
    {
        u.anim.SetBool("Move", false);
    }

    
}
