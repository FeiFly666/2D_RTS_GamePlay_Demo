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
    [SerializeField] private DataCatalog dataCatalog;
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

        var json = JsonConvert.SerializeObject(root,Formatting.Indented);

        File.WriteAllText(SavePath, json);

        Debug.Log("已存入" + SavePath);

    }
    public void LoadGame()
    {
        var json = File.ReadAllText(SavePath);

        if (json == null) return;

        MyInputsystem.Instance.ChangeInputState(InputState.None);

        foreach (var human in GameManager.Instance.liveHumanUnits)
        {
            Destroy(human.gameObject);
        }

        foreach(var builidng in GameManager.Instance.buildings)
        {
            Destroy(builidng.gameObject);
        }

        foreach(var resource in GameManager.Instance.resources)
        {
            Destroy(resource.gameObject);
        }
        PoolManager.Instance.ReturnAllToPool<Arrow>("Arrow");
        TilemapManager.Instance.isLoading = true;
        TilemapManager.Instance.InitPathfing();

        GameManager.Instance.groups.Clear();

        GameManager.Instance.liveHumanUnits.Clear();
        GameManager.Instance.buildings.Clear();
        GameManager.Instance.groups.Clear();
        GameManager.Instance.resources.Clear();



        
        SaveGameRoot root = JsonConvert.DeserializeObject<SaveGameRoot>(json);

        foreach (var resourceData in root.allResources)
        {
            ResourceAction data = dataCatalog.GetResourceByID(resourceData.SOID);

            Vector3 spawnPoint = new Vector3(resourceData.position.x, resourceData.position.y, resourceData.position.z);

            GameObject resourceGO = Instantiate(data.resourcePrefab, spawnPoint, Quaternion.identity);

            ResourceUnit resource = resourceGO.GetComponent<ResourceUnit>();
            resource.LoadData(resourceData);

            if (!GameManager.Instance.resources.Contains(resource))
            {
                GameManager.Instance.resources.Add(resource);
            }
        }

        foreach (var buildingData in root.allBuildings)
        {
            BuildingAction data = dataCatalog.GetBuildingByID(buildingData.buildingSOID);

            Vector3 spawnPos = new Vector3(buildingData.position.x, buildingData.position.y, buildingData.position.z);

            BuildingUnit building = BuildingFactory.CreateBuilding(data, spawnPos);
            building.LoadData(buildingData);

            if(!GameManager.Instance.buildings.Contains(building))
                GameManager.Instance.buildings.Add(building);
            
        }

        foreach (var humanData in root.allHumans)
        {
            HumanAction data = dataCatalog.GetHumanByID(humanData.HumanSOID);

            Vector3 spawnPos = new Vector3(humanData.position.x, humanData.position.y, humanData.position.z);
            GameObject humanGo = Instantiate(data.humanPrefab, spawnPos, Quaternion.identity);

            HumanUnit human = humanGo.GetComponent<HumanUnit>();
            human.isNeedInitPosition = false;

            if (!GameManager.Instance.liveHumanUnits.Contains(human))
                GameManager.Instance.liveHumanUnits.Add(human);

            human.LoadData(humanData);

        }

        foreach (var groupData in root.allGroups)
        {
            UnitGroup newGroup = new UnitGroup();
            newGroup.LoadData(groupData);
        }

        GameManager.Instance.InitID(root.nextAvailableID);

        TilemapManager.Instance.isLoading = false;
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
            human.ResumeLogic();
        }
    }
}
