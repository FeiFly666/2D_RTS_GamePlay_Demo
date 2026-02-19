using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    private Button button => GetComponent<Button>();
    [SerializeField] private Image buttonIcon;
    [SerializeField] private UIDescriptionBaseData unitData;
    public void InitButton(UIDescriptionBaseData unitData, Sprite icon, UnityAction action)
    {
        this.unitData = unitData;

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
        if(this.unitData != null)
        {
            UIManager.Instance.SetCurrentBuildingInfo(this.unitData);
        }
        else
        {

        }
    }
    public void CloseButtonInfo()
    {
        if (this.unitData != null)
        {
            UIManager.Instance.SetCurrentBuildingInfo(null);

        }
        else
        {

        }
    }
}
