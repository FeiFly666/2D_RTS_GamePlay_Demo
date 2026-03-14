using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    public UnitSide playerSide = UnitSide.A;
    public EnemyMode currentEnemyMode;
    public int sideNum;
    [SerializeField]public Arrow arrowPrefab;
    public static int[] EnemyMasks = new int[5];


    public bool isPlaying = false;
    public bool isNeedFog = false;

    private int availableID = 0;
    [SerializeField] public Transform HumanRoot;
    [SerializeField] public Transform BuildingRoot;
    [SerializeField] public Transform ResourceRoot;

    [SerializeField] public List<HumanUnit> liveHumanUnits = new List<HumanUnit>(100);
    [SerializeField] public List<BuildingUnit> buildings = new List<BuildingUnit>(50);
    [SerializeField] public List<UnitGroup> groups = new List<UnitGroup>(10);
    [SerializeField] public List<ResourceUnit> resources = new List<ResourceUnit>(100);

/*    public Dictionary<UnitSide, List<HumanUnit>> sideHuman = new Dictionary<UnitSide, List< HumanUnit>>();
    public Dictionary<UnitSide, List<BuildingUnit>> sideBuilding = new Dictionary<UnitSide, List<BuildingUnit>>();*/
    public Dictionary<int, List<ResourceUnit>> areaResources = new Dictionary<int, List<ResourceUnit>>();

    public List<FactionData> factions = new List<FactionData>();
    public List<FactionAI> ais = new List<FactionAI>();

    protected override void OnStart()
    {
        FilePath.Init();

        MyInputsystem.Instance.inputState = InputState.None;
        MyInputsystem.Instance.isGameStart = true;

        InitEnemyLayers();

        InitFactions();

        ;
    }
    private void Start()
    {
        //UnitSpawnManager.Instance.InitAllUnitPools();
        PoolManager.Instance.CreatePool("Arrow", arrowPrefab, 1000,this.transform);
        factions[(int)playerSide].AddGold(0);


        InitFactionAI();

        isPlaying = true;
    }

    private void Update()
    {
        MyInputsystem.Instance.UpdateMouseInput();
    }

    public void InitFactions()
    {
        isNeedFog = LevelOption.Instance.isNeedFog;
        currentEnemyMode = LevelOption.Instance.enemyMode;

        if (factions.Count > 0)
        {
            UIManager.Instance.LogOutFactionDataDisplay();
            foreach(var ai in ais)
            {
                ai.UnRegistAction();
            }
        }

        factions.Clear();
        for (int i = 0; i < sideNum; i++)
        {
            factions.Add(new FactionData((UnitSide)i));

        }

        UIManager.Instance.RegisterFactionDataDisplay();
    }
    public void InitFactionAI(bool isLoad = false,List<FactionAISaveData>datas = null)
    {
        if (ais.Count > 0)
        {
            for (int i = ais.Count - 1; i >= 0; i--)
            {
                Destroy(ais[i].gameObject);
            }
            ais.Clear();
        }
        for (int i = 0; i < sideNum; i++)
        {
            if (i != (int)playerSide)
            {
                GameObject ai = new GameObject("FacionAI");

                FactionAI factionAI = ai.AddComponent<FactionAI>();

                factionAI.unitSide = (UnitSide)i;

                factionAI.InitAI();

                ais.Add(factionAI);
            }

        }
        if(isLoad)
        {
            foreach(var data in datas)
            {
                foreach(var ai in ais)
                {
                    if(ai.unitSide == data.side)
                    {
                        ai.LoadData(data); 
                        break;
                    }
                }
            }
        }
        if (LevelOption.Instance.enemyNum < sideNum - 1)
        {
            int n = sideNum - 1 - LevelOption.Instance.enemyNum;
            for (int i = ais.Count - 1; i >= 0; i--)
            {
                int j = Random.Range(0, i + 1);
                FactionAI temp = ais[j];
                ais[j] = ais[i];
                ais[i] = temp;
            }
            for (int i = 0; i < n; i++)
            {
                FactionDestroy(ais[i].unitSide);
            }
        }
    }

    public void FactionDestroy(UnitSide side)
    {
        FactionData faction = factions[(int)side];

        foreach (var human in faction.humans.ToList())
        {
            if(human != null && human.stats != null)
                human.stats.DecreaseHP(null, 1000000);
        }
        foreach(var building in faction.buildings.ToList())
        {
            if (building != null && building.stats != null)
                building.stats.DecreaseHP(null, 1000000);
        }

        if (side == playerSide)
        {
            UIManager.Instance.gameLose.SetActive(true);
            isPlaying = false;
            return;
        }

        FactionAI factionAI = null;
        foreach(var ai in ais)
        {
            if(ai != null)
            {
                if(ai.unitSide == side)
                {
                    factionAI = ai;
                    break;
                }
            }
        }

        Destroy(factionAI.gameObject);
        ais.Remove(factionAI);

        if(ais.Count == 0)
        {
            UIManager.Instance.gameWin.SetActive(true);
            isPlaying = false;
        }

    }

    public void RegisterSideUnit(Unit unit)
    {
        if (factions.Count < sideNum)
        {
            InitFactions();
        }
        if (unit is HumanUnit human)
        {
            /*if(!sideHuman.ContainsKey(human.unitSide))
            {
                sideHuman[human.unitSide] = new List<HumanUnit>();
            }
            sideHuman[human.unitSide].Add(human);*/
            if (!human.isBuildingUnit)
            {
                factions[(int)unit.unitSide].humans.Add(human);
                if (human is Worker worker) factions[(int)unit.unitSide].workers.Add(worker);
                human.gameObject.transform.SetParent(HumanRoot);
            }
            
        }
        else if(unit is BuildingUnit building)
        {
            /*if (!sideBuilding.ContainsKey(building.unitSide))
            {
                sideBuilding[building.unitSide] = new List<BuildingUnit>();
            }
            sideBuilding[building.unitSide].Add(building);*/
            factions[(int)unit.unitSide].buildings.Add(building);

            if(building is TrainingBuilding train)
                factions[(int)unit.unitSide].trainings.Add(train);
            if(building is GoldMine gold)
                factions[(int)unit.unitSide].goldMines.Add(gold);

            building.gameObject.transform.SetParent(BuildingRoot);
            if(unit.unitSide != playerSide)
            {
                FactionAI ai = null;
                foreach(var a in ais)
                {
                    if(a.unitSide == unit.unitSide)
                    {
                        ai = a; break;
                    }
                }
                if(ai != null)
                {
                    building.stats.OnHurtByUnits += ai.tacitical.OnBuildingHurt;
                }
            }

        }
        else if(unit is ResourceUnit resource)
        {
            if(!areaResources.ContainsKey(resource.resourceAreaID))
            {
                areaResources[resource.resourceAreaID] = new List<ResourceUnit>();
            }
            areaResources[resource.resourceAreaID].Add(resource);
            resource.gameObject.transform.SetParent(ResourceRoot);
        }
    }
    public void UnregisterSideUnit(Unit unit)
    {
        if(unit is HumanUnit human)
        {
            /*sideHuman[human.unitSide].Remove(human);*/
            if(!human.isBuildingUnit)
            {
                factions[(int)unit.unitSide].humans.Remove(human);
                if (human is Worker worker) factions[(int)unit.unitSide].workers.Remove(worker);
            }
        }
        else if (unit is BuildingUnit building)
        {
            /*sideBuilding[building.unitSide].Remove(building);*/
            factions[(int)unit.unitSide].buildings.Remove(building);
            if (building is TrainingBuilding train)
                factions[(int)unit.unitSide].trainings.Remove(train);
            if (building is GoldMine gold)
                factions[(int)unit.unitSide].goldMines.Remove(gold);
            if (unit.unitSide != playerSide)
            {
                FactionAI ai = null;
                foreach (var a in ais)
                {
                    if (a.unitSide == unit.unitSide)
                    {
                        ai = a; break;
                    }
                }
                if (ai != null)
                {
                    building.stats.OnHurtByUnits -= ai.tacitical.OnBuildingHurt;
                }
            }

            if (isPlaying)
            {
                if (factions[(int)unit.unitSide].buildings.Count <= 0)
                {
                    FactionDestroy(unit.unitSide);
                }
            }
        }
        else if(unit is ResourceUnit resource)
        {
            if (areaResources.ContainsKey(resource.resourceAreaID))
            {
                areaResources[resource.resourceAreaID].Remove(resource);
            }
        }
    }

    private void InitEnemyLayers()
    {
        int s0 = 1 << LayerMask.NameToLayer("Side0");
        int s1 = 1 << LayerMask.NameToLayer("Side1");
        int s2 = 1 << LayerMask.NameToLayer("Side2");
        int s3 = 1 << LayerMask.NameToLayer("Side3");
        int s4 = 1 << LayerMask.NameToLayer("Side4");
        int all = s0 | s1 | s2 | s3 | s4;

        EnemyMasks[0] = all ^ s0; 
        EnemyMasks[1] = all ^ s1; 
        EnemyMasks[2] = all ^ s2; 
        EnemyMasks[3] = all ^ s3;
        EnemyMasks[4] = all ^ s4;

    }
    public int GetAnID()
    {
        int res = availableID;
        availableID++;
        return res;
    }
/*    public BuildingUnit GetNearestAllyBase(Unit unit)
    {
        BuildingUnit nearestBase = null;
        float nearestDistance = Mathf.Infinity;
        foreach(var b in buildings)
        {
            if (b.unitSide != unit.unitSide || b.buildingState == BuildingState.InConstruction || b.buildingType == BuildingType.Attack) continue;

            float distance = (unit.transform.position - b.transform.position).sqrMagnitude;
            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBase = b;
            }
        }
        return nearestBase;
    }*/
    public void InitID(int newID)
    {
        this.availableID = newID;
    }

    public void BackToMenu()
    {
        StartCoroutine(LoadScene());
    }
    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Menu");
        while (!op.isDone)
        {
            yield return null;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SaveData()
    {
        SaveManager.Instance.SaveGame();
    }
    public void LoadData()
    {
        SaveManager.Instance.LoadGame();
    }
}
