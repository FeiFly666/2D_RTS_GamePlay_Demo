using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    private Image img => GetComponent<Image>();
    [SerializeField] private GameObject actionButtonPPrefab;

    [SerializeField]private List<ActionButton> actionButtons = new List<ActionButton>();
    [Header("ÑµÁ·Ãæ°å")]
    [SerializeField] private UITrainningProcess trainingProcess;

    private void Start()
    {
        CloseActionBar();
    }
    public void ShowActionBar()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseActionBar()
    {
        this.gameObject.SetActive(false);
        ClearAllActionButtons();
    }

    public void UpdateTrainingProcess()
    {
        this.trainingProcess.UpdateUI();
    }

    public void RegisterActionButton(UIDescriptionBaseData buildingData, Sprite icon, UnityAction action)
    {
        GameObject newButton = Instantiate(actionButtonPPrefab,this.transform);

        ActionButton button = newButton.GetComponent<ActionButton>();

        actionButtons.Add(button);

        button.InitButton(buildingData, icon, action);
    }
    public void RegisterActionButton(Sprite icon, UnityAction action)
    {
        GameObject newButton = Instantiate(actionButtonPPrefab, this.transform);

        ActionButton button = newButton.GetComponent<ActionButton>();

        actionButtons.Add(button);

        button.InitButton(icon, action);
    }

    public void ShowActionBarForUnit(Unit unit)
    {
        if(unit.Actions.Count > 0)
        {
            foreach(var action in  unit.Actions)
            {
                if (action is BuildingAction buildAction)
                {
                    RegisterActionButton(buildAction.GetBuildingBaseData(), action.Icon, () => action.ExecuteAction(unit.unitSide));
                }
                else if(action is HumanAction humanAction)
                {
                    RegisterActionButton(humanAction.GetHumanBaseData(),action.Icon, () => action.ExecuteAction(unit));
                    if(unit is TrainingBuilding trainingBuilding)
                    {
                        trainingProcess.Show(trainingBuilding);
                    }
                }
            }
            ShowActionBar();
        }
    }

    private void ClearAllActionButtons()
    {
        if(actionButtons.Count > 0)
        {
            for(int i = actionButtons.Count - 1; i >= 0; i--)
            {
                Destroy(actionButtons[i].gameObject);
            }
        }
        actionButtons.Clear();

        trainingProcess.Hide();
    }
}
