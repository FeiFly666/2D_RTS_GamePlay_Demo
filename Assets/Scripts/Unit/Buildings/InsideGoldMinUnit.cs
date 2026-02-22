using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public  class InsideGoldMinUnit
{
    public int UnitID;

    public string data;

    public float humanCurrentHP;

    public bool isWorker;

    public InsideGoldMinUnit() { }


    public InsideGoldMinUnit(HumanUnit unit)
    {
        UnitID = unit.uniqueID;

        humanCurrentHP = unit.stats.currentHP;

        data = unit.data.ID;

        isWorker = unit is Worker;
    }

    public void ToSaveData()
    {

    }
}
