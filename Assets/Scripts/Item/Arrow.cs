using Assets.Scripts.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Arrow : MonoBehaviour , IPoolable
{
    [Header("基本参数")]
    [SerializeField] private float FlySpeed = 10f;
    [SerializeField] private float detectedRadius = 0.2f;
    private Unit owner;
    private Unit target;

    [SerializeField]private float DestroyTime = 3f;
    private float destroyTimer = 0f;

    private float damage;
    public void ReigsterArrow(Unit owner, Unit target)
    {
        this.owner = owner;
        this.target = target;
        this.destroyTimer = Time.time;
        if(target != null )
        {
            this.damage = owner.stats.CalculateDamage(target.stats);
        }
        this.FlySpeed = (owner as Archer).arrowFlySpeed;

    }
    public void OnSpawn()
    {
        destroyTimer = 0f;
    }
    // Update is called once per frame
    void Update()
    {

        if (target == null || owner == null)
        {
            //Destroy(this.gameObject); 
            Despawn();
            return;
        }

        this.transform.position += transform.right * FlySpeed * Time.deltaTime;

        CheckHit();

        if (Time.time - destroyTimer >= DestroyTime)
        {
            //Destroy(this.gameObject);
            Despawn();
        }

    }
    private void CheckHit()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(this.transform.position, detectedRadius);

        if (hit != null)
        {
            foreach(var unit in hit)
            {
                if (unit.GetComponent<Unit>() == target)
                {
                    ApplyDamage();
                }
            }
        }
    }
    private void ApplyDamage()
    {
        if(target != null)
            target.stats.DecreaseHP(this.owner.stats, this.damage);
        //Destroy(this.gameObject);
        Despawn();
    }
    public void OnDespawn()
    {
        owner = null;
        target = null;
    }
    private void Despawn()
    {
        PoolManager.Instance.Despawn("Arrow", this);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectedRadius);
    }
}
