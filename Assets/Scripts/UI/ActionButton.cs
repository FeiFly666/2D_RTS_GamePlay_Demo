using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    private Button button => GetComponent<Button>();
    [SerializeField] private Image buttonIcon;
    [SerializeField]private BuildingBaseData buildingBaseData;
    public void InitButton(BuildingBaseData buildingData, Sprite icon, UnityAction action)
    {
        this.buildingBaseData = buildingData;

        button.onClick.AddListener(action);

        buttonIcon.overrideSprite = icon;
    }
    public void InitButton(Sprite icon, UnityAction action)
    {
        button.onClick.AddListener(action);

        buttonIcon.overrideSprite = icon;
    }
    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void ShowButtonInfo()
    {
        if(this.buildingBaseData != null)
        {
            UIManager.Instance.SetCurrentBuildingInfo(this.buildingBaseData);
        }
        else
        {

        }
    }
    public void CloseButtonInfo()
    {
        if (this.buildingBaseData != null)
        {
            UIManager.Instance.SetCurrentBuildingInfo(null);

        }
        else
        {

        }
    }
}
