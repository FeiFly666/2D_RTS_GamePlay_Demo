using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BuildingFactory
{
    public static BuildingUnit CreateBuilding(BuildingAction action, Vector3 spawnPoint)
    {
        GameObject buildingGo = GameObject.Instantiate(action.structurePrefab, spawnPoint, Quaternion.identity);

        var building = buildingGo.GetComponent<BuildingUnit>();

        building.InitFoundation();

        return building;
    }
}
