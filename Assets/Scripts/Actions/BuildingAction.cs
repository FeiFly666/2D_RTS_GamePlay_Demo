using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "BuildingAction",menuName = "Action/BuildingAction")]


public class BuildingAction : Action
{
    [SerializeField] private Sprite FoundationSprite;
    [SerializeField] private Sprite CompleteSprite;
    [SerializeField] private Sprite DestroyedSprite;
    [SerializeField] private GameObject StructurePrefab;
    [SerializeField] private BuildingDataOS buildingDataOS;


    public Sprite foundationSprite => FoundationSprite;
    public Sprite completeSprite => CompleteSprite;
    public Sprite destroyedSprite => DestroyedSprite;
    public GameObject structurePrefab => StructurePrefab;
    public string buildingName => buildingDataOS.buildingName;
    public string buildingDescription => buildingDataOS.buildingDescription;
    public int peopleAddNum => buildingDataOS.peopleAddNum;
    public int goldCost => buildingDataOS.goldCost;
    public int woodCost => buildingDataOS.woodCost;
    public int fullHP => buildingDataOS.fullHP;
    public int armor => buildingDataOS.armor;
    public Vector3Int buildingSize => buildingDataOS.buildingSize;
    public Vector3Int buildingOffset => buildingDataOS.buildingOffset;
    public float finishValue => buildingDataOS.finishValue;
    public bool isStartGenerate => buildingDataOS.isStartGenerate;
    public override void ExecuteAction(UnitSide unitSide)
    {

        BuildingManager.Instance.CanclePlacement();
        BuildingManager.Instance.StartPlacement(this, unitSide);
    }

    public UIDescriptionBaseData GetBuildingBaseData()
    {
        return new UIDescriptionBaseData(this.buildingName, this.buildingDescription, this.goldCost, this.woodCost, this.finishValue);
    }
}
