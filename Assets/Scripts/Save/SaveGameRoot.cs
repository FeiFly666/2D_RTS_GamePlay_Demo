using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class SaveGameRoot
{
    public int saveVersion;
    public string saveName;// 存档名称
    public string timestamp;// 存档时间

    public string currentLevelName;//地图名称

    public List<FactionSaveData> factions = new List<FactionSaveData>();
    public List<UnitSaveData> allHumans = new List<UnitSaveData>();
    public List<BuildingSaveData> allBuildings = new List<BuildingSaveData>();
    public List<GroupSaveData> allGroups = new List<GroupSaveData>();
    public List<ResourceSaveData> allResources = new List<ResourceSaveData>();
    public byte[] fogData;

    public int nextAvailableID;// GameManager全局 ID 计数器



}

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public SerializableVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
}