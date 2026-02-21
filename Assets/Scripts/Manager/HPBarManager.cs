using Assets.Scripts.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HPBarManager : MonoSingleton<HPBarManager>
{
    [SerializeField] private Transform hpBarRoot;
    [SerializeField] private UIHealthBar unitHpBar;
    [SerializeField] private UIHealthBar smallBuildingHpBar;
    [SerializeField] private UIHealthBar largeBuildingHpBar;

    protected override void OnStart()
    {
        base.OnStart();
        PoolManager.Instance.CreatePool("UnitHpBar", unitHpBar, 100, hpBarRoot);
        PoolManager.Instance.CreatePool("SmallBuildingHpBar", smallBuildingHpBar, 50, hpBarRoot);
        PoolManager.Instance.CreatePool("LargeBuildingHpBar", largeBuildingHpBar, 20, hpBarRoot);
    }

    public UIHealthBar GetAnHpBar(Unit unit)
    {
        UIHealthBar healthBar = null; 
        if (unit != null)
        {
            switch(unit.hpbarType)
            {
                case HpBarType.Unit:
                    healthBar = PoolManager.Instance.Spawn<UIHealthBar>("UnitHpBar");
                    break;
                case HpBarType.SmallBuilding:
                    healthBar = PoolManager.Instance.Spawn<UIHealthBar>("SmallBuildingHpBar");
                    break;
                case HpBarType.LargeBuilding:
                    healthBar = PoolManager.Instance.Spawn<UIHealthBar>("LargeBuildingHpBar");
                    break;
            }
        }
        healthBar.RegisterOwner(unit);
        return healthBar;
    }
    public void ReturnAllBar()
    {
        PoolManager.Instance.ReturnAllToPool<UIHealthBar>("UnitHpBar");
        PoolManager.Instance.ReturnAllToPool<UIHealthBar>("SmallBuildingHpBar");
        PoolManager.Instance.ReturnAllToPool<UIHealthBar>("LargeBuildingHpBar");
    }
}
