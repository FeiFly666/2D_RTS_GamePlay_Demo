using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIProcessItem : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    private TrainingBuilding building;
    private int queueIdx;
    
    public void GetIdx(TrainingBuilding building, int queueIdx)
    {
        this.building = building;
        this.queueIdx = queueIdx;
    }
    public void ResetIdx()
    {
        this.building = null;
        this.queueIdx = -1;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CancelProduction();
        }
    }

    private void CancelProduction()
    {   
        building?.CancleTrainingTask(queueIdx);
    }
}