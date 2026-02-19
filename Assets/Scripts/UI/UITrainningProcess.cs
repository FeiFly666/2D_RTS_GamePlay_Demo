using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITrainningProcess : MonoBehaviour
{

    [SerializeField] private Image currentIcon;
    [SerializeField] private Image currentProcess;
    [SerializeField] private Image[] queueSlots;

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
            currentIcon.gameObject.SetActive (true);
            currentIcon.sprite = queue[0].humanData.Icon;

            float percent = trainingBuilding.GetCurrentTrainingTaskProcess();
            currentProcess.fillAmount = 1 - percent;
        }
        else
        {
            currentIcon.gameObject.SetActive(false);
            currentProcess.fillAmount = 0;
        }

        for(int i = 0; i < queueSlots.Length; i++)
        {
            int queueIdx = i + 1;
            if(queueIdx < queue.Count)
            {
                queueSlots[i].gameObject.SetActive(true);
                queueSlots[i].sprite = queue[i].humanData.Icon;
            }
            else
            {
                queueSlots[i].gameObject.SetActive(false);
            }
        }
    }
}