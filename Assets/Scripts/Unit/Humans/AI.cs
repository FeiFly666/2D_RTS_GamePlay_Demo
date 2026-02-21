using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;



// 注意：此类已功能稳定，请勿在未验证情况下重构
// 所有新增行为优先通过外部系统驱动
public class AI : MonoBehaviour
{
    [SerializeField]private Vector3 FinalTargetPosition;
    private Vector3 CurrentTargetPosition;
    [SerializeField] public float movesSpeed;
    private HumanUnit unit;

    public UnitGroup currentGroup = null;

    private List<Node> currentPath = new List<Node>();

    private Node TargetNode;
    private bool isSearchingPark = false;
    private int currentNodeIndex = 0;

    public int nodeNum;

    public bool isForcingMoving = false;
    public bool needSpace = false;

    public bool isWaitingPath = false;

    public bool isInGroup = false;

    private float nextSearchTime = 0f;
    private Vector2 groupFormationOffset;

    public System.Action arriveTarget;

    public System.Action<Vector3,Unit> OnTeamPathBlocked;

    public Vector2 targetNodePos = new Vector2();
    private void Awake()
    {
        this.currentGroup = null;
        unit = GetComponent<HumanUnit>();
    }
    private void Start()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            FinalTargetPosition = transform.position;
        }
        groupFormationOffset = new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
    }

    private void Update()
    {
        if(TargetNode != null)
        {
            targetNodePos = TargetNode.GetNodePosition();
        }
        else
        {
            targetNodePos = new Vector2(-10000, -10000);
        }
        
        isInGroup = IsUnitInGroup;
        nodeNum = currentPath.Count;
        if (unit.isDead) return;
        if(IsPathVaild())
        {
            FollowPath();

            CheckParkingStatus();
        }

    }
    private void FollowPath()
    {
        if (currentNodeIndex >= currentPath.Count || currentPath.Count == 0) return;

        Node newNode = currentPath[currentNodeIndex];

        if(!newNode.IsWalkable)
        {
            Debug.Log(gameObject.name + " 卧槽！路不能走了，重新寻路...");
            if(currentGroup == null)
            {
                ReRouteToNearestOpenSlot();
            }
            else
            {
                this.OnTeamPathBlocked(FinalTargetPosition, unit.target);
            }
            return;
        }

        CurrentTargetPosition = new Vector3(newNode.CenterX, newNode.CenterY);

        if(isInGroup)
        {
            CurrentTargetPosition = CurrentTargetPosition + (Vector3)groupFormationOffset;
        }


        Vector3 direction = (CurrentTargetPosition - transform.position).normalized;

        unit.FlipController(CurrentTargetPosition);

        transform.position += direction * movesSpeed * Time.deltaTime;
        if ((CurrentTargetPosition - transform.position).sqrMagnitude < 0.016f)
        {
            currentNodeIndex++;
            if (currentNodeIndex >= currentPath.Count)
            {
                OnReachPathEnd();
                return;
            }
        }
    }
    private void CheckParkingStatus()
    {
        if (currentGroup != null && currentGroup.targetUnit != null) return;

        int nodeLeft = currentPath.Count - currentNodeIndex;
        if(nodeLeft <= 5)
        {
            if (Time.time < nextSearchTime) return;

            nextSearchTime = Time.time + 0.5f;

            Node targetNode = TilemapManager.Instance.FindNode(FinalTargetPosition);
            if(targetNode != null)
            {
                bool isSpaceOccupiedByOthers = !TilemapManager.Instance.IsNodeAndNeighborsFree(targetNode, this.gameObject, needSpace);
                if(isSpaceOccupiedByOthers)
                {
                    ReRouteToNearestOpenSlot();
                }
                else if(!targetNode.IsQccupied)
                {
                    ReleaseTargetNode();

                    TargetNode = targetNode;

                    targetNode.occupant = this.gameObject;

                }
            }
            
            
        }
    }
    private void ReRouteToNearestOpenSlot()
    {
        if (isSearchingPark) return;
        isSearchingPark = true;

        Debug.Log("目标位置被占用啦，找一下最近的可用格子....");

        Node bestNode = TilemapManager.Instance.FindNearestAvailableNode(FinalTargetPosition, this.gameObject, needSpace);
        if (bestNode != null && bestNode != TargetNode)
        {
            ReleaseTargetNode();

            TargetNode = bestNode;
            TargetNode.occupant = this.gameObject;

            FinalTargetPosition = TargetNode.GetNodePosition();
            unit.lastPathRequestTargetPos = FinalTargetPosition;

            PathRequestController.RequestPath(this.transform.position, FinalTargetPosition, OnPathFound,this);

        }
        else
        {
            Debug.Log("目标位置附近也无法到达了，停下....");

            FinalTargetPosition = this.transform.position;
            unit.lastPathRequestTargetPos = this.transform.position;

            PathRequestController.RequestPath(this.transform.position, this.transform.position, OnPathFound,this);
        }
        isSearchingPark = false;
    }


    private void OnReachPathEnd()
    {
        if (isWaitingPath || (unit.target != null && !unit.target.isDead))
        {
            return;
        }
        if (currentGroup  != null)
        {
            if (currentGroup.targetUnit == null || currentGroup.targetUnit.isDead)
            {
                unit.isForcingTarget = false;
                LeaveGroup();
            }

        }
        ClearPath();
        this.isForcingMoving = false;
        if(TargetNode != null)
        {
            TargetNode.occupant = this.gameObject;
        }

        arriveTarget?.Invoke();
    }

    public void InstantOccupyNode()
    {
        Node currentNode = TilemapManager.Instance.FindNode(transform.position);
        if (currentNode != null)
        {
            ReleaseTargetNode();
            TargetNode = currentNode;
            TargetNode.occupant = this.gameObject;
        }
    }
    public void ForceMovingToDesitination(Vector3 destination)
    {
        RegisterDestinationFrame(destination);
        isForcingMoving = true;
        unit.isForcingTarget = false;
    }
    public void ForceRegisterPath(List<Node> thePath, UnitGroup group)
    {
        isWaitingPath = false;

        if(group.targetUnit != null)
        {
            isForcingMoving = false;
            unit.isForcingTarget = true;
        }
        else
        {
            isForcingMoving = true;
            unit.isForcingTarget = false;
        }

        currentPath = thePath;
        currentNodeIndex = 0;

        ReleaseTargetNode();
        if (currentGroup != null)
        {
            if(group.targetUnit == null)
            {
                FinalTargetPosition = group.movePosition;
            }
            else
            {
                FinalTargetPosition = group.lastTargetPos;
            }
        }
        else
        {
            FinalTargetPosition = this.transform.position;
        }
        //unit.lastPathRequestTargetPos = FinalTargetPosition;
    }
    public void RegisterPath(List<Node> path, int index)
    {
        currentPath = path;
        currentNodeIndex = index;
        ReleaseTargetNode();

        FinalTargetPosition = path[path.Count - 1].GetNodePosition();
    }
    public void RegisterDestinationFrame(Vector3 destination)
    {
        //unit.lastPathRequestTargetPos = destination;
        isWaitingPath = true;
        FinalTargetPosition = destination;
        LeaveGroup();
        ReleaseTargetNode();
        
        PathRequestController.RequestPath(this.transform.position, destination, OnPathFound, this);
    }
    private void ReleaseTargetNode()
    {
        if(TargetNode != null && TargetNode.occupant == this.gameObject)
        {
            TargetNode.occupant = null;
        }
        TargetNode = null;
    }
    private void OnPathFound(List<Node> newPath, bool success)
    {
        if (this == null) return;//防止异步结果到达时ai由于某些原因被销毁
        isWaitingPath = false;
        if(success)
        {
            currentPath = newPath;
            currentNodeIndex = 0;
            if(newPath.Count > 0)
            {
                FinalTargetPosition = newPath[newPath.Count - 1].GetNodePosition();
            }
            else
            {
                FinalTargetPosition = this.transform.position;
                ClearPath();
            }
            if(currentGroup == null && unit.target != null)
            {
                TilemapManager.Instance.pathShare.BoardcastPath(unit.target, currentPath);
            }
        }
        else
        {
            if(this.unit != null && !this.unit.isDead)
            {
                PathRequestController.RequestPath(this.transform.position, this.transform.position, OnPathFound, this);
            }
        }
    }
    public void RegisterGroup(UnitGroup group)
    {
        if (group != null)
        {

            this.currentGroup = group;
        }
    }
    public void LeaveGroup()
    {
        if(this.currentGroup != null)
        {
            currentGroup.LeaveUnitGroup(this.unit);
            if(IsPathVaild())
            {
                ClearPath();
            }
            isForcingMoving = false;
            this.currentGroup = null;
        }
    }

    public void ClearPath()
    {
        currentPath.Clear();
        currentNodeIndex = 0;

        if(unit.isBuildingUnit) { return; }

        Node currentStandNode = TilemapManager.Instance.FindNode(this.transform.position);

        //if (TargetNode != null && currentStandNode == TargetNode) return;

        needSpace = (unit.target == null);

        if (isWaitingPath || IsUnitInGroup) return;

        if (currentStandNode != null)
        {
            //新规则检查当前位置
            if (!TilemapManager.Instance.IsNodeAndNeighborsFree(currentStandNode, this.gameObject, needSpace))
            {
                Node neighborFreeNode = TilemapManager.Instance.FindNearestAvailableNode(this.transform.position, this.gameObject, needSpace);
                if (neighborFreeNode != null)
                {
                    ReleaseTargetNode();
                    TargetNode = neighborFreeNode;
                    neighborFreeNode.occupant = this.gameObject;
                    RegisterDestinationFrame(neighborFreeNode.GetNodePosition());
                }
            }
            else
            {
                ReleaseTargetNode();
                TargetNode = currentStandNode;
                currentStandNode.occupant = this.gameObject;
            }
        }
    }

    public bool IsPathVaild()
    {
        return currentPath != null && currentPath.Count > 0 && currentNodeIndex < currentPath.Count;
    }
    public bool IsUnitInGroup => currentGroup != null;
    public Vector3 GetDestination()
    {
        return new Vector3(FinalTargetPosition.x, FinalTargetPosition.y, FinalTargetPosition.z);
    }
    public Node GetPathFinalNode()
    {
        if(currentPath != null && currentPath.Count > 0)
        {
            return currentPath[currentPath.Count - 1];
        }
        return null;
    }
    private void OnDestroy()
    {
        ReleaseTargetNode();
    }
}
