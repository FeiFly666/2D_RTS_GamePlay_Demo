using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
public enum BuildingType
{
    Static,//基地
    Attack,//防御塔
    Train,//训练营
    Ranged,//弓箭手前置
    Goblin,//哥布林前置
    Collect,//金矿
    House,//房子
    Exchange//交易所
}
public enum BuildingState
{
    InConstruction,
    ConstructionFinished
}
public class BuildingUnit : Unit
{
    public BuildingState buildingState;
    [Header("检测间隔")]
    [SerializeField] protected float checkFrequency;
    protected float checkTimer = 0f;

    [Header("建造粒子")]
    [SerializeField] private ParticleSystem buildingParticalSystem;

    [Header("毁坏粒子")]
    [SerializeField] private ParticleSystem DeathParticalSystem;

    [Header("建筑物兵种预制体")]
    [SerializeField] private GameObject UnitPrefab;
    [Header("兵种生成位置")]
    [SerializeField] private Vector2[] spawnPositions;
    private Vector2Int cacheCenterPosition;
    private Vector3 centerPos;
    private List<HumanUnit> spawnUnits = new List<HumanUnit>();

    [SerializeField] public BuildingType buildingType;
    [SerializeField] private HumanUnit buildingUnit;

    protected BuildingProcess buildingProcess;
    [SerializeField]public BuildingAction data;

    public bool isStartGenerate = false;

    protected FactionData faction;
    protected override void Awake()
    {
        base.Awake();
        InitData();
        centerPos = sr.gameObject.transform.position;
    }

