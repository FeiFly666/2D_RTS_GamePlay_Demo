using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public enum UnitRole
{
    Melee, //近战类
    Ranged, //远程类
    Worker, // 工人类
    Healr // 支援类
}
public class SelectionManager : MonoSingleton<SelectionManager>
{
    public List<Unit> ActiveUnits = new List<Unit>();

    [SerializeField] private float dectectRadius = 0.2f;

    public bool TrySelectNewUnit(Vector3 position, bool debugSelect = false)
    {
        Unit unit = GetUnitAtPos(position);

        if (unit != null && unit is HumanUnit human)
        {
            if (human.isBuildingUnit) { return false; }
        }

        if (unit != null && (unit.unitSide == GameManager.Instance.playerSide|| debugSelect) )
        {
            if(MyInputsystem.Instance.inputState == InputState.Human && unit is BuildingUnit)
            {
                return false;
            }

            ClearActiveUnit();

            ActiveUnits.Add(unit);

            unit.OnSelected?.Invoke();

            if(unit is HumanUnit)
            {
                MyInputsystem.Instance.ChangeInputState(InputState.Human);
            }
            else
            {
                MyInputsystem.Instance.ChangeInputState(InputState.Building);
            }

            UIManager.Instance.actionBar.CloseActionBar();


            if(unit is BuildingUnit building && building.buildingState == BuildingState.ConstructionFinished)
            {
                UIManager.Instance.actionBar.ShowActionBarForUnit(unit);
            }

            return true;
        }
        return false;
    }
    public void TrySelectMutipleUnits(Vector2 start, Vector2 end)
    {
        ClearActiveUnit();
        Collider2D[] colliders = Physics2D.OverlapAreaAll(start, end);
        foreach(var col in colliders)
        {
            Unit unit = col.GetComponent<Unit>();
            if(unit is HumanUnit human && unit.unitSide == GameManager.Instance.playerSide)
            {
                if (human.isBuildingUnit) continue;
                if(!ActiveUnits.Contains(unit))
                {
                    ActiveUnits.Add(unit);
                    unit.OnSelected?.Invoke();
                }
            }
        }
        if(ActiveUnits.Count > 0)
        {
            MyInputsystem.Instance.ChangeInputState(InputState.Human);
            if(ActiveUnits.Count == 1)
            {
                UIManager.Instance.actionBar.ShowActionBarForUnit(ActiveUnits[0]);
            }
        }
        else
        {
            MyInputsystem.Instance.ChangeInputState(InputState.None);
        }
    }

    
    public void ExecuteCommand(Vector3 mousePos)
    {
        for (int i = ActiveUnits.Count - 1; i >= 0; i--)
        {
            if (ActiveUnits[i] == null)
            {
                ActiveUnits.RemoveAt(i);
            }
        }
        if (ActiveUnits.Count == 0)
        {
            MyInputsystem.Instance.ChangeInputState(InputState.None);
            return;
        }

        VisualManager.Instance.UpdatePointer(mousePos, null, PointerMode.click);

        if (ActiveUnits[0] is BuildingUnit)
        {
            HandleBuildingCommand(ActiveUnits[0] as BuildingUnit, mousePos);
        }
        else
        {
            HandleHumanCommand(mousePos);
        }

    }
    private void HandleBuildingCommand(BuildingUnit unit, Vector3 mousePos)
    {
        Unit target = GetUnitAtPos(mousePos);

        bool isEnemy = target != null && target is not ResourceUnit && target.unitSide != GameManager.Instance.playerSide;
        bool isAlly = target != null && target is not ResourceUnit && target.unitSide == GameManager.Instance.playerSide;
        bool isGround = target == null;
        bool commandAvailable = true;

        if(unit.buildingState == BuildingState.ConstructionFinished)
        {
            switch (unit.buildingType)
            {
                case BuildingType.Attack:
                    if (isEnemy)
                    {
                        unit.SetBuildingUnitTarget(target);
                    }
                    else
                    {
                        commandAvailable = false;
                    }
                    break;
                case BuildingType.Train:
                    if (isGround)
                    {
                        TrainingBuilding t = unit as TrainingBuilding;
                        t.SetGatherPosition(mousePos);
                    }
                    else
                    {
                        commandAvailable = false;
                    }
                    break;
                case BuildingType.Collect:
                    if (target == unit)
                    {
                        GoldMine goldMine = unit as GoldMine;
                        goldMine.ReleaseAllUnits();
                    }
                    else
                    {
                        commandAvailable = false;
                    }
                    break;
                default:
                    commandAvailable = false;
                    break;
            }
        }
        else
            commandAvailable = false;
        if(!commandAvailable)
        {
            ClearActiveUnit();
        }
    }
    private void HandleHumanCommand(Vector3 mousePos)
    {
        foreach (var unit in ActiveUnits)
        {
            if (unit is HumanUnit human && unit != null)
            {
                human.target = null;
                human.targetID = -1;
                human.ai.LeaveGroup();
            }
        }

        VisualManager.Instance.ClearAll();

        Unit target = GetUnitAtPos(mousePos);
        bool isEnemy = target != null && target is not ResourceUnit && target.unitSide != GameManager.Instance.playerSide;
        bool isAlly = target != null && target is not ResourceUnit && target.unitSide == GameManager.Instance.playerSide;
        bool isInsideMine = isAlly && target is GoldMine mine && mine.buildingState == BuildingState.ConstructionFinished;
        bool isResource = target != null && target is ResourceUnit;

        Dictionary<UnitRole, List<HumanUnit>> roleUnits = new Dictionary<UnitRole, List<HumanUnit>>();
        //不同类型进行分类
        foreach (var unit in ActiveUnits)
        {
            HumanUnit human = unit as HumanUnit;
            if (human != null)
            {
                if (!roleUnits.ContainsKey(human.role)) roleUnits.Add(human.role, new List<HumanUnit>());

                roleUnits[human.role].Add(human);
            }
        }

        foreach (var role in roleUnits)
        {
            UnitRole currentRole = role.Key;
            List<HumanUnit> currentUnits = role.Value;

            List<UnitGroup> unitGroups = SplitGroups(currentUnits);

            foreach (var unitGroup in unitGroups)
            {
                //金矿所有单位都可下，所以无需判断role
                if (isInsideMine)
                {
                    Vector3 entryPos = target.transform.position + (Vector3)(target as GoldMine).spawnPosition;
                    if (unitGroup.members.Count == 1)
                    {
                        HumanUnit human = unitGroup.members[0];
                        human.ai.LeaveGroup();
                        human.SetClickTarget(target);
                    }
                    else
                    {
                        unitGroup.FormGroupMoving(entryPos, target);
                    }
                    continue;
                }

                if (unitGroup.members.Count == 1)
                {
                    HumanUnit human = unitGroup.members[0];
                    unitGroup.members[0].ai.LeaveGroup();

                    DispatchCommand(human, currentRole, target, mousePos, isEnemy, isAlly, isResource);
                }
                else
                {
                    DispatchCommand(unitGroup, currentRole, target, mousePos, isEnemy, isAlly, isResource);
                }
            }

        }
    }

