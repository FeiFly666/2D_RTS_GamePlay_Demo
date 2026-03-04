using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIChangePanel : MonoBehaviour
{

    [SerializeField] Button fogButton;

    public void ChangeButtonColor()
    {
        fogButton.GetComponent<Image>().color = !LevelOption.Instance.isNeedFog ? Color.red : Color.green;
    }


}