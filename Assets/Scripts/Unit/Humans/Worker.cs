using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : HumanUnit
{
    public BuildingUnit currentWorkBuilidng;
    public ResourceUnit currentResource;
    [SerializeField] public GameObject woodGO;

    public int ResourceAreaID = -1;

    public int ChopNum = 2;
    public int currentChopNum = 0;

    public int MaxHoldResourceNum = 24;

    public int holdResourceNum = 0;

    public Node mySlot = null;
    

    private void PrepareToDeliver()
    {
        OnExitWorkState();
        BuildingUnit nearestBase = GameManager.Instance.GetNearestAllyBase(this);

        if(nearestBase == null)
        {
            this.stateMachine.Change(UnitStateType.Idle);
            return;
        }

        target = nearestBase;
        targetID = nearestBase.uniqueID;

        this.stateMachine.Change(UnitStateType.Deliver);

    }
    public override void OnExitWorkState()
    {
        if(currentWorkBuilidng != null)
        {
            currentWorkBuilidng.GetBuildingProcess()?.RemoveWorker(this);

            currentWorkBuilidng = null;

            target = null;
        }
        if(currentResource != null)
        {
            currentResource.ReleaseSlot(this);
        }
        AnimationFinishTrigger3();
    }
    public override void Death()
    {
        OnExitWorkState();
        base.Death();
    }
    protected override void InitBaseBehaviors()
    {
        targetSelector = new WorkerTargetSelector();
        combatBehaviour = new MeleeCombatBehaviour();
    }
    public override void PreformAttackAnimation()
    {
        AnimationFinishTrigger1();

        anim.SetBool("Attack_Horizontal", true);
        anim.SetInteger("comboCounter", this.ComboCounter);
    }
    
    public void GatherResource()
    {
        if(target is ResourceUnit resource)
        {
            currentChopNum++;
            if(currentChopNum == ChopNum)
            {
                if (resource.resourceLeftNum > 0) 
                {
                    resource.resourceLeftNum--;
                    holdResourceNum++;
                }
                currentChopNum = 0;
            }
        }
    }
}