    private List<UnitGroup> SplitGroups(List<HumanUnit> humans )
    {
        List<UnitGroup> groups = new List<UnitGroup>();

        foreach (var unit in humans)
        {
            Vector3 unitPos = unit.transform.position;
            if (unit is not HumanUnit human || unit == null) continue;
            bool findCloseGroup = false;
            foreach (var group in groups)
            {
                if ((group.leader.transform.position - unit.transform.position).sqrMagnitude < 25)
                {
                    //TODO:查看unit和leader间是否隔墙然进行操作
                    if (TilemapManager.Instance.CheckBlockBetween2Nodes(group.leader.transform.position, unitPos))
                    {
                        group.AddNewMember(human);
                        findCloseGroup = true;
                        break;
                    }
                }
            }
            if (findCloseGroup) continue;
            UnitGroup newGroup = new UnitGroup(human);
            groups.Add(newGroup);
        }
        return groups;
    }

    private void DispatchCommand(UnitGroup group, UnitRole role, Unit target, Vector3 mousePos, bool isEnemy, bool isAlly, bool isResource)
    {
        if (group == null) return;
        Node targetNode = TilemapManager.Instance.FindNode(mousePos);
        if(target is not HumanUnit && target != null)
        {
            targetNode = TilemapManager.Instance.GetClosestInteractableNode(target.gameObject, group.leader.transform.position, group.leader.gameObject);
        }

        Vector2 targetPos = new Vector2(0, 0);
        if (targetNode != null)
            targetPos = targetNode.GetNodePosition();

        bool commandAvailable = true;
        Debug.Log($"{isEnemy} {isAlly} {isResource}");
        switch(role)
        {
            case UnitRole.Melee:
                if (isEnemy) group.FormGroupMoving(target.transform.position, target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    group.FormGroupMoving(targetPos);
                }
                else
                {
                    commandAvailable = false;
                }
                break;
            case UnitRole.Ranged:
                if (isEnemy) group.FormGroupMoving(target.transform.position, target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    group.FormGroupMoving(targetPos);
                }
                else
                {
                    commandAvailable = false;
                }
                break;
            case UnitRole.Worker:
                if (isAlly && target is BuildingUnit) group.FormGroupMoving(target.transform.position, target);
                else if (isResource) group.FormGroupMoving(target.transform.position, target);
                else if (isEnemy) group.FormGroupMoving(target.transform.position, target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    group.FormGroupMoving(targetPos);
                }
                else
                {
                    commandAvailable = false;
                }
                break;
            case UnitRole.Healr:
                if (isAlly && target is HumanUnit) group.FormGroupMoving(target.transform.position, target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    group.FormGroupMoving(targetPos);
                }
                else
                {
                    commandAvailable = false;
                }
                break;
        }
        if(!commandAvailable)
        {
            foreach (var member in group.members.ToList())
            {
                member.ai.LeaveGroup();
                member.ai.ClearPath();
            }
        }
    }

