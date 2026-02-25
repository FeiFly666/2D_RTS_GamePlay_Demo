using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class TrainingBuilding : BuildingUnit
{
    private LineRenderer line => GetComponent<LineRenderer>();

    [Header("生产配置")]
    public int maxTrainingNum = 9;

    public List<TrainTask> trainingQueue = new List<TrainTask>();

    public Vector2 spawnPosition;

    public Vector2 gatherPosition = new Vector2(-1500,-1500);

    public int queuePopWeight;//给AI用防止AI一直训练兵种



    protected override void Start()
    {
        base.Start();
        faction = GameManager.Instance.factions[(int)unitSide];

        line.positionCount = 2;
        line.startColor = line.endColor = Color.black;
        line.startWidth = line.endWidth = 0.1f;
        line.enabled = false;

        OnSelected += ShowGatherPosition;

        OnDeselected += StopShowGatherPosition;
    }

    protected override void Update()
    {
        base.Update();
        if(this.buildingState == BuildingState.ConstructionFinished)
        {
            UpdateTraining();
        }
        
    }
    private void UpdateTraining()
    {
        if (trainingQueue.Count == 0) return;
        
        TrainTask currentTask = trainingQueue[0];
        currentTask.remainTime -= Time.deltaTime;

        if(currentTask.remainTime <= 0)
        {
            //当前人口满了停止训练
            if (!faction.HasPeopleSpace(0)) return;
            CompleteTraining(currentTask);
        }
    }
    private void CompleteTraining(TrainTask task)
    {
        //faction.currentPeopleNum += task.humanData.popWeight;
        //faction.AddPopWeight(task.humanData.popWeight);

        // GameObject go = Instantiate(task.humanData.humanPrefab, this.transform.position + (Vector3)spawnPosition, Quaternion.identity);

        HumanUnit human = UnitFactory.CreateHuman(task.humanData, this.transform.position + (Vector3)spawnPosition);

        human.unitSide = unitSide;

        if(gatherPosition.x != -1500 || gatherPosition.y != -1500)
        {
            human.checkTimer = -100f;
            human.TransitionTo(UnitStateType.Move);
            human.MoveToDestinationFrame(gatherPosition);
        }

        queuePopWeight -= task.humanData.popWeight;

        trainingQueue.RemoveAt(0);

    }
    public float GetCurrentTrainingTaskProcess()
    {
        if(trainingQueue.Count == 0) return 0;
        TrainTask task = trainingQueue[0];

        return task.remainTime / task.totalTime;
    }
    public void AddTrainingTask(HumanAction humanData)
    {
        if (trainingQueue.Count >= maxTrainingNum) return;
        if (!faction.CanAfford(humanData.goldCost, humanData.woodCost)) return;
        if (!faction.HasPeopleSpace(0)) return;

        faction.TrySpendResource(humanData.goldCost, humanData.woodCost);
        
        queuePopWeight += humanData.popWeight;

        trainingQueue.Add(new TrainTask(humanData));

    }

    public void CancleTrainingTask(int index)
    {
        if(index < 0 || index >= trainingQueue.Count) return;

        TrainTask cancleTask = trainingQueue[index];
        if(cancleTask == null) return;

        queuePopWeight -= cancleTask.humanData.popWeight;

        HumanAction data = cancleTask.humanData;

        faction.RefundResource(data.goldCost, data.woodCost);
       /* faction.WoodNum += data.woodCost;
        faction.GoldNum += data.goldCost;*/

        trainingQueue.Remove(cancleTask);
    }
    public void RemoveAllTrainingTask()
    {
        trainingQueue.Clear();
    }

    public void ResumeTrainingTask(BuildingSaveData data)
    {
        foreach(var taskData in data.tasks)
        {
            TrainTask resumeTask = new TrainTask(taskData);
            this.trainingQueue.Add(resumeTask);
        }
    }

    public void SetGatherPosition(Vector3 Position)
    {
        if(TilemapManager.Instance.FindNode(Position).AreaID == TilemapManager.Instance.FindNode(this.transform.position + (Vector3)spawnPosition).AreaID)
        {
            this.gatherPosition = Position;
            ShowGatherPosition();
        }
    }

    public void ShowGatherPosition()
    {
        if (gatherPosition.x == -1500 && gatherPosition.y == -1500)
        {
            return;
        }
        line.enabled = true;
        line.SetPosition(0, transform.position + (Vector3)spawnPosition);
        line.SetPosition(1, gatherPosition);
    }
    public void StopShowGatherPosition()
    {
        line.enabled = false;
    }

    public override void Death()
    {
        base.Death();

        OnSelected -= ShowGatherPosition;
        OnDeselected -= StopShowGatherPosition;
    }
}
