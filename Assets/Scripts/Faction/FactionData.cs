using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionData
{
    public UnitSide side;

    [Header("阵营资源")]

    public int GoldNum = 20;
    public int WoodNum = 20;

    [Header("阵营单位")]
    public List<HumanUnit> humans = new List<HumanUnit>();
    public List<BuildingUnit> buildings = new List<BuildingUnit>();

    [Header("阵营人口信息")]
    public int TotalPeopleNum = 0;
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
    public void ResetFactionData(FactionSaveData data)
    {
        TotalPeopleNum = 0;
        currentPeopleNum = 0;

        WoodNum = data.WoodNum;
        GoldNum = data.GoldNum;
    }
    public FactionSaveData ToSaveData()
    {
        return new FactionSaveData(this);
    }
}
