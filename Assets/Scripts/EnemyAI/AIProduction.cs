using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIProduction
{
    private FactionAI AI;
    public AIProduction(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        HumanAction requested = AI.strategy.requestHuman;
        if (requested == null) return;

        TrainingBuilding producer = FindBestProducerFor(requested);

        if (producer != null)
        {
            if (producer.trainingQueue.Count < producer.maxTrainingNum)
            {
                producer.AddTrainingTask(requested);

                AI.strategy.requestHuman = null;
            }
        }
    }

    private TrainingBuilding FindBestProducerFor(HumanAction data)
    {
        return AI.faction.trainings
                 .Where(t => t.buildingType == BuildingType.Train)
                 .OrderBy(t => t.trainingQueue.Count)
                 .FirstOrDefault();
    }
}
