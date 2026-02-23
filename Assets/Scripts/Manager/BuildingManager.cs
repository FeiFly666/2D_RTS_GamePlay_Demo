using Common;
using System.Collections;
using UnityEngine;

public class BuildingManager : Singleton<BuildingManager>
{
    private PlacementProcess placementProcess;

    public void StartPlacement(BuildingAction action, UnitSide side)
    {
        placementProcess = new PlacementProcess(action, side);

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
        if(placementProcess != null && placementProcess.CanPlaceBuilding(out Vector3 placePostion))
        {
            var building = UnitFactory.CreateBuilding(
                    placementProcess.buildingAction,
                    placePostion
            );
            building.InitConstruction();
            building.unitSide = placementProcess.buildingSide;
            ClearPlacementProcess();
            MyInputsystem.Instance.ChangeInputState(InputState.Building);
        }
    }
    public void CanclePlacement()
    {
        ClearPlacementProcess() ;
        MyInputsystem.Instance.ChangeInputState(InputState.Building);
    }

    private void ClearPlacementProcess()
    {
        placementProcess?.ClearHighlightArea();
        placementProcess?.DestroyOutline();
        placementProcess = null;
    }
}