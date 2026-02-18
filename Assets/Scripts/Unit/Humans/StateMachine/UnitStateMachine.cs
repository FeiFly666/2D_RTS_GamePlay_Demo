using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum UnitStateType
{
    Idle,
    Move,
    Attack,
    Work,
    Deliver
}
public class UnitStateMachine
{
    private HumanUnit owner;

    public IUnitState CurrentState { get; private set; }
    private Dictionary<UnitStateType, IUnitState> states;

    public UnitStateMachine(HumanUnit human)
    {
        this.owner = human;
        InitStates();
    }

    private void InitStates()
    {
        states = new Dictionary<UnitStateType, IUnitState>();
        states[UnitStateType.Idle] = new IdleState(owner);
        states[UnitStateType.Move] = new MoveState(owner);
        states[UnitStateType.Attack] = new AttackState(owner);
        if(owner is Worker worker)
        {
            states[UnitStateType.Work] = new WorkState(worker);
            states[UnitStateType.Deliver] = new DeliverState(worker);
        }
    }

    public void Update()
    {
        CurrentState?.Update();
    }
    public void Change(UnitStateType stateType)
    {
        if (CurrentState == states[stateType]) return;
        if(owner.isBuildingUnit)
        {
            if (stateType != UnitStateType.Idle && stateType != UnitStateType.Attack) return;
        }

        CurrentState?.Exit();

        CurrentState = states[stateType];

        CurrentState?.Enter();
    }
}
