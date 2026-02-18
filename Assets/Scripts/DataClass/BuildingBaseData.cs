using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBaseData
{
    public string buildingName;
    public string buildingDesciption;
    public int goldCost;
    public int woodCost;
    public float finishValue;

    public BuildingBaseData(string name, string description, int gold, int wood, float finishValue)
    {
        this.buildingName = name;
        this.buildingDesciption = description;
        this.goldCost = gold;
        this.woodCost = wood;
        this.finishValue = finishValue;
    }
    public BuildingBaseData(BuildingBaseData data)
         : this(data.buildingName, data.buildingDesciption,
                data.goldCost, data.woodCost, data.finishValue)
    {

    }

}
