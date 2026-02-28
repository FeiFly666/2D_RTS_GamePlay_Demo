using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        if (data.BuildingTypeCount[BuildingType.Static] > 0)
        {
            if (NeedEmergencyBuilding(data)) { return; }
        }

        //应急型训练
        if (data.BuildingTypeCount[BuildingType.Train] >=1)
        {
            if(NeedEmergencyTraining(data)) { return; }
        }
        //正常发展
        HandleBuildingConstruction(data);

        if (requestBuilding == null)//没有需要的建筑造兵
        {
            if (data.GoldNum > 100 && data.WoodNum > 100)
                HandleNormalProduction(data);
        }
        else//lz很富有，可以边造兵边建建筑
        {
            if (data.GoldNum > (requestBuilding.goldCost + goldBuffer * 3) && data.WoodNum > (requestBuilding.woodCost + woodBuffer * 3))
            {
                HandleNormalDevelopment(data);
            }
        }

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
        else if (data.BuildingTypeCount[BuildingType.Exchange] < 1 && AI.faction.GoldNum > 800)
        {
            requestBuilding = AI.buildingMap[BuildingType.Exchange][0];
        }

        return requestBuilding != null;
    }
    private bool NeedEmergencyTraining(FactionData data)
    {
        if (data.GetWorkerNum() < 5)
        {
            requestHuman = AI.humanMap[UnitRole.Worker][0];
        }
        else if (IsAnyGoldMineEmpty())
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }
        else if (data.humans.Count < 8)
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }
        return requestHuman != null;
    }
    private void HandleNormalDevelopment(FactionData data)
    {
        if (data.BuildingTypeCount[BuildingType.Train] > 0 && data.GoldNum > 100 && data.WoodNum > 100)
        {
            HandleNormalProduction(data);
        }

        HandleBuildingConstruction(data);
    }
    private void HandleBuildingConstruction(FactionData data)
    {
        if (!IsAnyBuildingIsInConstruction() && data.BuildingTypeCount[BuildingType.Static] > 0)
        {
            List<BuildingAction> possibleActions = new List<BuildingAction>();
            if (data.BuildingTypeCount[BuildingType.Ranged] < 1)
            {
                BuildingAction rangedCamp = AI.buildingMap[BuildingType.Ranged][0];
                if (data.CanAfford(rangedCamp.goldCost + goldBuffer, rangedCamp.woodCost + woodBuffer))
                {
                    possibleActions.Add(rangedCamp);
                }
            }

            if (data.BuildingTypeCount[BuildingType.Ranged] > 0)
            {
                BuildingAction tower = AI.buildingMap[BuildingType.Attack][0];

                if (data.CanAfford(tower.goldCost + goldBuffer / 2, tower.woodCost + woodBuffer / 2) && data.HasPeopleSpace(-tower.peopleAddNum))
                {
                    possibleActions.Add(tower);
                }
            }

            if (data.BuildingTypeCount[BuildingType.Train] < 3)
            {
                BuildingAction barracks = AI.buildingMap[BuildingType.Train][0];
                if (data.CanAfford(barracks.goldCost + goldBuffer, barracks.woodCost + woodBuffer))
                {
                    possibleActions.Add(barracks);
                }
            }

            if (data.BuildingTypeCount[BuildingType.Collect] < 3)
            {
                BuildingAction goldMine = AI.buildingMap[BuildingType.Collect][0];
                if (data.CanAfford(goldMine.goldCost + goldBuffer, goldMine.woodCost + woodBuffer))
                {
                    possibleActions.Add(goldMine);
                }
            }


            if (possibleActions.Count > 0)
            {
                possibleActions.Add(null);
                possibleActions.Add(null);
                possibleActions.Add(null);
                possibleActions.Add(null);

                int random = Random.Range(0, possibleActions.Count);

                requestBuilding = possibleActions[random];
            }
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

        List<HumanAction> possibleActions = new List<HumanAction>();

        int meleeCount = data.humans.Count(h => h.role == UnitRole.Melee);
        int rangedCount = data.humans.Count(h => h.role == UnitRole.Ranged);

        if(meleeCount < rangedCount || !(data.BuildingTypeCount[BuildingType.Ranged] > 0))
        {
            HumanAction melee = AI.humanMap[UnitRole.Melee][0];
            if (AI.faction.CanAfford(melee.goldCost + goldBuffer / 2, melee.woodCost + woodBuffer / 2))
                possibleActions.Add(melee);
        }
        else
        {
            if(data.BuildingTypeCount[BuildingType.Ranged] > 0)
            {
                HumanAction ranged = AI.humanMap[UnitRole.Ranged][0];
                if (AI.faction.CanAfford(ranged.goldCost + goldBuffer / 2, ranged.woodCost + woodBuffer / 2))
                    possibleActions.Add(ranged);
            }
        }

        if (AI.faction.GetWorkerNum() < 8)
        {
            HumanAction worker = AI.humanMap[UnitRole.Worker][0];
            if (AI.faction.CanAfford(worker.goldCost + goldBuffer / 2, worker.woodCost + woodBuffer / 2))
            {
                possibleActions.Add(AI.humanMap[UnitRole.Worker][0]);
                possibleActions.Add(AI.humanMap[UnitRole.Worker][0]);
            }
        }

        if(possibleActions.Count > 0)
        {
            possibleActions.Add(null);
            possibleActions.Add(null);
            possibleActions.Add(null);

            int random = Random.Range(0, possibleActions.Count);

            requestHuman = possibleActions[random];
        }

    }
    public bool IsAnyEmergency(FactionData faction)
    {
        return NeedEmergencyBuilding(faction) || NeedEmergencyTraining(faction);
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
