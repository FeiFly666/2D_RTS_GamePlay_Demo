using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBehaviourInterface 
{
    public interface ITargetSelector
    {
        public Unit SetNewTarget(HumanUnit self);
        public bool IsTargetReachable(HumanUnit self, Unit targetUnit, bool isForced = false);
    }
    public interface ICombatBehaviour
    {
        bool CanAttack(HumanUnit self, Unit target);
        void ExecuteAttack(HumanUnit self, Unit target);
        void UpdateAttack(HumanUnit self, Unit target);
    }
}
