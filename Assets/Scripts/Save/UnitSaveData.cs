using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class UnitSaveData
{
    public string HumanSOID;
    public int ID;
    public int unitSide;
    public SerializableVector3 position;

    public float currentHP;

    public bool isForcingTarget;
    public int targetID;

    public bool isForcingMoving;
    public SerializableVector3 destiantion =new SerializableVector3( -1500, -1500, -1500 );

    public bool isReturningHome;
    public int holdResourceNum = 0;
    public int currentChopNum = 0;
    public int resourceAreaID = -1;
    public UnitSaveData()
    {

    }

    public UnitSaveData(HumanUnit unit)
    {
        this.HumanSOID = unit.data.ID;
        this.ID = unit.uniqueID;
        this.unitSide = (int)unit.unitSide;
        this.position = new SerializableVector3(unit.transform.position);

        this.currentHP = unit.stats.currentHP;

        this.isForcingTarget = unit.isForcingTarget;
        this.targetID = unit.targetID;

        this.isForcingMoving = unit.IsForcingMoving;
        this.destiantion =  new SerializableVector3(unit.GetDestination());

        this.isReturningHome = unit.isReturningHome;

        if(unit is Worker worker)
        {
            holdResourceNum = worker.holdResourceNum;
            resourceAreaID = worker.ResourceAreaID;    
            currentChopNum = worker.currentChopNum;
        }

    }


}