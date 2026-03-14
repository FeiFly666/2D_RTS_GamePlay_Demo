using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    private Unit unit => GetComponent<Unit>();
    [Header("µ•Œª Ù–‘")]
    public int FullHP;
    public int Attack;
    public int Armor;
    public int CritChance;
    public float CritMutiple;

    public float currentHP;
    public System.Action OnHealthChanged;
    public System.Action<Unit> OnHurtByUnits;

    private void Start()
    {
        //currentHP = FullHP;
    }

    public void AttackTarget(UnitStats target)
    {
        HumanUnit unit = this.unit as HumanUnit;

        target.DecreaseHP(this, CalculateDamage(target));
    }

    public float CalculateDamage(UnitStats enemy)
    {
        float damage = Attack;

        if(Random.Range(0,100) <= CritChance)
        {
            damage *= (1 + CritMutiple);
        }

        damage -= enemy.Armor * 0.5f;
        damage = Mathf.Max(damage, 0.1f);
        return damage;
    }

    public void DecreaseHP(UnitStats attacker, float damage)
    {
        currentHP -= damage;
        OnHealthChanged?.Invoke();
        if(attacker != null && attacker.unit != null)
        {
            OnHurtByUnits?.Invoke(attacker.unit);
        }

        if (currentHP <= 0 && !unit.isDead)
        {
            this.Death();
        }

        if (currentHP <= FullHP * 0.67f && attacker != null)
        {
            if(unit is HumanUnit human && !human.isBuildingUnit)
            {
                if(human.stateMachine.CurrentState is IdleState && !human.IsForcingMoving && !human.isForcingTarget)
                {
                    Unit lastAttacker = attacker.unit;
                    if(lastAttacker is HumanUnit humanAttacker)
                    {
                        if(humanAttacker.isBuildingUnit)
                        {
                            BuildingUnit theBuilding = humanAttacker.GetComponentInParent<BuildingUnit>();
                            if(theBuilding != null)
                            {
                                lastAttacker = theBuilding;
                            }
                            else
                            {
                                lastAttacker = null;
                            }
                        }
                    }
                    human.lastAttacker = lastAttacker;
                    human.lastAttackTime = Time.time;
                }
            }
        }
    }
    public void IncreaseHP(float healAmount)
    {
        if(currentHP >= FullHP) { return;  }
        currentHP += healAmount;
        if(currentHP > FullHP)
        {
            currentHP = FullHP;
        }

        OnHealthChanged?.Invoke();
    }
    public bool IsFullHP => currentHP == FullHP;
    public void Death()
    {
        unit.Death();
    }
}
