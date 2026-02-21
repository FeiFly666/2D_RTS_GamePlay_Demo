using Assets.Scripts.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour, Assets.Scripts.ObjectPool.IPoolable
{
    private Unit owner;

    private Vector3 offsetVector;

    [SerializeField]private GameObject fill;
    [SerializeField] private GameObject bg;
    private SpriteRenderer fillSr;
    public SpriteRenderer barSr;
    private SpriteRenderer bgSr;

    private float hideTimer = 0f;
    private float displayDuration = 3f;
    private bool isSelected = false;

    private void Awake()
    {
        fillSr = fill.GetComponent<SpriteRenderer>();
        barSr = GetComponent<SpriteRenderer>();
        bgSr = bg.GetComponent<SpriteRenderer>();
    }
    void Start()
    {

    }

    void Update()
    {
        if(isSelected) { return; }

        if(this.gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if(hideTimer <= 0f)
            {
                hideTimer = 0f;
                this.gameObject.SetActive(false);
            }

        }
    }
    private void LateUpdate()
    {
        if (owner == null || owner is not HumanUnit) return;

        Vector3 barPos = owner.transform.position + offsetVector;

        barPos.z = barPos.y * 0.1f;

        this.transform.position = barPos;

        Vector3 bgPos = bg.transform.position;

        bgPos.z = this.transform.position.z - 0.001f;
        bg.transform.position = bgPos;

        Vector3 fillPos = fill.transform.position;

        fillPos.z = this.transform.position.z - 0.002f;
        fill.transform.position = fillPos;

    }
    public void OnSpawn()
    {

    }
    public void OnDespawn()
    {
        if(this.owner != null)
        {
            owner.stats.OnHealthChanged -= HandleHealthChanged;
            owner.OnSelected -= HandleDeselect;
            owner.OnDeselected -= HandleDeselect;
        }
        this.owner = null;
        this.gameObject.SetActive(false);
    }
    public void RegisterOwner(Unit owner)
    {
        this.owner = owner;

        this.offsetVector = new Vector3(0, owner.hpBarOffset, 0);

        owner.stats.OnHealthChanged += HandleHealthChanged;

        owner.OnSelected += HandleSelect;
        owner.OnDeselected += HandleDeselect;

        this.transform.position = owner.transform.position + this.offsetVector;

        ChangeUI();
    }

    private void HandleSelect()
    {
        isSelected = true;
        this.gameObject.SetActive(true);
    }
    private void HandleDeselect()
    {
        isSelected =false;
        hideTimer = 0f;
        this.gameObject.SetActive(false);
    }
    private void HandleHealthChanged()
    {
        this.gameObject.SetActive(true) ;
        hideTimer = displayDuration;
        ChangeUI();

    }
    private void ChangeUI()
    {
        float barValue = (float)owner.stats.currentHP / (float)owner.stats.FullHP;

        fill.transform.localScale = new Vector3(barValue, 1, 1);
        if (barValue >= 0.5f) fillSr.color = Color.green;
        if (0.2f <= barValue && barValue < 0.5f) fillSr.color = Color.yellow;
        else if (barValue < 0.2f) fillSr.color = Color.red;
    }
    public void OnUnitDestroy()
    {
        string poolKey = string.Empty;
        switch(owner.hpbarType)
        {
            case HpBarType.Unit:
                poolKey = "UnitHpBar";
                break;
            case HpBarType.SmallBuilding:
                poolKey = "SmallBuildingHpBar";
                break;
            case HpBarType.LargeBuilding:
                poolKey = "LargeBuildingHpBar";
                break;
        }

        PoolManager.Instance.Despawn<UIHealthBar>(poolKey,this);
    }
}
