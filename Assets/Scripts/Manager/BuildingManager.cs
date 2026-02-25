using Common;
using System.Collections;
using UnityEngine;

public class BuildingManager : Singleton<BuildingManager>
{
    private PlacementProcess placementProcess;
    public BuildingAction currentBuilding;
    public UnitSide currentSide;

    public void StartPlacement(BuildingAction action, UnitSide side)
    {
        placementProcess = new PlacementProcess(action, side);

        currentBuilding = action;
        currentSide = side;

        MyInputsystem.Instance.ChangeInputState(InputState.Placing);
    }
        
    public void UpdatePlacement()
    {
        if (placementProcess != null)
        {
            placementProcess.Update();
        }
    }
    public void ConfirmPlacement()
    {
        if(placementProcess != null && placementProcess.CanPlaceBuilding(out Vector3 placePosition))
        {
            var building = UnitFactory.CreateBuilding(
                    placementProcess.buildingAction,
                    placePosition
            );
            building.InitConstruction();
            building.unitSide = placementProcess.buildingSide;
            ClearPlacementProcess();
            MyInputsystem.Instance.ChangeInputState(InputState.Building);
        }
    }
    public void ConfirmPlacementForAI(BuildingAction buildingAction, UnitSide buildingSide ,Vector3Int placePosition)
    {
        var building = UnitFactory.CreateBuilding(
                    buildingAction,
                     placePosition
            );
        building.InitConstruction();
        building.unitSide = buildingSide;

        FactionData faction = GameManager.Instance.factions[(int)buildingSide];

        faction.TrySpendResource(buildingAction.goldCost, buildingAction.woodCost);
    }
    public void CanclePlacement()
    {
        if(currentBuilding != null)
        {
            FactionData faction = GameManager.Instance.factions[(int)currentSide];

            faction.RefundResource(currentBuilding.goldCost, currentBuilding.woodCost);
        }
        ClearPlacementProcess() ;
        MyInputsystem.Instance.ChangeInputState(InputState.Building);
    }

    private void ClearPlacementProcess()
    {
        placementProcess?.ClearHighlightArea();
        placementProcess?.DestroyOutline();
        placementProcess = null;
        currentBuilding = null;
        currentSide = UnitSide.E;
    }
}