    private void DispatchCommand(HumanUnit unit, UnitRole role, Unit target, Vector3 mousePos, bool isEnemy, bool isAlly, bool isResource)
    {
        if (unit == null) return;
        Node targetNode = TilemapManager.Instance.FindNode(mousePos);
        if (target is not HumanUnit && target != null)
        {
            targetNode = TilemapManager.Instance.GetClosestInteractableNode(target.gameObject, unit.transform.position, unit.gameObject);
        }

        Vector2 targetPos = new Vector2(0,0);
        if (targetNode != null)
            targetPos = targetNode.GetNodePosition();

        bool commandAvailabe = true;
        switch (role)
        {
            case UnitRole.Melee:
                if (isEnemy) unit.SetClickTarget(target);
                else if(targetNode != null && targetNode.IsWalkable)
                {
                    unit.ForcingMoveToDestination(targetPos);
                }
                else
                {
                    commandAvailabe = false;
                }
                break;
            case UnitRole.Ranged:
                if (isEnemy) unit.SetClickTarget(target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    unit.ForcingMoveToDestination(targetPos);
                }
                else
                {
                    commandAvailabe = false;
                }
                break;
            case UnitRole.Worker:
                if (isAlly && target is BuildingUnit) unit.SetClickTarget(target);
                else if (isResource) unit.SetClickTarget(target);
                else if (isEnemy) unit.SetClickTarget(target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    unit.ForcingMoveToDestination(targetPos);
                }
                else
                {
                    commandAvailabe = false;
                }
                break;
            case UnitRole.Healr:
                if (isAlly && target is HumanUnit) unit.SetClickTarget(target);
                else if (targetNode != null && targetNode.IsWalkable)
                {
                    unit.ForcingMoveToDestination(targetPos);
                }
                else
                {
                    commandAvailabe = false;
                }
                break;
        }
        if (!commandAvailabe)
        {
            unit.ai.ClearPath();
        }
    }

    public void HandleBuyBuildingCommand(Vector3 pos)
    {
        Unit unit = GetUnitAtPos(pos);

        if(unit is BuildingUnit building)
        {
            building.BuyThisBuilding();
        }
    }

    public void ClearActiveUnit()
    {
        foreach(var unit in ActiveUnits)
        {
            if (unit == null) continue;
            unit.OnDeselected?.Invoke();
        }
        ActiveUnits.Clear();
        MyInputsystem.Instance.ChangeInputState(InputState.None);
        UIManager.Instance.actionBar.CloseActionBar();
        VisualManager.Instance.ClearAll();
    }

    private static Collider2D[] slot = new Collider2D[20];
    public Unit GetUnitAtPos(Vector3 pos)
    {
        int num = Physics2D.OverlapCircleNonAlloc(pos, dectectRadius, slot);

        Unit bestTarget = null;

        for (int i = 0; i < num; i++)
        {
            Collider2D col = slot[i];
            Unit u = col.GetComponentInParent<Unit>();//人优先级最高
            if (u == null) continue;

            if (u is HumanUnit) return u;

            bestTarget = u;
        }

        return bestTarget;
    }
}
