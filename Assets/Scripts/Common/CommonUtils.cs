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
    public static void DisableMouseEvents(Camera cam)
    {
        cam.eventMask = 0;
    }

    public static Vector3 ClosestPoint(Vector3 requesterPos, Bounds bounds)
    {
        Vector3 searchOrigin;

        float closestX = Mathf.Clamp(requesterPos.x, bounds.min.x, bounds.max.x);
        float closestY = Mathf.Clamp(requesterPos.y, bounds.min.y, bounds.max.y);

        Vector3 edgePoint = new Vector3(closestX, closestY, 0);

        Vector3 dir = (requesterPos - edgePoint).normalized;

        if (dir == Vector3.zero)
        {
            dir = (requesterPos - bounds.center).normalized;
            if (dir == Vector3.zero)
                dir = Vector3.up;
        }

        float pushOffset = TilemapManager.Instance.GetNodeSize().x * 0.6f;
        searchOrigin = edgePoint + dir * pushOffset;

        return searchOrigin;

    }
}
