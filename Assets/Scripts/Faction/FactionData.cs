using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionData
{
    public UnitSide side;

    [Header("阵营资源")]

    public int GoldNum = 1000;
    public int WoodNum = 1000;

    [Header("阵营单位")]
    public List<HumanUnit> humans = new List<HumanUnit>();
    public List<BuildingUnit> buildings = new List<BuildingUnit>();

    [Header("阵营人口信息")]
    public int TotalPeopleNum = 10;
    public int currentPeopleNum = 0;

    public bool CanAfford(int gold, int wood) => gold <= GoldNum && wood <= WoodNum;
    public bool HasPeopleSpace(int PopOc) => (currentPeopleNum +  PopOc) <= TotalPeopleNum;

    public Dictionary<BuildingType, int> BuildingTypeCount = new Dictionary<BuildingType, int>();

    public FactionData (UnitSide side)
    {
        this.side = side;
    }


    public bool CanGenerate(BuildingType[] conditions)
    {
        foreach (BuildingType type in conditions)
        {
            if(BuildingTypeCount.TryGetValue(type, out int count))
            {
                if(count <= 0)
                {
                    count = 0;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

}
