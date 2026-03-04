using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class FactionData
{
    public UnitSide side;

    [Header("阵营资源")]
    [SerializeField] private int _goldNum = 80;
    public int GoldNum
    {
        get { return _goldNum;}
        private set 
        { 
            _goldNum = value;
            OnDataUpdate?.Invoke();
        }
    }
    [SerializeField] private int _woodNum = 80;
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
    public List<Worker> workers = new List<Worker>();
    public List<HumanUnit>IdleNoWorkerHumans = new List<HumanUnit>();
    public List<Worker>IdleWorkers = new List<Worker>();
    public List<BuildingUnit> buildings = new List<BuildingUnit>();
    public List<TrainingBuilding> trainings = new List<TrainingBuilding>();
    public List<GoldMine> goldMines = new List<GoldMine>();

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
    public bool HasPeopleSpace(int PopOc) => (currentPeopleNum +  PopOc) < TotalPeopleNum;

    //public Dictionary<BuildingType, int> BuildingTypeCount = new Dictionary<BuildingType, int>();
    public List<int> BuildingTypeCount = new List<int>();

    public System.Action OnDataUpdate;

    public FactionData (UnitSide side)
    {
        this.side = side;
        InitBuildingTypeCountMap();
    }

    private void InitBuildingTypeCountMap()
    {

        for (int i = 0; i <= 8; i++)
        {
            BuildingTypeCount.Add(0);
        }

    }
    public void AddTypeBuildingNum(BuildingType type)
    {
        BuildingTypeCount[(int)type] += 1;
        OnDataUpdate?.Invoke();
    }
    public void DecreaseTypeBuildingNum(BuildingType type)
    {
        BuildingTypeCount[(int)type] -= 1;
        OnDataUpdate?.Invoke();

    }

    public bool CanGenerate(BuildingType[] conditions)
    {
        foreach (BuildingType type in conditions)
        {
            if (BuildingTypeCount[(int)type] <= 0) return false;
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
    public void ExchangeGoldToWood()
    {
        if(GoldNum >=90)
        {
            _goldNum -= 90;
            _woodNum += 30;
            OnDataUpdate?.Invoke();
        }
    }
    public int GetWorkerNum()
    {
        int res = workers.Count;
        foreach(var t in trainings)
        {
            for(int i = 0;i<t.trainingQueue.Count;i++)
            {
                var human = t.trainingQueue[i].humanData.humanPrefab.GetComponent<HumanUnit>();
                if(human.role == UnitRole.Worker)
                {
                    res++;
                }
            }
        }
        return res;
    }
    public BuildingUnit GetNearestAllyBase(Unit unit)
    {
        BuildingUnit nearestBase = null;
        float nearestDistance = Mathf.Infinity;
        foreach (var b in buildings)
        {
            if (b.buildingState == BuildingState.InConstruction || b.buildingType == BuildingType.Attack || b.buildingType == BuildingType.Collect) continue;

            float distance = (unit.transform.position - b.transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBase = b;
            }
        }
        return nearestBase;
    }
    public FactionSaveData ToSaveData()
    {
        return new FactionSaveData(this);
    }
}
