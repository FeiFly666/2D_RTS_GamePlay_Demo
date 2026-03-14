using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartPanel : MonoBehaviour
{
    [SerializeField] private UIChangePanel changePanel;

    public void StartLevel()
    {
        StartCoroutine(LoadScene());
    }

    public void Leave()
    {
        Application.Quit();
    }

    public void ShowChangePanel()
    {
        changePanel.gameObject.SetActive(!changePanel.gameObject.activeSelf);
        changePanel.ChangeButtonColor();
        changePanel.ChangeEnemyNum(LevelOption.Instance.enemyNum);
        changePanel.ChangeEnemeyMode();
    }
    public void ChangeEnemyNum(int num)
    {
        LevelOption.Instance.enemyNum = num;
        changePanel.ChangeEnemyNum(num);
    }
    public void ChangeEnenmyMode(int num)
    {
        LevelOption.Instance.enemyMode = (EnemyMode)num;
        changePanel.ChangeEnemeyMode();
    }

    public void ChangeNeedFog()
    {
        LevelOption.Instance.isNeedFog = !LevelOption.Instance.isNeedFog;
    }


    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("SampleScene");
        while (!op.isDone)
        {
            yield return null;
        }
    }
}
