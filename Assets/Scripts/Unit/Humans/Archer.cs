using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : HumanUnit
{
    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] public float arrowFlySpeed = 15f;
    
    private Vector3 GetPredictedPosition(Unit target, float projectSpeed)
    {
        Vector3 targetVelocity = Vector3.zero;

        if(target != null)
        {
            if (target is BuildingUnit) return GetTargetAimPoint();
            if(target is HumanUnit human)
            {
                targetVelocity = human.CurrentVelocity;
                if (targetVelocity.magnitude < 0.05f) targetVelocity = Vector3.zero;
            }
            float distance = Vector3.Distance(this.detectPosition, target.detectPosition);
            float lookAheadTime = distance / projectSpeed;

            Vector3 predictedPos = target.detectPosition + targetVelocity * lookAheadTime;

            return predictedPos;
        }

        return Vector3.zero;
    }
    protected override void InitBaseBehaviors()
    {
        targetSelector = new MeleeTargetSelector();
        combatBehaviour = new RangeCombatBehaviour();
    }
    public override void PreformAttackAnimation()
    {
        AnimationFinishTrigger2();

        Vector3 aimPos = GetPredictedPosition(target, arrowFlySpeed);
        Vector2 direction = (aimPos - detectPosition).normalized;

        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;

        if (angle > 67.5f)
        {
            anim.SetBool("Attack_Up", true);
        }
        else if (angle > 24.5f)
        {
            anim.SetBool("Attack_SlantedUp", true);
        }
        else if (angle > -21.5f)
        {
            anim.SetBool("Attack_Horizontal", true);
        }
        else if (angle > -67.5f) 
        {
            anim.SetBool("Attack_SlantedDown", true);
        }
        else
        {
            anim.SetBool("Attack_Down", true);
        }
    }
    public void LaunchArrow()
    {
        Vector3 aimPos = GetPredictedPosition(target, arrowFlySpeed);
        Vector2 direction = (aimPos - detectPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion rotate = Quaternion.Euler(0, 0, angle);

        //GameObject newArrow = Instantiate(arrowPrefab, detectPosition, rotate);

        Arrow newArrow = PoolManager.Instance.Spawn<Arrow>("Arrow");

        newArrow.transform.SetPositionAndRotation(detectPosition,Quaternion.Euler(0f, 0f, angle));

        newArrow.GetComponent<Arrow>().ReigsterArrow(this, this.target);
    }
}
