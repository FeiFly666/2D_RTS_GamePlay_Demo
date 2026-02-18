using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildingSaveData
{
    public int ID;
    public string buildingSOID;

    public int unitSide;
    public SerializableVector3 position;

    public int buildingState;
    public float currentProcess = -1;

    public float currentHP;

    public BuildingSaveData()
    {

    }
    public BuildingSaveData(BuildingUnit unit)
    {
        this.ID = unit.uniqueID;

        this.buildingSOID = unit.data.ID;

        this.unitSide = (int)unit.unitSide;
        this.position = new SerializableVector3(unit.transform.position);

        this.buildingState = (int)unit.buildingState;
        if(unit.buildingState == BuildingState.InConstruction)
        {
            currentProcess = unit.GetBuildingProcess().process;
        }

        this.currentHP = unit.stats.currentHP;
    }

}
