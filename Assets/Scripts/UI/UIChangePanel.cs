using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIChangePanel : MonoBehaviour
{

    [SerializeField] Button fogButton;
    [SerializeField] Text enemyNum;
    [SerializeField] Button[] enemyModeButtons;

    public void ChangeButtonColor()
    {
        fogButton.GetComponent<Image>().color = !LevelOption.Instance.isNeedFog ? Color.red : Color.green;
    }
    public void ChangeEnemyNum(int num)
    {
        enemyNum.text = num.ToString();
    }
    public void ChangeEnemeyMode()
    {
        for(int i = 0;i<enemyModeButtons.Length;i++)
        {
            enemyModeButtons[i].GetComponent<Image>().color = (int)LevelOption.Instance.enemyMode == i ? Color.green : Color.red;
        }
    }

}