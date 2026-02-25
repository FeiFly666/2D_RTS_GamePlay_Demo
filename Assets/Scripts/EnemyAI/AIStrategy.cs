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
    public HumanAction requestHuman;

    private int woodBuffer = 100;
    private int goldBuffer = 70;

    public void UpdateLogic()
    {
        FactionData data = AI.faction;
        requestBuilding = null;

        //应急型建造
        if(NeedEmergencyBuilding(data)) { return; }

        //应急型训练
        if (data.BuildingTypeCount[BuildingType.Train] >=1)
        {
            if(NeedEmergencyTraining(data)) { return; }
        }
        //正常发展
        HandleNormalDevelopment(data);

    }
    private bool NeedEmergencyBuilding(FactionData data)
    {
        if (data.currentPeopleNum >= data.TotalPeopleNum * 0.8f && !IsBuildingTypeInConstruction(BuildingType.House))
        {
            requestBuilding = AI.buildingMap[BuildingType.House][0];
        }
        else if (data.BuildingTypeCount[BuildingType.Collect] < 1 && !IsBuildingTypeInConstruction(BuildingType.Collect))
        {
            requestBuilding = AI.buildingMap[BuildingType.Collect][0];
        }
        else if (data.BuildingTypeCount[BuildingType.Train] < 1 && !IsBuildingTypeInConstruction(BuildingType.Train))
        {
            requestBuilding = AI.buildingMap[BuildingType.Train][0];
        }

        return requestBuilding != null;
    }
    private bool NeedEmergencyTraining(FactionData data)
    {
        if (AI.GetCollectWorkerNum() < 2 || data.workers.Count < 5)
        {
            requestHuman = AI.humanMap[UnitRole.Worker][0];
        }
        else if (IsAnyGoldMineEmpty())
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }
        else if (data.humans.Count < 10)
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }
        return requestHuman != null;
    }
    private void HandleNormalDevelopment(FactionData data)
    {
        if(!IsAnyBuildingIsInConstruction())
        {
            if (data.BuildingTypeCount[BuildingType.Ranged] < 1)
            {
                BuildingAction rangedCamp = AI.buildingMap[BuildingType.Ranged][0];
                if (data.CanAfford(rangedCamp.goldCost + goldBuffer, rangedCamp.woodCost + woodBuffer))
                {
                    requestBuilding = rangedCamp;
                    return;
                }
            }

            float randomIndex = Random.Range(0, 100f);

            if(randomIndex <= 50f)
            {
                if (data.BuildingTypeCount[BuildingType.Attack] < 10)//最多造10个
                {
                    BuildingAction tower = AI.buildingMap[BuildingType.Attack][0];

                    if (data.CanAfford(tower.goldCost + goldBuffer, tower.woodCost + woodBuffer) && data.HasPeopleSpace(-tower.peopleAddNum + 3))
                    {
                        requestBuilding = tower;
                        return;
                    }
                }
            }

            if (data.BuildingTypeCount[BuildingType.Train] < 3 && data.GoldNum > 500 && data.WoodNum > 300)
            {
                BuildingAction barracks = AI.buildingMap[BuildingType.Train][0];
                requestBuilding = barracks;
                return;
            }

        }
        if (data.GoldNum > 100 && data.WoodNum > 200)
        {
            HandleNormalProduction(data);
        }
    }
    private void HandleNormalProduction(FactionData data)
    {
        int allTrainWeight = 0;
        foreach(var train in AI.faction.trainings)
        {
            allTrainWeight += train.queuePopWeight;
        }
        if (!data.HasPeopleSpace(allTrainWeight + 1)) return;

        int meleeCount = data.humans.Count(h => h.role == UnitRole.Melee);
        int rangedCount = data.humans.Count(h => h.role == UnitRole.Ranged);

        if (data.BuildingTypeCount[BuildingType.Ranged] < 1)
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }
        else
        {
            if (meleeCount <= rangedCount)
                requestHuman = AI.humanMap[UnitRole.Melee][0];
            else
                requestHuman = AI.humanMap[UnitRole.Ranged][0];
        }
    }
    private bool IsBuildingTypeInConstruction(BuildingType buildingType)
    {
        return AI.faction.buildings.Any(b => b.buildingType == buildingType && b.buildingState == BuildingState.InConstruction);
    }
    private bool IsAnyBuildingIsInConstruction()
    {
        return AI.faction.buildings.Any(b => b.buildingState == BuildingState.InConstruction);
    }
    private bool IsAnyGoldMineEmpty()
    {
        return AI.faction.goldMines.Any(g => g.humanInsideData.Count < g.maxUnitNum);
    }
}
