using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnitSpawnManager : Singleton<UnitSpawnManager>
{

    public void InitAllUnitPools()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("Prefab/Units");

        foreach(var prefab in allPrefabs)
        {
            if(prefab == null) continue;

            string key = prefab.name;
            if(string.IsNullOrEmpty(key)) continue;

            Unit unit = prefab.GetComponent<Unit>();
            if(unit == null) continue;

            if(unit.unitType == UnitType.unit)
            {
                PoolManager.Instance.CreatePool(key, prefab.GetComponent<HumanUnit>(), 50, GameManager.Instance.HumanRoot);
            }
            else if(unit.unitType == UnitType.building)
            {
                PoolManager.Instance.CreatePool(key, prefab.GetComponent<BuildingUnit>(), 10, GameManager.Instance.BuildingRoot);
            }
            else
            {
                PoolManager.Instance.CreatePool(key, prefab.GetComponent<ResourceUnit>(), 10, GameManager.Instance.ResourceRoot);
            }
        }
    }
    
}
