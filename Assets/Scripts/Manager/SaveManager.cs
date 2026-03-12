using Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoSingleton<SaveManager>
{
    private string SavePath;
    [SerializeField] public DataCatalog dataCatalog;
    private int SaveVersion = 1;
    private void Start()
    {
        this.SavePath = Application.persistentDataPath +"/Save.json";
    }
    public void SaveGame()
    {
        SaveGameRoot root = new SaveGameRoot();

        root.saveVersion = SaveVersion;

        root.saveName = "1";

        DateTime nowTime = DateTime.Now;

        root.timestamp = $"{nowTime.Year}年{nowTime.Month}月{nowTime.Day}日  {nowTime.Hour}:{nowTime.Minute}";

        root.nextAvailableID = GameManager.Instance.GetAnID();

        foreach(var faction in GameManager.Instance.factions)
        {
            root.factions.Add(faction.ToSaveData());
        }
        
        foreach(var ai in GameManager.Instance.ais)
        {
            root.factionAis.Add(ai.ToSaveData());
        }

        foreach(var human in GameManager.Instance.liveHumanUnits)
        {
            root.allHumans.Add(human.ToSaveData());
        }

        foreach(var building in GameManager.Instance.buildings)
        {
            root.allBuildings.Add(building.ToSaveData());
        }

        foreach(var group in GameManager.Instance.groups)
        {
            root.allGroups.Add(group.ToSaveData());
        }

        foreach(var resource in GameManager.Instance.resources)
        {
            root.allResources.Add(resource.ToSaveData());
        }

        root.fogData = TilemapManager.Instance.SaveCheckStatus();

        var json = JsonConvert.SerializeObject(root,Formatting.Indented);

        File.WriteAllText(SavePath, json);

        Debug.Log("已存入" + SavePath);

    }
    public void LoadGame()
    {
        var json = File.ReadAllText(SavePath);

        if (json == null) return;

        MyInputsystem.Instance.ChangeInputState(InputState.None);

        GameManager.Instance.isPlaying = false;

        foreach (var human in GameManager.Instance.liveHumanUnits)
        {
            Destroy(human.gameObject);
        }

        for (int i = 0; i < GameManager.Instance.sideNum; i++)
        {
            FactionData currentFaction = GameManager.Instance.factions[i];
            currentFaction.humans.Clear();
            currentFaction.buildings.Clear();
        }


        foreach(var builidng in GameManager.Instance.buildings)
        {
            builidng.ApplyArea(-1);
            Destroy(builidng.gameObject);
        }

        foreach(var resource in GameManager.Instance.resources)
        {
            Destroy(resource.gameObject);
        }
        PoolManager.Instance.ReturnAllToPool<Arrow>("Arrow");

        HPBarManager.Instance.ReturnAllBar();

        TilemapManager.Instance.isLoading = true;;

        TilemapManager.Instance.InitPathfing();

        UIManager.Instance.actionBar.CloseActionBar();

        GameManager.Instance.liveHumanUnits.Clear();
        GameManager.Instance.buildings.Clear();
        GameManager.Instance.groups.Clear();
        GameManager.Instance.resources.Clear();

        
        SaveGameRoot root = JsonConvert.DeserializeObject<SaveGameRoot>(json);

        GameManager.Instance.InitFactions();

        foreach (var factionData in root.factions)
        {
            GameManager.Instance.factions[factionData.side].ResetFactionData(factionData);
        }

        foreach (var resourceData in root.allResources)
        {
            ResourceAction data = dataCatalog.GetResourceByID(resourceData.SOID);

            Vector3 spawnPoint = new Vector3(resourceData.position.x, resourceData.position.y, resourceData.position.z);

            ResourceUnit resource = UnitFactory.CreatResource(data, spawnPoint);

            resource.LoadData(resourceData);

            //GameManager.Instance.RegisterSideUnit(resource);
        }

        foreach (var buildingData in root.allBuildings)
        {
            BuildingAction data = dataCatalog.GetBuildingByID(buildingData.buildingSOID);

            Vector3 spawnPos = new Vector3(buildingData.position.x, buildingData.position.y, buildingData.position.z);

            BuildingUnit building = UnitFactory.CreateBuilding(data, spawnPos);

            building.LoadData(buildingData);

            //GameManager.Instance.RegisterSideUnit(building);
        }

        foreach (var humanData in root.allHumans)
        {
            HumanAction data = dataCatalog.GetHumanByID(humanData.HumanSOID);

            Vector3 spawnPos = new Vector3(humanData.position.x, humanData.position.y, humanData.position.z);

            HumanUnit human = UnitFactory.CreateHuman(data, spawnPos);

            human.isNeedInitPosition = false;

            human.LoadData(humanData);

            //GameManager.Instance.RegisterSideUnit(human);

        }

        foreach (var groupData in root.allGroups)
        {
            UnitGroup newGroup = new UnitGroup();
            newGroup.LoadData(groupData);
        }

        GameManager.Instance.InitID(root.nextAvailableID);

        TilemapManager.Instance.isLoading = false;
        TilemapManager.Instance.LoadCheckStatus(root.fogData);

        if(GameManager.Instance.isNeedFog)
        {
            FogManager.Instance.GetPathFinding();
            FogManager.Instance.InitFOW();
        }

        TilemapManager.Instance.UpdateAllNodes();

        //开始赋予灵魂
        //小队移动唤醒
        foreach (var group in GameManager.Instance.groups.ToList())
        {
            group.ResumeLogic();

        }
        //单体移动唤醒
        foreach (var human in GameManager.Instance.liveHumanUnits.ToList())
        {
            if (human.ai.IsUnitInGroup) continue;
            if(!GameManager.Instance.groups.Contains(human.ai.currentGroup))
                 human.ResumeLogic();
        }
        GameManager.Instance.InitFactionAI(true, root.factionAis);
        GameManager.Instance.isPlaying = true;
    }
}
