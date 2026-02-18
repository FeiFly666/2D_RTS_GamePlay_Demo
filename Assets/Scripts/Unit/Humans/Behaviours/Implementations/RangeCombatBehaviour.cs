using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static HumanBehaviourInterface;


public class RangeCombatBehaviour : ICombatBehaviour
{
    public bool CanAttack(HumanUnit self, Unit target)
    {
        if (target == null || target.isDead) return false;

        IUnitState currentState = self.stateMachine.CurrentState;
        if (target is HumanUnit)
        {
            float distance = Vector2.Distance(target.detectPosition, self.detectPosition);

            float enterRange = self.attackRadius - 1.5f;
            float exitRange = self.attackRadius;

            return (currentState is AttackState) ? distance <= exitRange : distance <= enterRange;
        }
        else
        {
            float dist = Vector2.Distance(self.GetTargetAimPoint(), self.detectPosition);
            float enterRange = self.attackRadius - 1.0f;
            float exitRange = self.attackRadius;
            return (currentState is AttackState) ? dist <= exitRange : dist <= enterRange;
        }

    }
    public void ExecuteAttack(HumanUnit self, Unit target)
    {
        self.TransitionTo(UnitStateType.Attack);
        if (self.ai.IsPathVaild()) self.ai.ClearPath();
        self.FlipController(self.GetTargetAimPoint());
        self.PreformAttackAnimation();
    }
    public void UpdateAttack(HumanUnit self, Unit target)
    {
        self.FlipController(self.GetTargetAimPoint());
    }
}

