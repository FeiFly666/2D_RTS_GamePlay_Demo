using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITopBar : MonoBehaviour
{
    [SerializeField] private Text WoodNum;
    [SerializeField] private Text GoldNum;
    [SerializeField] private Text CurrentPeople;
    [SerializeField] private Text TotalPeople;
    
    public void UpdatUI()
    {
        FactionData faction = GameManager.Instance.factions[(int)GameManager.Instance.playerSide];

        WoodNum.text = faction.WoodNum.ToString();

        GoldNum.text = faction.GoldNum.ToString();

        CurrentPeople.text = faction.currentPeopleNum.ToString();

        TotalPeople.text = faction.TotalPeopleNum.ToString();
    }

}