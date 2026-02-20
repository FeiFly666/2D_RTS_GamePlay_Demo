using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[Serializable]
public class TrainTask
{
    public HumanAction humanData;
    public float remainTime;
    public float totalTime;

    public TrainTask(HumanAction humanData)
    {
        this.humanData = humanData;
        this.totalTime = this.remainTime = humanData.trainingTime;

    }

    public TrainTask(TrainTaskSaveData data)
    {
        this.humanData = SaveManager.Instance.dataCatalog.GetHumanByID(data.HumanDataOS);
        this.remainTime = data.remainTime;
        this.totalTime = this.humanData.trainingTime;
    }


    public TrainTaskSaveData ToSaveData()
    {
        return new TrainTaskSaveData(this);
    }
}

