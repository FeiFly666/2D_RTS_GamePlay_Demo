using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRay : MonoBehaviour
{
    private bool isStart = false;

    private Transform owner;
    private Vector2 targetPos;

    private LineRenderer ray;

    public void RayInit(Transform startPos, Vector2 endPos)
    {
        ray = GetComponent<LineRenderer>();

        ray.sortingLayerName = "Unit";
        ray.sortingOrder = -1;
        ray.material = new Material(Shader.Find("Sprites/Default"));
        ray.startColor = ray.endColor = Color.red;
        ray.startWidth = ray.endWidth = 0.05f;

        SetRayPoint(startPos, endPos);
    }

    public void SetRayPoint(Transform startPos, Vector2 endPos)
    {
        this.owner = startPos;
        this.targetPos = endPos;
        isStart = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isStart)
        {
            ray.positionCount = 2;
            ray.SetPosition(0, owner.transform.position);
            ray.SetPosition(1, targetPos);

            StartCoroutine(RayDisappear());
        }
    }

    private IEnumerator RayDisappear()
    {
        yield return new WaitForSeconds(.8f);
        if(this.gameObject != null)
        {
            Destroy(this.gameObject);
        }
    }
}
