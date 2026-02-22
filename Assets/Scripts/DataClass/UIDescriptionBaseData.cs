using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct UIDescriptionBaseData
{
    public string Name;
    public string Desciption;
    public bool isNone;
    public int goldCost;
    public int woodCost;
    public float finishValue;
    public int peopleNum;

    public static UIDescriptionBaseData Empty()
    {
        UIDescriptionBaseData data = new UIDescriptionBaseData();
        data.isNone = true;
        return data;
    }
    public UIDescriptionBaseData(string name, string description, int gold, int wood, float finishValue, int peopleNum)
    {
        this.Name = name;
        this.Desciption = description;
        this.isNone = false;
        this.goldCost = gold;
        this.woodCost = wood;
        this.finishValue = finishValue;
        this.peopleNum = peopleNum;
    }
    public UIDescriptionBaseData(UIDescriptionBaseData data)
         : this(data.Name, data.Desciption,
                data.goldCost, data.woodCost, data.finishValue, data.peopleNum)
    {

    }

}
