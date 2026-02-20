using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TrainTaskSaveData
{
    public string HumanDataOS;

    public float remainTime;


    public TrainTaskSaveData()
    {

    }

    public TrainTaskSaveData(TrainTask task)
    {
        this.HumanDataOS = task.humanData.ID;

        this.remainTime = task.remainTime;
    }

}
