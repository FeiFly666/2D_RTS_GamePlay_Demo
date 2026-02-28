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

    private bool isInit = false;

    [SerializeField] private float executeFrequency = 0.5f;
    private float executeTimer;
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

        BuildingUnit basement = faction.buildings.Find(b => b.buildingType == BuildingType.Static);

        if(basement == null) { return; }
        List<Action> action = basement.Actions;
        basePosition = basement.transform.position;

        InitBuildingMap(action);

        List<Action> humanAction = buildingMap[BuildingType.Train][0].structurePrefab.GetComponent<TrainingBuilding>().Actions;

        InitHumanMap(humanAction);

        executeTimer = -0.5f;

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
        executeTimer += Time.deltaTime;
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

    public int GetWorkerNum()
    {
        return faction.workers.Count;
    }
    public int GetCollectWorkerNum()
    {
        int count = 0;
        foreach(var worker in  faction.workers)
        {
            if(worker.ResourceAreaID !=-1) count++;
        }
        return count;
    }
}
