using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] public ActionBar actionBar;
    [SerializeField] public Text inputState;
    [SerializeField] public UIBuildingInfo buildingInfo;

    public void SetCurrentBuildingInfo(UIDescriptionBaseData buildingData)
    {
        if(buildingData == null)
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
            case InputState.Unit: stateText = "兵种"; break;
            case InputState.Buliding: stateText = "建造"; break;

        }
        inputState.text += stateText;
    }

}
