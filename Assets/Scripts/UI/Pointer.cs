using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PointerMode
{
    click,
    show
}
public class Pointer : MonoBehaviour
{
    [SerializeField] private float showTime;
    [SerializeField] private AnimationCurve ScaleCurve;

    private SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();

    private Vector3 originScale;

    private float timer = 0f;

    private bool isShow = false;

    private bool flip = false;

    private PointerMode mode;

    private Unit owner;
    void Start()
    {
        originScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(isShow)
        {
            if(mode == PointerMode.click)
            {
                timer += Time.deltaTime;

                float scaleSize = ScaleCurve.Evaluate(timer);

                this.transform.localScale = originScale * scaleSize;

                if (timer >= showTime)
                {
                    ClosePointer();
                }
            }
            else
            {
                if((owner as HumanUnit).GetDestination() != this.transform.position )
                {
                    this.transform.position = (owner as HumanUnit).GetDestination();
                }
                if (flip)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer -= Time.deltaTime;
                }

                float scaleSize = ScaleCurve.Evaluate(timer);

                this.transform.localScale = originScale * scaleSize;

                if (timer > showTime || timer < 0.2f)
                {
                    flip = !flip;
                    if (timer > showTime) timer = showTime;
                    else timer = 0.2f;
                }
            }
            if(owner != null)
            {
                if (Vector2.Distance(owner.transform.position, this.transform.position) < 0.1f)
                {
                    ClosePointer();
                }
            }
        }
    }
    public void SetPointerMode(PointerMode mode)
    {
        this.mode = mode;
    }
    public void SetPointPositionAndShow(Vector2 position,Unit owner)
    {
        this.transform.position = position;
        this.owner = owner;
        ShowPointer();
    }

    private void ShowPointer()
    {
        timer = mode == PointerMode.click ? 0 : 0.2f;
        spriteRenderer.enabled = true;
        isShow = true;
    }

    public void ClosePointer()
    {
        spriteRenderer.enabled = false;
        isShow = false;
    }

}
