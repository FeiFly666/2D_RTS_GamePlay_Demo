using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    public UnitSide playerSide = UnitSide.A;
    public int sideNum;
    [SerializeField]public Arrow arrowPrefab;
    public static int[] EnemyMasks = new int[5];

    public bool isNeedFog = false;

    private int availableID = 0;

    [SerializeField] public List<HumanUnit> liveHumanUnits = new List<HumanUnit>(100);
    [SerializeField] public List<BuildingUnit> buildings = new List<BuildingUnit>(50);
    [SerializeField] public List<UnitGroup> groups = new List<UnitGroup>(10);
    [SerializeField] public List<ResourceUnit> resources = new List<ResourceUnit>(100);

    public Dictionary<UnitSide, List<HumanUnit>> sideHuman = new Dictionary<UnitSide, List< HumanUnit>>();
    public Dictionary<UnitSide, List<BuildingUnit>> sideBuilding = new Dictionary<UnitSide, List<BuildingUnit>>();
    public Dictionary<int, List<ResourceUnit>> areaResources = new Dictionary<int, List<ResourceUnit>>();

    public List<FactionData> factions = new List<FactionData>();

    protected override void OnStart()
    {
        FilePath.Init();

        MyInputsystem.Instance.inputState = InputState.None;

        InitEnemyLayers();

        InitFactions();
    }
    private void Start()
    {
        PoolManager.Instance.CreatePool("Arrow", arrowPrefab, 300);
        
    }

    private void Update()
    {
        MyInputsystem.Instance.UpdateMouseInput();

        if(Input.GetKey(KeyCode.W))
        {
            Vector3 a = Camera.main.transform.position;

            a.y += 10 * Time.deltaTime;

            Camera.main.transform.position = a;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 a = Camera.main.transform.position;

            a.y -= 10 * Time.deltaTime;

            Camera.main.transform.position = a;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Vector3 a = Camera.main.transform.position;

            a.x += 10 * Time.deltaTime;

            Camera.main.transform.position = a;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 a = Camera.main.transform.position;

            a.x -= 10 * Time.deltaTime;

            Camera.main.transform.position = a;
        }
    }

    private void InitFactions()
    {
        for (int i = 0; i < sideNum; i++)
        {
            factions.Add(new FactionData((UnitSide)i));
        }
    }
    public void RegisterSideUnit(Unit unit)
    {
        if(unit is HumanUnit human)
        {
            if(!sideHuman.ContainsKey(human.unitSide))
            {
                sideHuman[human.unitSide] = new List<HumanUnit>();
            }
            sideHuman[human.unitSide].Add(human);
        }
        else if(unit is BuildingUnit building)
        {
            if (!sideBuilding.ContainsKey(building.unitSide))
            {
                sideBuilding[building.unitSide] = new List<BuildingUnit>();
            }
            sideBuilding[building.unitSide].Add(building);
        }
        else if(unit is ResourceUnit resource)
        {
            if(!areaResources.ContainsKey(resource.resourceAreaID))
            {
                areaResources[resource.resourceAreaID] = new List<ResourceUnit>();
            }
            areaResources[resource.resourceAreaID].Add(resource);
        }
    }
    public void UnregisterSideUnit(Unit unit)
    {
        if(unit is HumanUnit human)
        {
            sideHuman[human.unitSide].Remove(human);
        }
        else if (unit is BuildingUnit building)
        {
            sideBuilding[building.unitSide].Remove(building);
        }
        else if(unit is ResourceUnit resource)
        {
            areaResources[resource.resourceAreaID].Remove(resource);
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
    public BuildingUnit GetNearestAllyBase(Unit unit)
    {
        BuildingUnit nearestBase = null;
        float nearestDistance = Mathf.Infinity;
        foreach(var b in buildings)
        {
            if (b.unitSide != unit.unitSide || b.buildingState == BuildingState.InConstruction || b.buildingType == BuildingType.Attack) continue;

            float distance = Vector2.Distance(unit.transform.position, b.transform.position);
            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBase = b;
            }
        }
        return nearestBase;
    }
    public void InitID(int newID)
    {
        this.availableID = newID;
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
