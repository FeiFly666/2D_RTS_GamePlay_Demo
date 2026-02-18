using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//[Serializable]
//[Serializable]
public class UnitGroup
{
    public int uniqueID;
    public HumanUnit leader;
    public List<HumanUnit> members = new List<HumanUnit>();

    private List<Node> sharedPath;

    private float GroupPathUpdateFrequency = 0.2f;
    private float pathTimer = 0f;

    public Unit targetUnit;
    public int targetID = -1;

    public Vector3 movePosition = new Vector3(-1500,-1500,-1500);//鼠标点击移动用，非追击目标

    public Vector3 lastTargetPos;

    private float chaseUpdateFrequency = 0.5f;
    private float lastChaseUpdateTime = 0f;

    private bool isResume = false;

    public UnitGroup() { }

    public UnitGroup(HumanUnit leader)
    {
        this.leader = leader;
        AddNewMember(leader);
        this.uniqueID = GameManager.Instance.GetAnID();
        GameManager.Instance.groups.Add(this);
    }

    public void AddNewMember(HumanUnit member)
    {
        if(!members.Contains(member))
        {
            members.Add(member);
            if(member.ai.currentGroup != null && member.ai.currentGroup != this)
            {
                //member.ai.OnTeamPathBlocked -= member.ai.currentGroup.FormGroupMoving;
                member.ai.LeaveGroup();
            }
            member.ai.OnTeamPathBlocked += FormGroupMoving;
            member.ai.RegisterGroup(this);
        }
    }
    public void FormGroupTarget(Unit target)
    {
        targetUnit = target;
        targetID = target.uniqueID;
        foreach(var member in members)
        {
            member.SetClickTarget(target);
        }
    }
    private void CheckEmptyMember()
    {
        for(int i = members.Count - 1; i >= 0; i--)
        {
            var member = members[i];
            if (member == null) members.RemoveAt(i);
        }
    }
    private bool CheckTime()
    {
        if(Time.time - pathTimer > GroupPathUpdateFrequency)
        {
            pathTimer = Time.time;
            return true;
        }
        return false;
    }
    public void FormGroupMoving(Vector3 targetPos, Unit target = null)//选择领头人，领头人开始尝试寻路，如果寻找到路线，则成员开始涌入领头人选定的路线
    {
        if (!CheckTime() || members.Count == 0) return;

        leader = isResume ? FindFarestMemberToTarget(targetPos) : FindClosestMemberToTarget(targetPos);

        //Debug.Log($"leader 为 {leader.gameObject.name} 开始寻找路线");

        if (target != null && target is not HumanUnit)
        {
            Node bestNode = TilemapManager.Instance.GetClosestInteractableNode(target.gameObject, leader.transform.position, leader.gameObject);
            if(bestNode != null)
            {
                targetPos = bestNode.GetNodePosition();
            }
        }
        if(target == null)
        {
            this.movePosition = targetPos;
        }
        this.lastTargetPos = targetPos;
        foreach (var member in members) member.ai.isWaitingPath = true;

        PathRequestController.RequestPath(leader.transform.position, targetPos, (path, success) => {
            if (success)
            {
                /*if (isResume)
                {
                    isResume = false;
                }*/
                if (!success || leader == null || leader.isDead || members == null || members.Count == 0)
                {
                    return;
                }
                sharedPath = path;
                if(target != null)
                {
                    targetID = target.uniqueID;
                    FormGroupTarget(target);
                }
                if (target == null)
                {
                    movePosition = targetPos;
                }
                foreach (var member in members)
                {
                    member.ai.isWaitingPath = false;
                    if(target != null)
                        member.lastPathRequestTargetPos = targetPos;
                }
                
                StartGroupMove();
            }
        },this);
    }

    private void StartGroupMove()
    {
        //Debug.Log($"leader 为 {leader.gameObject.name} 的小组找到路线，开始移动");
        leader.ForcingMoveToDestination(sharedPath, this);
        bool hasEmptyMember = false;

        foreach (var member in members)
        {
            //if (member == leader) continue;
            if (member == null || member.isDead)
            {
                hasEmptyMember = true;
                continue;
            }
            //Debug.Log($"leader 为 {leader.gameObject.name} 的小组成员{member.gameObject.name}找到路线，开始移动");

            int startIndex = GetAnPathIndex(member);

            List<Node> memberPath = sharedPath.GetRange(startIndex, sharedPath.Count - startIndex);

            member.ForcingMoveToDestination(memberPath, this);
        }

        if(hasEmptyMember)
        {
            CheckEmptyMember();
        }

    }

