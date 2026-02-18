using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProcess
{
    public float process {  get; private set; }

    private float MaxValue;

    public bool IsComplete => process >= MaxValue;

    private HashSet<Worker> workers = new HashSet<Worker>();

    public System.Action Complete;


    public BuildingProcess(float maxValue, System.Action OnComplete)
    {
        this.MaxValue = maxValue;
        this.Complete = OnComplete;
    }
    public BuildingProcess(float currentProcess, float maxValue, System.Action OnComplete)//读档用的
    {
        this.MaxValue = maxValue;

        this.process = currentProcess;

        this.Complete = OnComplete;
    }
    public void AddWorker(Worker unit)
    {
       if( workers.Add(unit))
       {
            //TODO:工人单位进入建造状态效果
       }
    }
    public void RemoveWorker(Worker unit)
    {
        workers.Remove(unit);
        //TODO:工人单位退出建造状态效果
    }

    public void RemoveAllWorkers()
    {
        foreach(Worker worker in workers)
        {
            //TODO:工人单位退出建造状态效果
        }
        workers.Clear();
    }

    public void UpdateProcess(float delatTime)
    {
        if(workers.Count == 0) return;

        process += workers.Count * delatTime;

        if(process >= MaxValue)
        {
            Complete?.Invoke();
        }
    }
}
