using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITrainningProcess : MonoBehaviour
{

    [SerializeField] private UIProcessItem currentSlot;
    [SerializeField] private Image currentProcess;
    [SerializeField] private UIProcessItem[] queueSlots;

    private TrainingBuilding trainingBuilding;

    public void Show(TrainingBuilding trainingBuilding)
    {
        this.trainingBuilding = trainingBuilding;
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        this.trainingBuilding = null;
    }
    private void Update()
    {
        if (trainingBuilding == null) return;
        UpdateUI();
    }
    public void UpdateUI()
    {
        var queue = trainingBuilding.trainingQueue;
        if(queue.Count > 0)
        {
            currentSlot.icon.gameObject.SetActive (true);
            currentSlot.icon.sprite = queue[0].humanData.Icon;

            currentSlot.GetIdx(trainingBuilding, 0);

            float percent = trainingBuilding.GetCurrentTrainingTaskProcess();
            currentProcess.fillAmount = 1 - percent;
        }
        else
        {
            currentSlot.ResetIdx();
            currentSlot.icon.gameObject.SetActive(false);
            currentProcess.fillAmount = 0;
        }

        for(int i = 0; i < queueSlots.Length; i++)
        {
            int queueIdx = i + 1;
            if(queueIdx < queue.Count)
            {
                queueSlots[i].GetIdx(trainingBuilding, queueIdx);
                queueSlots[i].icon.gameObject.SetActive(true);
                queueSlots[i].icon.sprite = queue[queueIdx].humanData.Icon;
            }
            else
            {
                queueSlots[i].ResetIdx();
                queueSlots[i].icon.gameObject.SetActive(false);
            }
        }
        
    }
}