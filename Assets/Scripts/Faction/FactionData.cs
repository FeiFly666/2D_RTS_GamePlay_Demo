using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionData
{
    public UnitSide side;

    [Header("阵营资源")]

    public int GoldNum = 0;
    public int WoodNum = 0;

    [Header("阵营单位")]
    public List<HumanUnit> humans = new List<HumanUnit>();
    public List<BuildingUnit> buildings = new List<BuildingUnit>();

    [Header("阵营人口信息")]
    public int TotalPeopleNum = 0;
    public int currentPeopleNum = 0;

    public bool CanAfford(int gold, int wood) => gold <= GoldNum && wood <= WoodNum;
    public bool HasPeopleSpace(int PopOc) => (currentPeopleNum +  PopOc) <= TotalPeopleNum;

    public FactionData (UnitSide side)
    {
        this.side = side;
    }

}
