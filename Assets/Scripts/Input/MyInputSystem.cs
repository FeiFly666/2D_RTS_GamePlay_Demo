
using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum InputState
{ 
    None, //未选中任何单位
    Human, //选中兵种单位
    Building, //选中建筑单位
    Placing // 放置建筑单位
}

public class MyInputsystem : Common.Singleton<MyInputsystem>
{
    private InputState _InputState;
    private InputState _LastState;

    private Vector2 MousePostion;

    private Vector2 LeftDragStartPos;
    private bool isLeftDragging = false;

    private Vector2 MiddleDragStartPos;
    private bool isMiddleDragging = false;
    private float minDragDistance = 0.5f;

    public bool isGameStart = false;

    public InputState inputState { 
        get { return _InputState; }
        set {
            _LastState = _InputState;
            _InputState = value;
            UIManager.Instance.ShowInputSystemState(this.inputState);
        }
    }
    public void UpdateMouseInput()
    {
        if (!isGameStart) return;

        MousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 mousePosition = MousePostion;



        if (CommonUtils.IsPointOverUIElement())
        {
            return;
        }

        if (_InputState == InputState.Placing)
        {
            BuildingManager.Instance.UpdatePlacement();
        }

        if (Input.GetMouseButtonDown(0)) { LeftDragStartPos = MousePostion; isLeftDragging = false; }
            //if(Input.GetMouseButtonDown(1)) { HandleRightClick(mousePosition); }

        if (Input.GetMouseButtonDown(1)) { HandleRightClick(mousePosition); }


        if(Input.GetMouseButton(0))
        {
            if (Vector2.Distance(LeftDragStartPos, mousePosition) > minDragDistance)
            {
                isLeftDragging = true;
                VisualManager.Instance.DrawSelectionBox(LeftDragStartPos, mousePosition);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            VisualManager.Instance.HideSelectionBox();
            if(isLeftDragging)
            {
                SelectionManager.Instance.TrySelectMutipleUnits(LeftDragStartPos, mousePosition);
            }
            else
            {
                HandleLeftClick(mousePosition);
            }
            isLeftDragging = false;
        }
    }
    private void HandleLeftClick(Vector3 mousePos)
    {
        if (inputState == InputState.Placing)
        {
            BuildingManager.Instance.ConfirmPlacement();
            return;
        }
        else if (inputState == InputState.Building)
        {
            SelectionManager.Instance.ExecuteCommand(mousePos);
            return;
        }
        //human和默认
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
        if(inputState == InputState.Placing)
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

    public void ChangeToLastState()
    {
        this.inputState = _LastState;
    }
}
