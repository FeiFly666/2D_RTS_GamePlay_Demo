using Assets.Scripts;
using Assets.Scripts.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    [SerializeField] private GameObject actionButtonPPrefab;

    [SerializeField]private List<ActionButton> actionButtons = new List<ActionButton>();
    [SerializeField] private Transform buttonRoot;
    [Header("ÑµÁ·Ãæ°å")]
    [SerializeField] private UITrainningProcess trainingProcess;

    public Unit currentUnit;

    private void Start()
    {
        CloseActionBar();
        PoolManager.Instance.CreatePool("ActionButton", actionButtonPPrefab.GetComponent<ActionButton>() ,10,this.transform);
    }
    public void ShowActionBar()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseActionBar()
    {
        this.gameObject.SetActive(false);

        if(currentUnit != null)
        {
            currentUnit.OnUnitDead -= CloseActionBar;
            currentUnit = null;
        }

        ClearAllActionButtons();
    }

    public void UpdateTrainingProcess()
    {
        this.trainingProcess.UpdateUI();
    }

    public void RegisterActionButton(UIDescriptionBaseData buildingData, Sprite icon, UnityAction action, bool interactable = false)
    {
        ActionButton button = PoolManager.Instance.Spawn<ActionButton>("ActionButton");

        actionButtons.Add(button);

        button.InitButton(buildingData, icon, action, interactable);

        button.transform.SetParent(buttonRoot);

    }

    private void ChangeCurrentUnit(Unit newUnit)
    {
        if (currentUnit != null) 
        {
            currentUnit.OnUnitDead -= CloseActionBar;
        }
        currentUnit = newUnit;

        if(currentUnit != null)
        {
            newUnit.OnUnitDead += CloseActionBar;
        }
    }
    public void ShowActionBarForUnit(Unit unit)
    {
        ClearAllActionButtons();

        ChangeCurrentUnit(unit);

        FactionData faction = GameManager.Instance.factions[(int)unit.unitSide];

        if(unit.Actions.Count > 0)
        {
            Unit invoker = unit;

            if (invoker is TrainingBuilding trainingBuilding)
            {
                trainingProcess.Show(trainingBuilding);
            }

            for (int i = 0; i < unit.Actions.Count; i++)
            {
                var currentAction = unit.Actions[i];

                if(currentAction is BuildingAction buildingAction)
                {
                    if (faction.CanGenerate(buildingAction.conditions))
                        RegisterActionButton(buildingAction.GetBuildingBaseData(), currentAction.Icon, ()=> currentAction.ExecuteAction(invoker.unitSide),faction.CanAfford(buildingAction.goldCost,buildingAction.woodCost));
                }
                else if (currentAction is HumanAction humanAction)
                {
                    if (faction.CanGenerate(humanAction.conditions))
                        RegisterActionButton(humanAction.GetHumanBaseData(), currentAction.Icon, () => currentAction.ExecuteAction(invoker), faction.CanAfford(humanAction.goldCost, humanAction.woodCost));
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
                ActionButton target = actionButtons[i];

                PoolManager.Instance.Despawn("ActionButton",target);
            }
        }
        actionButtons.Clear();

        trainingProcess.Hide();

    }
}
