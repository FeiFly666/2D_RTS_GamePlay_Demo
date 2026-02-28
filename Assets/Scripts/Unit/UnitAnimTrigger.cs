using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimTrigger : MonoBehaviour
{
    protected Unit owner => GetComponentInParent<Unit>();
    public void AnimCounterRandomAdd() => (owner as HumanUnit).AnimationCounterRandomAdd();

    private static Collider2D[] scanBuffer = new Collider2D[200];

    private void AttackTrigger()
    {
        HumanUnit owner = this.owner as HumanUnit;
        if (owner == null) return;

        int num = Physics2D.OverlapCircleNonAlloc(owner.detectPosition, owner.attackRadius, scanBuffer, GameManager.EnemyMasks[(int)owner.unitSide]);

        if (owner.target is ResourceUnit) return;

        for (int i = 0; i < num; i++)
        {
            Collider2D hit = scanBuffer[i];
            if(hit.TryGetComponent(out Unit enemy) && enemy.unitSide != owner.unitSide)
            {
                if(enemy.isDead) { return; }
                
                if(owner.isAOE)
                {
                    owner.stats.AttackTarget(enemy.GetComponent<UnitStats>());
                }
                else
                {
                    if(enemy == owner.target)
                    {
                        owner.stats.AttackTarget(enemy.GetComponent<UnitStats>());
                    }
                }
            }
        }
    }

    private void LaunchArrow() => (owner as Archer).LaunchArrow();

    private void DestroyUnit() => owner.DestroyUnit();

    private void GatherResource() => (owner as Worker).GatherResource();

    private void DestroyResource() => (owner as ResourceUnit).ResourceChopper();
}
