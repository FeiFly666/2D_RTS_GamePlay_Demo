using Assets.Scripts.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour,IPoolable
{
    [SerializeField ]private Button button;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private UIDescriptionBaseData unitData;

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }
    public void OnDespawn()
    {
        button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
    public void InitButton(UIDescriptionBaseData unitData, Sprite icon, UnityAction action, bool interactable = true)
    {
        this.unitData = unitData;

        button.onClick.AddListener(action);

        buttonIcon.overrideSprite = icon;

        button.interactable = interactable;
    }

    public void ShowButtonInfo()
    {
        if(!this.unitData.isNone)
        {
            UIManager.Instance.SetCurrentBuildingInfo(this.unitData);
        }
        else
        {

        }
    }
    public void CloseButtonInfo()
    {
        if (!this.unitData.isNone)
        {
            UIManager.Instance.SetCurrentBuildingInfo(UIDescriptionBaseData.Empty());

        }
        else
        {

        }
    }
}
