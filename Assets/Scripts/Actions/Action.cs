using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Action : ScriptableObject
{
    public Sprite Icon;
    public string ID = System.Guid.NewGuid().ToString();

    public abstract void ExecuteAction(UnitSide unitSide);

    public virtual void ExecuteAction(Unit invoker)
    {
        ExecuteAction(invoker.unitSide);
    }
}
