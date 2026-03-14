using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FactionAI : MonoBehaviour
{
    public UnitSide unitSide;

    public Vector3 basePosition;

    public List<BuildingAction> actionList;
    public Dictionary<BuildingType, List<BuildingAction>> buildingMap = new Dictionary<BuildingType, List<BuildingAction>>();

    public List<HumanAction> humanActionList;
    public Dictionary<UnitRole, List<HumanAction>> humanMap = new Dictionary<UnitRole, List<HumanAction>>();


    public FactionData faction;

    public AIStrategy strategy;
    public AIProduction production;
    public AIEconomy economy;
    public AITactical tacitical;

    [SerializeField]private bool isInit = false;
    [SerializeField] private bool isLoad = false;
    public UnitSide targetSide;
    public int attackTimes = 0;
    public bool prepareForAttack = false;
    public bool attack = false;
    public int nextAttackNum = 20;

    public int maxBuildingNum = 80;
    public int maxAttackBuildingNum = 25;

    [SerializeField] private float executeFrequency = 0.5f;
    private float executeTimer;

    [SerializeField] private float requestBuildingChangeTime = 25f;
    private float rBChangeTimer;

    public bool CanAttack = true;
    private void Awake()
    {
        strategy = new AIStrategy(this);
        production = new AIProduction(this);
        economy = new AIEconomy(this);
        tacitical = new AITactical(this);
    }
    void Start()
    {
        
    }
    public void InitAI()
    {
        faction = GameManager.Instance.factions[(int)unitSide];

        faction.OnDataUpdate += OnTotalPeopleChanged;

        BuildingUnit basement = faction.buildings.Find(b => b.buildingType == BuildingType.Static);

        if(basement == null) { return; }
        List<Action> action = basement.Actions;
        basePosition = basement.transform.position;

        InitBuildingMap(action);

        List<Action> humanAction = buildingMap[BuildingType.Train][0].structurePrefab.GetComponent<TrainingBuilding>().Actions;

        InitHumanMap(humanAction);

        executeTimer = -0.5f;

        if(!isLoad)
        {
            if (LevelOption.Instance.enemyMode == EnemyMode.Free)
            {
                tacitical.RandomTargetSide();
            }
            tacitical.RandomNextAttackNum();
        }
        isLoad = true;

        isInit = true;
    }
    private void InitBuildingMap(List<Action> action)
    {
        foreach (Action a in action)
        {
            BuildingAction building = a as BuildingAction;
            BuildingType type = building.structurePrefab.GetComponent<BuildingUnit>().buildingType;
            if (!buildingMap.ContainsKey(type))
            {
                buildingMap[type] = new List<BuildingAction>();
            }
            buildingMap[type].Add(building);
        }
    }

    private void InitHumanMap(List<Action> action)
    {
        foreach(var a in action)
        {
            HumanAction human = a as HumanAction;
            UnitRole role = human.humanPrefab.GetComponent<HumanUnit>().role;
            if (!humanMap.ContainsKey(role))
            {
                //Debug.LogError((int)role);
                humanMap[role] = new List<HumanAction>();
            }
            humanMap[role].Add(human);
        }
    }
    void Update()
    {
        if(!isInit)
        {
            InitAI();
            return;
        }
        if(!GameManager.Instance.isPlaying)
        {
            return;
        }
        executeTimer += Time.deltaTime;

        if(strategy.requestBuilding != null)
        {
            rBChangeTimer += Time.deltaTime;
            if(rBChangeTimer > requestBuildingChangeTime)
            {
                rBChangeTimer = 0;
                strategy.requestBuilding = null;
            }
        }
        else
        {
            rBChangeTimer = 0;
        }

        if(executeTimer>= executeFrequency)
        {
            executeTimer = 0;
            float randomRest = Random.Range(0, 1f);
            if (randomRest < 0.3f) return;
            strategy.UpdateLogic();
            economy.UpdateLogic();
            production.UpdateLogic();
            tacitical.UpdateLogic();
        }
    }
    public void OnTotalPeopleChanged()
    {
        nextAttackNum = Mathf.Min(faction.TotalPeopleNum - 10, nextAttackNum);
    }
    public void UnRegistAction()
    {
        faction.OnDataUpdate -= OnTotalPeopleChanged;
    }
    public FactionAISaveData ToSaveData()
    {
        return new FactionAISaveData(this);
    }

    public void LoadData(FactionAISaveData data)
    {
        faction = GameManager.Instance.factions[(int)data.side];
        this.attackTimes = data.attackTimes;
        this.nextAttackNum = data.nextAttackNum;

        this.targetSide = data.targetSide;
        this.tacitical.lastTargetSide = data.lastTargetSide;

        int targetID = data.groupTargetID;

        foreach(var member in data.groupMembers)
        {
            HumanUnit unit = GameManager.Instance.liveHumanUnits.Find(u => u.uniqueID == member);

            if(unit != null)
            {
                this.tacitical.groupMembers.Add(unit);
            }
        }
        if (tacitical.groupMembers.Count > 0 && tacitical.attackGroup == null)
        {
            tacitical.ResumeAttackGroup();
            if (tacitical.attackGroup.members.Count >= nextAttackNum || data.attack)
            {
                attack = true;
                BuildingUnit targetBuilding = GameManager.Instance.buildings.Find(u => u.uniqueID == targetID);
                if(targetBuilding != null)
                {
                    tacitical.ResumeGroupMoving(targetBuilding);
                }
            }
            else
            {
                prepareForAttack = true;
            }
        }

        isLoad = true;
    }

}
