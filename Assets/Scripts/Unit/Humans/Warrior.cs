using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : HumanUnit
{
    protected override void InitBaseBehaviors()
    {
        // 安装战士的插件
        targetSelector = new MeleeTargetSelector();
        combatBehaviour = new MeleeCombatBehaviour();
    }
    public override void PreformAttackAnimation()
    {
        AnimationFinishTrigger1();

        Vector3 aimPoint = this.GetTargetAimPoint();
        Vector2 direction = (aimPoint - detectPosition).normalized;
        //Debug.DrawLine(this.detectPosition, aimPoint, Color.yellow);

        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y) )
        {
            anim.SetBool("Attack_Horizontal", true);
        }
        else
        {
            if(direction.y > 0)
            {
                anim.SetBool("Attack_Up", true);
            }
            else
            {
                anim.SetBool("Attack_Down", true);
            }
        }
        anim.SetInteger("comboCounter", this.ComboCounter);
    }
}
