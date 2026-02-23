using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] public ActionBar actionBar;
    [SerializeField] private Text inputState;
    [SerializeField] private UIBuildingInfo buildingInfo;
    [SerializeField] private UITopBar TopBar;

    private void Start()
    {

    }
    public void RegisterFactionDataDisplay()
    {
        GameManager.Instance.factions[(int)GameManager.Instance.playerSide].OnDataUpdate += UpdatePlayerFactionData;
    }
    public void LogOutFactionDataDisplay()
    {
        GameManager.Instance.factions[(int)GameManager.Instance.playerSide].OnDataUpdate -= UpdatePlayerFactionData;
    }
    public void SetCurrentBuildingInfo(UIDescriptionBaseData buildingData)
    {
        if(buildingData.isNone)
        {
            buildingInfo.CloseBuildingInfo();
        }
        else
        {
            buildingInfo.ShowBuildingInfo(buildingData);
        }
    }


    public void ShowInputSystemState(InputState state)
    {
        inputState.text = "输入系统当前状态：";
        string stateText = "";
        switch (state)
        {
            case InputState.None: stateText = "默认"; break;
            case InputState.Human: stateText = "兵种"; break;
            case InputState.Building: stateText = "建筑"; break;
            case InputState.Placing: stateText = "放置"; break;

        }
        inputState.text += stateText;
    }

    private void UpdatePlayerFactionData()
    {
        TopBar.UpdatUI();
        if(actionBar.gameObject.activeSelf)
        {
            actionBar.ShowActionBarForUnit(actionBar.currentUnit);
        }
    }

}
