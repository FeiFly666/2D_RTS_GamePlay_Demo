using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIStrategy 
{
    private FactionAI AI;

    public AIStrategy(FactionAI AI)
    {
        this.AI = AI;
    }
    public BuildingAction requestBuilding;

    public void UpdateLogic()
    {
        FactionData data = AI.faction;
        requestBuilding = null;

        if(data.currentPeopleNum >= data.TotalPeopleNum * 0.8f && !IsBuildingTypeInConstruction(BuildingType.House))
        {
            requestBuilding = AI.actionList.Find(b => (b as BuildingAction).structurePrefab.GetComponent<BuildingUnit>().buildingType == BuildingType.House) as BuildingAction;
        }
        else if (data.BuildingTypeCount[BuildingType.Collect] < 1)
        {
            requestBuilding = AI.actionList.Find(b => (b as BuildingAction).structurePrefab.GetComponent<BuildingUnit>().buildingType == BuildingType.Collect) as BuildingAction;
        }
        else if (data.BuildingTypeCount[BuildingType.Train] < 1)
        {
            requestBuilding = AI.actionList.Find(b => (b as BuildingAction).structurePrefab.GetComponent<BuildingUnit>().buildingType == BuildingType.Train) as BuildingAction;

        }

    }
    private bool IsBuildingTypeInConstruction(BuildingType buildingType)
    {
        return AI.faction.buildings.Any(b => b.buildingType == buildingType && b.buildingState == BuildingState.InConstruction);
    }
}
