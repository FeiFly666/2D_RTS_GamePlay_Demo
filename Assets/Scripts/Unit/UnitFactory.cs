using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UnitFactory
{
    public static BuildingUnit CreateBuilding(BuildingAction action, Vector3 spawnPoint)
    {
        GameObject buildingGo = GameObject.Instantiate(action.structurePrefab, spawnPoint, Quaternion.identity);

        var building = buildingGo.GetComponent<BuildingUnit>();

        //building.transform.parent = GameManager.Instance.BuildingParent;

        if (!GameManager.Instance.buildings.Contains(building))
            GameManager.Instance.buildings.Add(building);
        

        building.InitFoundation();

        return building;
    }

    public static HumanUnit CreateHuman(HumanAction action, Vector3 spawnPoint)
    {
        GameObject humanGo = GameObject.Instantiate(action.humanPrefab, spawnPoint, Quaternion.identity);

        var human = humanGo.GetComponent<HumanUnit>();

        //human.transform.parent = GameManager.Instance.HumanParent;

        if (!GameManager.Instance.liveHumanUnits.Contains(human))
            GameManager.Instance.liveHumanUnits.Add(human);

        return human;
    }
    public static ResourceUnit CreatResource(ResourceAction action, Vector3 spawnPoint)
    {
        GameObject resourceGo = GameObject.Instantiate(action.resourcePrefab, spawnPoint, Quaternion.identity);

        var resource = resourceGo.GetComponent<ResourceUnit>();

        //resource.transform.parent = GameManager.Instance.ResourceParent;

        if (!GameManager.Instance.resources.Contains(resource))
        {
            GameManager.Instance.resources.Add(resource);
        }

        return resource;
    }
}
