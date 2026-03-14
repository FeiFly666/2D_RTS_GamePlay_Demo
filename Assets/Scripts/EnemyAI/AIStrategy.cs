using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AIStrategy 
{
    private FactionAI AI;

    public AIStrategy(FactionAI AI)
    {
        this.AI = AI;
    }
    public BuildingAction requestBuilding;
    public HumanAction requestHuman;

    private List<HumanAction> possibleHumanActions = new List<HumanAction>();
    private List<BuildingAction> possibleBuildingActions = new List<BuildingAction>();

    private int woodBuffer = 100;
    private int goldBuffer = 70;

    public void UpdateLogic()
    {
        FactionData data = AI.faction;
        requestBuilding = null;

        //应急型建造
        if (data.BuildingTypeCount[(int)BuildingType.Static] > 0 && AI.faction.buildings.Count < AI.maxBuildingNum)
        {
            if (NeedEmergencyBuilding(data)) { return; }
        }

        //应急型训练
        if (data.BuildingTypeCount[(int)BuildingType.Train] > 0)
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

        //是否进攻
        if (!AI.CanAttack) return;
        if(AI.faction.IdleNoWorkerHumans.Count > AI.nextAttackNum && !AI.attack)
        {
            AI.prepareForAttack = true;
        }
    }
    private bool NeedEmergencyBuilding(FactionData data)
    {
        if (data.currentPeopleNum >= data.TotalPeopleNum * 0.85f && !IsBuildingTypeInConstruction(BuildingType.House))
        {
            requestBuilding = AI.buildingMap[BuildingType.House][0];
        }
        else if (data.BuildingTypeCount[(int)BuildingType.Collect] < 1 && !IsBuildingTypeInConstruction(BuildingType.Collect))
        {
            requestBuilding = AI.buildingMap[BuildingType.Collect][0];
        }
        else if (data.BuildingTypeCount[(int)BuildingType.Train] < 1 && !IsBuildingTypeInConstruction(BuildingType.Train))
        {
            requestBuilding = AI.buildingMap[BuildingType.Train][0];
        }
        else if (data.BuildingTypeCount[(int)BuildingType.Ranged] < 1 && !IsBuildingTypeInConstruction(BuildingType.Ranged) && AI.faction.IdleNoWorkerHumans.Count > 5)
        {
            requestBuilding = AI.buildingMap[BuildingType.Ranged][0];
        }
        else if (data.BuildingTypeCount[(int)BuildingType.Exchange] < 1 && AI.faction.GoldNum > 800)
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
/*        else if (data.humans.Count < 8)
        {
            requestHuman = AI.humanMap[UnitRole.Melee][0];
        }*/
        return requestHuman != null;
    }
    private void HandleNormalDevelopment(FactionData data)
    {
        if (data.BuildingTypeCount[(int)BuildingType.Train] > 0 && data.GoldNum > 100 && data.WoodNum > 100)
        {
            HandleNormalProduction(data);
        }
        
        HandleBuildingConstruction(data);
    }
    private void HandleBuildingConstruction(FactionData data)
    {
        if (!IsAnyBuildingIsInConstruction() && data.BuildingTypeCount[(int)BuildingType.Static] > 0 && AI.faction.buildings.Count < AI.maxBuildingNum)
        {
            //List<BuildingAction> possibleActions = new List<BuildingAction>();
            if (data.BuildingTypeCount[(int)BuildingType.Ranged] < 1)
            {
                BuildingAction rangedCamp = AI.buildingMap[BuildingType.Ranged][0];
                if (data.CanAfford(rangedCamp.goldCost + goldBuffer, rangedCamp.woodCost + woodBuffer))
                {
                    possibleBuildingActions.Add(rangedCamp);
                    possibleBuildingActions.Add(rangedCamp);
                    possibleBuildingActions.Add(rangedCamp);
                }
            }

            if (data.BuildingTypeCount[(int)BuildingType.Ranged] > 0 && data.BuildingTypeCount[(int)BuildingType.Attack] < AI.maxAttackBuildingNum)
            {
                BuildingAction tower = AI.buildingMap[BuildingType.Attack][0];

                int goldCost = AI.faction.BuildingTypeCount[(int)BuildingType.Attack] < 2 ? tower.goldCost + goldBuffer / 2 : tower.goldCost + goldBuffer ;
                int woodCost = AI.faction.BuildingTypeCount[(int)BuildingType.Attack] < 2 ? tower.woodCost + woodBuffer / 2 : tower.woodCost + woodBuffer ;

                if (data.CanAfford(goldCost, woodCost) && data.HasPeopleSpace(-tower.peopleAddNum))
                {
                    possibleBuildingActions.Add(tower);
                }
            }

            if (data.BuildingTypeCount[(int)BuildingType.Train] < 4)
            {
                BuildingAction barracks = AI.buildingMap[BuildingType.Train][0];
                if (data.CanAfford(barracks.goldCost + goldBuffer, barracks.woodCost + woodBuffer))
                {
                    possibleBuildingActions.Add(barracks);
                }
            }

            if (data.BuildingTypeCount[ (int)BuildingType.Collect] < 5)
            {
                BuildingAction goldMine = AI.buildingMap[BuildingType.Collect][0];
                if (data.CanAfford(goldMine.goldCost + goldBuffer, goldMine.woodCost + woodBuffer))
                {
                    possibleBuildingActions.Add(goldMine);
                }
            }


            if (possibleBuildingActions.Count > 0)
            {
                possibleBuildingActions.Add(null);
                possibleBuildingActions.Add(null);
                possibleBuildingActions.Add(null);
                possibleBuildingActions.Add(null);

                int random = Random.Range(0, possibleBuildingActions.Count);

                requestBuilding = possibleBuildingActions[random];

                possibleBuildingActions.Clear();
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

        //List<HumanAction> possibleActions = new List<HumanAction>();

        int meleeCount = data.humans.Count(h => h.role == UnitRole.Melee);
        int rangedCount = data.humans.Count(h => h.role == UnitRole.Ranged);

        if(meleeCount < rangedCount || !(data.BuildingTypeCount[(int)BuildingType.Ranged] > 0))
        {
            HumanAction melee = AI.humanMap[UnitRole.Melee][0];
            if (AI.faction.CanAfford(melee.goldCost, melee.woodCost))
                possibleHumanActions.Add(melee);
        }
        else
        {
            if(data.BuildingTypeCount[(int)BuildingType.Ranged] > 0)
            {
                HumanAction ranged = AI.humanMap[UnitRole.Ranged][0];
                if (AI.faction.CanAfford(ranged.goldCost, ranged.woodCost))
                    possibleHumanActions.Add(ranged);
            }
        }

        if (AI.faction.GetWorkerNum() < 15)
        {
            HumanAction worker = AI.humanMap[UnitRole.Worker][0];
            if (AI.faction.CanAfford(worker.goldCost, worker.woodCost))
            {
                possibleHumanActions.Add(AI.humanMap[UnitRole.Worker][0]);
                possibleHumanActions.Add(AI.humanMap[UnitRole.Worker][0]);
            }
        }

        if(possibleHumanActions.Count > 0)
        {
            possibleHumanActions.Add(null);
            possibleHumanActions.Add(null);
            possibleHumanActions.Add(null);

            int random = Random.Range(0, possibleHumanActions.Count);

            requestHuman = possibleHumanActions[random];

            possibleHumanActions.Clear();
        }

    }
    public bool IsAnyEmergency(FactionData faction)
    {
        return NeedEmergencyBuilding(faction) || NeedEmergencyTraining(faction);
    }
    private bool IsBuildingTypeInConstruction(BuildingType buildingType)
    {
        foreach(var building in AI.faction.buildings)
        {
            if(building.buildingType == buildingType && building.buildingState == BuildingState.InConstruction)
            {
                return true;
            }
        }
        return false;
        //return AI.faction.buildings.Any(b => b.buildingType == buildingType && b.buildingState == BuildingState.InConstruction);
    }
    private bool IsAnyBuildingIsInConstruction()
    {
        foreach(var building in AI.faction.buildings)
        {
            if(building.buildingState == BuildingState.InConstruction)
            {
                return true;
            }
        }
        return false;
        //return AI.faction.buildings.Any(b => b.buildingState == BuildingState.InConstruction);
    }
    private bool IsAnyGoldMineEmpty()
    {
        foreach(var goldMine in AI.faction.goldMines)
        {
            if(goldMine.humanInsideData.Count < goldMine.maxUnitNum)
            {
                return true;
            }
        }
        return false;

        //return AI.faction.goldMines.Any(g => g.humanInsideData.Count < g.maxUnitNum);
    }
}
