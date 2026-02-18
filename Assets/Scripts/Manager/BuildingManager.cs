using Common;
using System.Collections;
using UnityEngine;

public class BuildingManager : Singleton<BuildingManager>
{
    private PlacementProcess placementProcess;

    public void StartPlacement(BuildingAction action, UnitSide side)
    {
        placementProcess = new PlacementProcess(action, side);
        MyInputsystem.Instance.ChangeInputState(InputState.Buliding);
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
            var building = BuildingFactory.CreateBuilding(
                    placementProcess.buildingAction,
                    placePostion
            );
            building.InitConstruction();
            building.unitSide = placementProcess.buildingSide;
            ClearPlacementProcess();
            MyInputsystem.Instance.ChangeInputState(InputState.None);
        }
    }
    public void CanclePlacement()
    {
        ClearPlacementProcess() ;
        MyInputsystem.Instance.ChangeInputState(InputState.None );
    }

    private void ClearPlacementProcess()
    {
        placementProcess?.ClearHighlightArea();
        placementProcess?.DestroyOutline();
        placementProcess = null;
    }
}