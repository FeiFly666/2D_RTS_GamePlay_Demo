using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "DataOS/BuildingData")]
public class BuildingDataOS : ScriptableObject
{
    [SerializeField] private GameObject StructurePrefab;
    [SerializeField] private int PeopleAddNum;
    [SerializeField] private string BuildingName;
    [SerializeField] private string BuildingDescription;
    [SerializeField] private int GoldCost;
    [SerializeField] private int WoodCost;
    [SerializeField] private int FullHP;
    [SerializeField] private int Armor;
    [SerializeField] private Vector3Int BuildingSize;
    [SerializeField] private Vector3Int BuildingOffset;
    [SerializeField] private float FinishValue;
    [SerializeField] private bool IsStartGenerate;
    [SerializeField] public BuildingType[] conditions;

    public GameObject structurePrefab => StructurePrefab;
    public int peopleAddNum => PeopleAddNum;
    public string buildingName => BuildingName;
    public string buildingDescription => BuildingDescription;
    public int goldCost => GoldCost;
    public int woodCost => WoodCost;
    public int fullHP => FullHP;
    public int armor => Armor;
    public Vector3Int buildingSize => BuildingSize;
    public Vector3Int buildingOffset => BuildingOffset;
    public float finishValue => FinishValue;
    public bool isStartGenerate => IsStartGenerate; 
}