    private int GetAnPathIndex(HumanUnit member)
    {
        if (sharedPath == null || sharedPath.Count == 0) return 0;

        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 1; i < Mathf.Min(sharedPath.Count,20); i++)
        {
            if (!TilemapManager.Instance.CheckBlockBetween2Nodes(member.transform.position, sharedPath[i].GetNodePosition()))
            {
                continue;
            }
            float dist = Vector2.Distance(member.transform.position, sharedPath[i].GetNodePosition());
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
    private HumanUnit FindClosestMemberToTarget(Vector3 targertPos)
    {
        HumanUnit closest = members[0];
        float closestDistance = Mathf.Infinity;
        foreach(HumanUnit member in members)
        {
            if (member == null) continue;
            float distance = Vector2.Distance(member.transform.position, targertPos);
            if(distance < closestDistance)
            {
                closest = member;
                closestDistance = distance;
            }
        }
        return closest;
    }
    private HumanUnit FindFarestMemberToTarget(Vector3 targertPos)
    {
        HumanUnit closest = members[0];
        float closestDistance = -1;
        foreach (HumanUnit member in members)
        {
            if (member == null) continue;
            float distance = Vector2.Distance(member.transform.position, targertPos);
            if (distance > closestDistance)
            {
                closest = member;
                closestDistance = distance;
            }
        }
        return closest;
    }
    public void UpdateGroupChase()
    {
        if (targetUnit == null || targetUnit.isDead)
        {
            targetUnit = null;
            targetID = -1;
            foreach(var member in members.ToList())
            {
                if (member.ai.currentGroup != null)
                {
                    member.ai.LeaveGroup();
                }
            }
            return;
        }
        if (targetUnit is not HumanUnit)
        {
            return;
        }
        Debug.Log("正在群体追击！！！！");
        //FormGroupTarget(targetUnit);
        if (Time.time - lastChaseUpdateTime > chaseUpdateFrequency)
        {
            if (Vector2.Distance(targetUnit.transform.position, lastTargetPos) > 3f || (leader != null && leader.ai.nodeNum <= 7))
            {
                lastChaseUpdateTime = Time.time;
                lastTargetPos = targetUnit.transform.position;
                FormGroupMoving(targetUnit.transform.position, targetUnit);
            }
        }
    }
    public void LeaveUnitGroup(HumanUnit unit)
    {
        if (members == null) return;
        if(members.Contains(unit))
        {
            members.Remove(unit);

            unit.ai.OnTeamPathBlocked -= FormGroupMoving;
        }

        if (unit == leader && members.Count > 0)
        {
            leader = members[0];
        }


        if (members.Count <= 1)
        {
            if(members.Count!=0)
            {
                members[0].ai.OnTeamPathBlocked -= FormGroupMoving;
                members.RemoveAt(0);
            };
            targetUnit = null;
            targetID = -1;
            sharedPath = null;
            leader = null;
            members.Clear();
            if(GameManager.Instance.groups.Contains(this))
            {
                GameManager.Instance.groups.Remove(this);
            }
        }
        
    }

    public GroupSaveData ToSaveData()
    {
        return new GroupSaveData(this);
    }
    public void LoadData(GroupSaveData data)
    {
        this.uniqueID = data.ID;
        this.pathTimer = -100f;
        this.lastChaseUpdateTime = -100f;

        if (!GameManager.Instance.groups.Contains(this))
        {
            GameManager.Instance.groups.Add(this);
        }

        leader = GameManager.Instance.liveHumanUnits.Find(h => h.uniqueID == data.leaderID);

        foreach(var memberID in data.memberIDs)
        {
            var member = GameManager.Instance.liveHumanUnits.Find(h => h.uniqueID == memberID);
            if(member == null) continue;
            AddNewMember(member);
            //member.ai.currentGroup = this;
        }
        if(data.targetID != -1)
        {
            this.targetID = data.targetID;
            this.targetUnit = GameManager.Instance.liveHumanUnits.Find(h => h.uniqueID == data.targetID);
            if(this.targetUnit == null)
            {
                this.targetUnit = GameManager.Instance.buildings.Find(h => h.uniqueID == data.targetID);
            }
        }
        this.movePosition = new Vector3(data.targetPosition.x, data.targetPosition.y, data.targetPosition.z);
    }
    public void ResumeLogic()
    {
        isResume = true;
        if (targetUnit != null)
        {
            Vector3 latestEnemyPos = targetUnit.transform.position;

            this.lastTargetPos = latestEnemyPos;
            FormGroupMoving(latestEnemyPos, targetUnit);
        }
        else if(movePosition != new Vector3(-1500, -1500, -1500))
        {
            FormGroupMoving(movePosition);
        }

    }
}
