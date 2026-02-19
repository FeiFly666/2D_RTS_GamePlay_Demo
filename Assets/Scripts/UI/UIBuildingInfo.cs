using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingInfo : MonoBehaviour
{
    [SerializeField]public Text titleName;
    [SerializeField]public Text goldCost;
    [SerializeField]public Text woodCost;
    [SerializeField]public Text finishValue;
    [SerializeField]public Text buildingDescription;

    public void ShowBuildingInfo(UIDescriptionBaseData data)
    {
        this.titleName.text = data.Name;
        this.goldCost.text = data.goldCost.ToString();
        this.woodCost.text = data.woodCost.ToString();
        this.finishValue.text = data.finishValue.ToString();
        this.buildingDescription.text = data.Desciption;
        ShowUI();
    }
    public void CloseBuildingInfo()
    {
        CloseUI();
    }
    private void ShowUI()
    {
        this.gameObject.SetActive(true);
    }
    private void CloseUI()
    {
        this.gameObject.SetActive(false);
    }
}
