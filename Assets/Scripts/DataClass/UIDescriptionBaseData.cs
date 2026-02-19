using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDescriptionBaseData
{
    public string Name;
    public string Desciption;
    public int goldCost;
    public int woodCost;
    public float finishValue;

    public UIDescriptionBaseData(string name, string description, int gold, int wood, float finishValue)
    {
        this.Name = name;
        this.Desciption = description;
        this.goldCost = gold;
        this.woodCost = wood;
        this.finishValue = finishValue;
    }
    public UIDescriptionBaseData(UIDescriptionBaseData data)
         : this(data.Name, data.Desciption,
                data.goldCost, data.woodCost, data.finishValue)
    {

    }

}
