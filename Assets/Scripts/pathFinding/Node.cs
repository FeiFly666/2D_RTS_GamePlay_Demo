using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int GridX, GridY;
    public int ButtomX, ButtomY;
    public int AreaID;

    public bool IsWalkable;

    public float CenterX,CenterY;

    public int gCost;//该格到起点格的距离
    public int hCost;//该格到终点格的距离
    public int fCost;//gCost + fCost
    public bool isTouched;//只要这个点被改过成本就true,方便重置

    public bool isChecked;//迷雾用

    public Node parentNode;

    public GameObject occupant;

    public bool IsQccupied => occupant != null;

    public Node(int i, int j, float centerX, float centerY, bool isWalkable)
    {
        GridX = i;
        GridY = j;

        CenterX = centerX;
        CenterY = centerY;

        IsWalkable = isWalkable;

        gCost = 0;
        hCost = 0;
        fCost = 0;

        parentNode = null;

    }

    public Vector3 GetNodePosition()
    {
        return new Vector3(CenterX, CenterY);
    }

    public bool CanBeEndPoint(GameObject request)
    {
        return IsWalkable && (occupant == request || occupant == null);
    }
    
}
