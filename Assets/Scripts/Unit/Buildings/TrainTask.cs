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
}

