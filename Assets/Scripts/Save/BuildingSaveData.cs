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

    //训练建筑信息
    public List<TrainTaskSaveData> tasks = new List<TrainTaskSaveData>();
    public SerializableVector3 gatherPosition;

    public List<InsideGoldMinUnit> goldMinUnits= new List<InsideGoldMinUnit>();

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

        if(unit is TrainingBuilding t)
        {
            foreach(var task in t.trainingQueue)
            {
                TrainTaskSaveData data = task.ToSaveData();
                tasks.Add(data);
            }

            this.gatherPosition = new SerializableVector3(t.gatherPosition);
        }

        if(unit is GoldMine g)
        {
            this.goldMinUnits = g.humanInsideData.ToList();
        }
    }

}
