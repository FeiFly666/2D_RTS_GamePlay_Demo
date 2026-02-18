using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    private Slider slider => GetComponentInChildren<Slider>();
    private Unit owner => GetComponentInParent<Unit>();

    [SerializeField] private RectTransform myTransform;
    [SerializeField] private GameObject HpBarGameObject;
    private Image fill;

    private float hideTimer = 0f;
    private float displayDuration = 3f;
    private bool isSelected = false;
    void Start()
    {
        owner.OnFlipped += Flip;

        owner.stats.OnHealthChanged += HandleHealthChanged;

        owner.OnSelected += HandleSelect;
        owner.OnDeselected += HandleDeselect;

        fill = slider.fillRect.GetComponent<Image>();

        fill.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        if(isSelected) { return; }

        if(HpBarGameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if(hideTimer <= 0f)
            {
                hideTimer = 0f;
                HpBarGameObject.SetActive(false);
            }

        }
    }
    private void HandleSelect()
    {
        isSelected = true;
        HpBarGameObject.SetActive(true);
    }
    private void HandleDeselect()
    {
        isSelected =false;
        hideTimer = 0f;
        HpBarGameObject.SetActive(false);
    }
    private void Flip()
    {
        myTransform.Rotate(0, 180, 0);
    }
    private void HandleHealthChanged()
    {
        HpBarGameObject.SetActive(true) ;
        hideTimer = displayDuration;
        ChangeUI();

    }
    private void ChangeUI()
    {
        float barValue = (float)owner.stats.currentHP / (float)owner.stats.FullHP;
        slider.value = barValue;
        if (barValue >= 0.5f) fill.color = Color.green;
        if (0.2f <= barValue && barValue < 0.5f) fill.color = Color.yellow;
        else if (barValue < 0.2f) fill.color = Color.red;
    }
    public void OnUnitDestroy()
    {
        owner.OnFlipped -= Flip;
        owner.stats.OnHealthChanged -= HandleHealthChanged;
        owner.OnSelected -= HandleDeselect;
        owner.OnDeselected -= HandleDeselect;
    }
}
