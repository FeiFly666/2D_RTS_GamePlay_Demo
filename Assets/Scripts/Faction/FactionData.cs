using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionData
{
    public UnitSide side;

    [Header("阵营资源")]
    [SerializeField] private int _goldNum = 20;
    public int GoldNum
    {
        get { return _goldNum;}
        private set 
        { 
            _goldNum = value;
            OnDataUpdate?.Invoke();
        }
    }
    [SerializeField] private int _woodNum = 20;
    public int WoodNum
    {
        get { return _woodNum; }
        private set
        {
            _woodNum = value;
            OnDataUpdate?.Invoke();
        }
    }

    [Header("阵营单位")]
    public List<HumanUnit> humans = new List<HumanUnit>();
    public List<BuildingUnit> buildings = new List<BuildingUnit>();

    [Header("阵营人口信息")]
    private int _totalPeopleNum = 0;
    public int TotalPeopleNum
    {
        get { return _totalPeopleNum; }
    
        private set 
        { 
            _totalPeopleNum = value;
            OnDataUpdate?.Invoke();
        }
    }
    private int _currentPeopleNum = 0;
    public int currentPeopleNum
    {
        get
        { return _currentPeopleNum; }
        private set
        {
            _currentPeopleNum = value;
            OnDataUpdate?.Invoke();
        }
    }

    public bool CanAfford(int gold, int wood) => gold <= GoldNum && wood <= WoodNum;
    public bool HasPeopleSpace(int PopOc) => (currentPeopleNum +  PopOc) <= TotalPeopleNum;

    public Dictionary<BuildingType, int> BuildingTypeCount = new Dictionary<BuildingType, int>();

    public System.Action OnDataUpdate;

    public FactionData (UnitSide side)
    {
        this.side = side;
    }

    public void AddTypeBuildingNum(BuildingType type)
    {
        if(!BuildingTypeCount.ContainsKey(type))
        {
            BuildingTypeCount[type] = 0;
        }
        BuildingTypeCount[type] += 1;
        OnDataUpdate?.Invoke();
    }
    public void DecreaseTypeBuildingNum(BuildingType type)
    {
        BuildingTypeCount[type] -= 1;
        OnDataUpdate?.Invoke();
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
    public void AddGold(int goldNum) => GoldNum += goldNum;
    public void AddWood(int woodNum) => WoodNum += woodNum;
    public void AddTotalPeople(int peopleNum) => TotalPeopleNum += peopleNum;
    public void AddPopWeight(int popWeight) => currentPeopleNum += popWeight;

    public void RefundResource(int goldNum, int woodNum)
    {
        _goldNum += goldNum;
        _woodNum += woodNum;

        OnDataUpdate?.Invoke();
    }

    public void Add2PeopleData(int peopleWeight, int peopleNum)
    {
        _currentPeopleNum += peopleWeight;
        _totalPeopleNum += peopleNum;

        OnDataUpdate?.Invoke();
    }

    public void Decrease2PeopleData(int peopleWeight, int peopleNum)
    {
        _currentPeopleNum -= peopleWeight;
        _totalPeopleNum -= peopleNum;

    }

    public bool TrySpendResource(int goldNum,int woodNum)
    {
        if (!CanAfford(goldNum, woodNum)) return false;

        _goldNum -= goldNum;
        _woodNum -= woodNum;

        OnDataUpdate?.Invoke();

        return true;
    }
    public void ReleasePopWeight(int popWeight)
    {
        currentPeopleNum -= popWeight;
    }

    public void DecreaseTotalPeople(int peopleNum)
    {
        TotalPeopleNum -= peopleNum;
    }

    public void ResetFactionData(FactionSaveData data)
    {
        _totalPeopleNum = 0;
        _currentPeopleNum = 0;

        _woodNum = data.WoodNum;
        _goldNum = data.GoldNum;

        OnDataUpdate?.Invoke();
    }
    public FactionSaveData ToSaveData()
    {
        return new FactionSaveData(this);
    }
}
