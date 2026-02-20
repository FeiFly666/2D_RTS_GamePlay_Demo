using Assets.Scripts;
using Assets.Scripts.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class ActionBar : MonoBehaviour
{
    private Image img => GetComponent<Image>();
    [SerializeField] private GameObject actionButtonPPrefab;

    [SerializeField]private List<ActionButton> actionButtons = new List<ActionButton>();
    [Header("训练面板")]
    [SerializeField] private UITrainningProcess trainingProcess;
    private ObjectPool<ActionButton> actionPool;

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
        ClearAllActionButtons();
    }

    public void UpdateTrainingProcess()
    {
        this.trainingProcess.UpdateUI();
    }

    public void RegisterActionButton(UIDescriptionBaseData buildingData, Sprite icon, UnityAction action)
    {
       // GameObject newButton = Instantiate(actionButtonPPrefab,this.transform);

        ActionButton button = PoolManager.Instance.Spawn<ActionButton>("ActionButton");

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
        ClearAllActionButtons();
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
                    RegisterActionButton(buildingAction.GetBuildingBaseData(), currentAction.Icon, ()=> currentAction.ExecuteAction(invoker.unitSide));
                }
                else if (currentAction is HumanAction humanAction)
                {
                    //Debug.Log($"按钮点击：建筑是 {invoker.gameObject.name}, 执行动作是 {humanAction.name}");
                    RegisterActionButton(humanAction.GetHumanBaseData(), currentAction.Icon, () => currentAction.ExecuteAction(invoker));
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

                /*targetGo.SetActive(false);

                targetGo.transform.SetParent(null);

                Destroy(targetGo);*/
            }
        }
        actionButtons.Clear();

        trainingProcess.Hide();
    }
}