    protected override void Start()
    {
        base.Start();
        faction = GameManager.Instance.factions[(int)unitSide];

        StartCoroutine(NotifyingBuildingChanged());
        if(!GameManager.Instance.buildings.Contains(this))
        {
            GameManager.Instance.buildings.Add(this);
        }

        if(isStartGenerate)
        {
            CompleteConstruction();
        }

    }
    IEnumerator NotifyingBuildingChanged()
    {
        if(TilemapManager.Instance.isLoading)
        {
            yield break;
        }
        yield return new WaitForSeconds(0.02f);
        TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);
    }
    public void InitData()
    {
        this.stats.FullHP = data.fullHP;

        this.stats.Attack = 0;
        this.stats.Armor = data.armor;
        this.stats.CritChance = 0;
        this.stats.CritMutiple = 0;

        this.stats.currentHP = stats.FullHP;
        this.isStartGenerate = data.isStartGenerate;
    }
    public void InitFoundation()
    {
        this.sr.sprite = data.foundationSprite;
    }
    protected override void Update()
    {
        if(this.buildingState == BuildingState.InConstruction && buildingProcess != null)
        {
            buildingProcess.UpdateProcess(Time.deltaTime);
            return;
        }

        if (Time.time - checkTimer > checkFrequency)
        {
            checkTimer = Time.time;
            UpdateBehaviour();
        }


    }
    protected virtual void UpdateBehaviour()
    {

    }
    public void InitConstruction()
    {
        buildingProcess = new BuildingProcess(data.finishValue, CompleteConstruction);

        sr.sprite = data.foundationSprite;

        this.buildingState = BuildingState.InConstruction;

/*        if (data.peopleAddNum < 0)//占用
        {
            faction.AddPopWeight(data.peopleAddNum);
        }*/
    }

    public BuildingProcess GetBuildingProcess()
    {
        return this.buildingProcess;
    }

    protected void CompleteConstruction()
    {
        sr.sprite = data.completeSprite;

        buildingProcess?.RemoveAllWorkers();

        buildingProcess = null;

        this.buildingState = BuildingState.ConstructionFinished;
        ApplyArea(1);

        if(faction == null)
        {
            faction = GameManager.Instance.factions[(int)unitSide];
        }

        if (data.peopleAddNum > 0)//扩张
        {
            faction.AddTotalPeople(data.peopleAddNum);
        }

        faction.AddTypeBuildingNum(this.buildingType);


        for (int i = 0; i < spawnPositions.Length; i++) 
        {
            SpawnBuildingUnit(spawnPositions[i],i);
        }
    }
    protected void SpawnBuildingUnit(Vector3 position,int sortOrder)
    {
        GameObject go = Instantiate(UnitPrefab,this.transform);

        go.transform.localPosition = position;
        go.transform.localScale = new Vector3(0.68f, 0.68f, 0.68f);

        HumanUnit humanUnit = go.GetComponent<HumanUnit>();
        if (humanUnit != null)
        {
            humanUnit.isBuildingUnit = true;
            humanUnit.unitSide = this.unitSide;

            spawnUnits.Add(humanUnit);
            humanUnit.sg.sortingOrder = sortOrder + 2;
            humanUnit.attackRadius *= 1.4f;
            humanUnit.dectectRadius = humanUnit.attackRadius;
        }

    }
    public override void Death()
    {
        if(isDead) return;
        base.Death();

        buildingProcess?.RemoveAllWorkers();

        foreach(var unit in spawnUnits)
        {
            unit.stats.DecreaseHP(null, 1000000000);
            unit.sr.enabled = false;
        }

        if(this is TrainingBuilding t)
        {
            t.RemoveAllTrainingTask();
        }

        if(this.buildingState == BuildingState.ConstructionFinished)
        {
            if (faction != null)
            {
                if(data.peopleAddNum > 0)//扩张
                {
                    faction.DecreaseTotalPeople(data.peopleAddNum);
                }
/*                else if(data.peopleAddNum < 0)//占用
                {
                    faction.ReleasePopWeight(data.peopleAddNum);
                }*/

                faction.DecreaseTypeBuildingNum(this.buildingType);
            }

            ApplyArea(-1);
        }
        this.buildingType = BuildingType.Static;

        GameManager.Instance.buildings.Remove(this);

        DeathParticalSystem.Play();

        StartCoroutine(AfterDeath());
    }
    public void ApplyArea(int delta) // delta: 1增加, -1移除
    {
        if(delta > 0)
        {
            Vector3Int cellPos = TilemapManager.Instance.WalkableTilemap.WorldToCell(centerPos);

            Vector2Int logicalBase = new Vector2Int(cellPos.x + data.buildingOffset.x + 1, cellPos.y + data.buildingOffset.y + 1);

            cacheCenterPosition = logicalBase;
        }

        TilemapManager.Instance.AddBuildingArea(cacheCenterPosition, data.buildingSize, this.unitSide, delta, this);
    }
    public void BuyThisBuilding()
    {
        buildingProcess?.RemoveAllWorkers();

        foreach (var unit in spawnUnits)
        {
            unit.stats.DecreaseHP(null, 1000000000);
            unit.sr.enabled = false;
        }
        if (this is TrainingBuilding t)
        {
            t.RemoveAllTrainingTask();
        }

        if (this.buildingState == BuildingState.ConstructionFinished)
        {
            if (faction != null)
            {
                if (data.peopleAddNum > 0)//扩张
                {
                    faction.DecreaseTotalPeople(data.peopleAddNum);
                }
/*                else if (data.peopleAddNum < 0)//占用
                {
                    faction.ReleasePopWeight(data.peopleAddNum);
                }*/

                faction.DecreaseTypeBuildingNum(this.buildingType);

                faction.RefundResource(data.goldCost / 2, data.woodCost / 2);
            }

            ApplyArea(-1);
        }

        GameManager.Instance.buildings.Remove(this);
        GameManager.Instance.UnregisterSideUnit(this);

        TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);

        Destroy(this.gameObject);

    }
    public void SetBuildingUnitTarget(Unit target)
    {
        foreach (var unit in spawnUnits)
        {
            if (unit.combatBehaviour.CanAttack(unit,target))
            {
                unit.SetClickTarget(target);
            }
        }
    }
    private bool DeathParticalFinished()
    {
        return !DeathParticalSystem.isPlaying;
    }
    IEnumerator AfterDeath()
    {
        sr.sprite = data.destroyedSprite;

        yield return new WaitForSeconds(0.02f);

        yield return new WaitUntil(DeathParticalFinished);

        TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);

        Destroy(this.gameObject);
    }
    public BuildingSaveData ToSaveData()
    {
        return new BuildingSaveData(this);
    }
    public void LoadData(BuildingSaveData data)
    {
        this.uniqueID = data.ID;

        this.unitSide = (UnitSide)data.unitSide;
        this.transform.position = new Vector3(data.position.x, data.position.y, data.position.z);

        this.buildingState = (BuildingState)data.buildingState;

        this.stats.currentHP = data.currentHP;

        ResumeProcess(data);

        if(this is TrainingBuilding t)
        {
            t.gatherPosition = new Vector2(data.gatherPosition.x, data.gatherPosition.y);

            if(this.buildingState == BuildingState.ConstructionFinished && data.tasks.Count > 0)
            {
                t.ResumeTrainingTask(data);
            }

        }
        if(this is GoldMine g)
        {
            g.ResumeInsideUnits(data.goldMinUnits);
        }
    }

    public void ResumeProcess(BuildingSaveData data)
    {
        if(this.buildingState == BuildingState.ConstructionFinished)
        {
            this.CompleteConstruction();
        }
        else
        {
            InitConstruction();
            this.buildingProcess = new BuildingProcess(data.currentProcess, this.data.finishValue, CompleteConstruction);
        }
    }
}
