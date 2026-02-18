using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum InputState
{ 
    None, //未选中任何单位
    Unit, //选中兵种单位
    Buliding // 选中建筑单位
}

public class MyInputsystem : Common.Singleton<MyInputsystem>
{
    private InputState InputState;

    private Vector2 MousePostion;

    private Vector2 dragStartPos;
    private bool isDragging = false;
    private float minDragDistance = 0.5f;

    public InputState inputState { 
        get { return InputState; }
        set { InputState = value;
            UIManager.Instance.ShowInputSystemState(this.inputState);
        }
    }
    public void UpdateMouseInput()
    {
        if (CommonUtils.IsPointOverUIElement()) return;

        MousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 mousePosition = MousePostion;

        if(InputState == InputState.Buliding)
        {
            BuildingManager.Instance.UpdatePlacement();
        }

        if(Input.GetMouseButtonDown(0)) { dragStartPos = MousePostion; isDragging = false; }
        //if(Input.GetMouseButtonDown(1)) { HandleRightClick(mousePosition); }

        if (Input.GetMouseButtonDown(1)) { HandleRightClick(mousePosition); }

        if(Input.GetMouseButton(0))
        {
            if (Vector2.Distance(dragStartPos, mousePosition) > minDragDistance)
            {
                isDragging = true;
                VisualManager.Instance.DrawSelectionBox(dragStartPos, mousePosition);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            VisualManager.Instance.HideSelectionBox();
            if(isDragging)
            {
                SelectionManager.Instance.TrySelectMutipleUnits(dragStartPos, mousePosition);
            }
            else
            {
                HandleLeftClick(mousePosition);
            }
            isDragging = false;
        }

    }
    private void HandleLeftClick(Vector3 mousePos)
    {
        if (inputState == InputState.Buliding)
        {
            BuildingManager.Instance.ConfirmPlacement();
            return;
        }
        else if (inputState == InputState.Unit)
        {
            Unit unit = SelectionManager.Instance.GetUnitAtPos(mousePos);
            if (unit == null && SelectionManager.Instance.ActiveUnits.Count == 1 && SelectionManager.Instance.ActiveUnits[0] is BuildingUnit)
            {
                SelectionManager.Instance.ClearActiveUnit();
                return;
            }
        }
        if (SelectionManager.Instance.TrySelectNewUnit(mousePos))
        {
            VisualManager.Instance.ClearAll();
            if (SelectionManager.Instance.ActiveUnits.Count == 1 && SelectionManager.Instance.ActiveUnits[0] is HumanUnit human && human.stateMachine.CurrentState is MoveState)
            {
                VisualManager.Instance.UpdatePointer(human.GetDestination(), human, PointerMode.show);
            }
        }
        else
        {
            SelectionManager.Instance.ExecuteCommand(mousePos);
        }
    }
    private void HandleRightClick(Vector3 mousePos)
    {
        if(inputState == InputState.Buliding)
        {
            BuildingManager.Instance.CanclePlacement();
            return;
        }
        
        SelectionManager.Instance.ClearActiveUnit();

        // 测试用

        if (!SelectionManager.Instance.TrySelectNewUnit(mousePos,true))
        {
            SelectionManager.Instance.ClearActiveUnit();
        }
        /*else
        {
            VisualManager.Instance.ClearAll();
            if(SelectionManager.Instance.ActiveUnits.Count == 1 && SelectionManager.Instance.ActiveUnits[0] is HumanUnit human && human.state == UnitState.Move)
            {
                VisualManager.Instance.UpdatePointer(human.GetDestination(), human ,PointerMode.show);
            }
        }*/
    }
    public void ChangeInputState(InputState state)
    {
        this.inputState = state;
    }

}
