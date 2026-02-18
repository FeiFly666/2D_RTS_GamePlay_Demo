using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCatalog", menuName = "DataCatalog")]
public class DataCatalog : ScriptableObject
{
    public List<BuildingAction> allBuildings = new List<BuildingAction>();
    public List<HumanAction> allHumans = new List<HumanAction>();
    public List<ResourceAction> allResources = new List<ResourceAction>();

    public HumanAction GetHumanByID(string id)
    {
        return allHumans.Find(b => b.ID == id);
    }
    public BuildingAction GetBuildingByID(string id)
    {
        return allBuildings.Find(b => b.ID == id);
    }
    public ResourceAction GetResourceByID(string id)
    {
        return allResources.Find(r => r.ID == id);
    }
}