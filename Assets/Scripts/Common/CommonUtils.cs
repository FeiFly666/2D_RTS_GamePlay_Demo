using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommonUtils
{
    public static Vector3 GetWorldMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public static bool IsPointOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    public static bool IsInRange(Vector3 A, Vector3 B, float range)
    {
        return (A - B).sqrMagnitude <= range * range;
    }
}
