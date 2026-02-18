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
            if(MyInputsystem.Instance.inputState == InputState.Unit && unit is BuildingUnit)
            {
                return false;
            }

            ClearActiveUnit();

            ActiveUnits.Add(unit);

            unit.OnSelected?.Invoke();

            MyInputsystem.Instance.ChangeInputState(InputState.Unit);

            UIManager.Instance.actionBar.CloseActionBar();

            UIManager.Instance.actionBar.ShowActionBarForUnit(unit);

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
            MyInputsystem.Instance.ChangeInputState(InputState.Unit);
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
        if (ActiveUnits.Count == 0) return;
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

        foreach(var role in roleUnits)
        {
            UnitRole currentRole = role.Key;
            List<HumanUnit> currentUnits = role.Value;

            List<UnitGroup> unitGroups = SplitGroups(currentUnits);

            foreach (var unitGroup in unitGroups)
            {
                if(unitGroup.members.Count == 1)
                {
                    HumanUnit human = unitGroup.members[0];
                    unitGroup.members[0].ai.LeaveGroup();

                    DispatchCommand(human, currentRole, target, mousePos ,isEnemy, isAlly, isResource);
                }
                else
                {
                    DispatchCommand(unitGroup, currentRole, target, mousePos, isEnemy, isAlly, isResource);
                }
            }

        }



        //以组为单位进行命令
        //Node targetNode = TilemapManager.Instance.FindNode(mousePos);
        //处理建造命令或治疗命令
/*        foreach (var group in Normalgroups)
        {
            if(group.members.Count == 1)
            {
                HumanUnit human = group.members[0];
                group.leader.ai.LeaveGroup();
                GameManager.Instance.groups.Remove(group);
                if (isAttackCommand)
                {
                    human.SetClickTarget(target);
                }
                else if(targetNode != null && targetNode.IsWalkable)
                {
                    human.ForcingMoveToDestination(targetNode.GetNodePosition());
                }
                continue;
            }
            else
            {
                if(isAttackCommand)
                {
                    group.FormGroupMoving(target.transform.position, target);
                }
                else if(isMovingCommand && targetNode != null && targetNode.IsWalkable)
                {
                    group.FormGroupMoving(mousePos);
                }
                else
                {
                    foreach(var member in  group.members.ToList())
                    {
                        member.ai.LeaveGroup();
                        member.ai.ClearPath();
                    }
                }
            }
           
        }*/
        
        VisualManager.Instance.UpdatePointer(mousePos, null, PointerMode.click);
;
    }
    private List<UnitGroup> SplitGroups(List<HumanUnit> humans )
    {
        List<UnitGroup> groups = new List<UnitGroup>();

        foreach (var unit in humans)
        {
            if (unit is not HumanUnit human || unit == null) continue;
            bool findCloseGroup = false;
            foreach (var group in groups)
            {
                if (Vector2.Distance(group.leader.transform.position, unit.transform.position) < 5)
                {
                    //TODO:查看unit和leader间是否隔墙然进行操作
                    if (TilemapManager.Instance.CheckBlockBetween2Nodes(group.leader.transform.position, unit.transform.position))
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

    public Unit GetUnitAtPos(Vector3 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos,dectectRadius);

        Unit bestTarget = null;

        foreach (var col in colliders)
        {
            Unit u = col.GetComponentInParent<Unit>();//人优先级最高
            if (u == null) continue;

            if (u is HumanUnit) return u;

            bestTarget = u;
        }

        return bestTarget;
    }
}
