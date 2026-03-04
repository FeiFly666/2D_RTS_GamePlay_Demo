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
