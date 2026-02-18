using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public Sprite Icon;
    public string ID = System.Guid.NewGuid().ToString();

    public abstract void ExecuteAction(UnitSide unitSide);
}
