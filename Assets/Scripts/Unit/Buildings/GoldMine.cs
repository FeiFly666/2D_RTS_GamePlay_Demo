using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GoldMine : BuildingUnit
{
    public List<InsideGoldMinUnit> humanInsideData = new List<InsideGoldMinUnit>();

    [SerializeField] private Sprite hasHumanSprite;

    [SerializeField] private int CollectNum = 5;

    [SerializeField] private float CollectCD = 5;

    [SerializeField] private int maxUnitNum = 3;

    public Vector2 spawnPosition;
    private float collectTimer;

    public bool isEmpty => humanInsideData.Count == 0;

    public bool CanInside => humanInsideData.Count < maxUnitNum;

    protected override void Start()
    {
        base.Start();
        faction = GameManager.Instance.factions[(int)this.unitSide];

    }

    protected override void UpdateBehaviour()
    {
        if(isEmpty || isDead) return;

        float currentCD = CollectCD;
        int currentNum = CollectNum;

        foreach(var unit in humanInsideData)
        {
            if(unit.isWorker)
            {
                currentCD -= 0.5f;
            }
            else
            {
                currentNum++;
            }
        }

        if(Time.time - collectTimer >= currentCD)
        {
            collectTimer = Time.time;

            faction.AddGold(currentNum);
            
        }
    }

    public bool InsideBuilding(HumanUnit unit)
    {

        if (!CanInside) return false;

        InsideGoldMinUnit data = new InsideGoldMinUnit(unit);
        humanInsideData.Add(data);

        if(humanInsideData.Count == 1)
        {
            collectTimer = Time.time;
            sr.sprite = hasHumanSprite;
        }

        return true;
    }

    public void ReleaseAllUnits()
    {
        foreach(var data in humanInsideData)
        {
            HumanAction humanData = SaveManager.Instance.dataCatalog.GetHumanByID(data.data);

            //GameObject go = Instantiate(humanData.humanPrefab, this.transform.position + (Vector3)spawnPosition, Quaternion.identity);

            HumanUnit human = UnitFactory.CreateHuman(humanData, this.transform.position + (Vector3)spawnPosition);

            human.uniqueID = data.UnitID;

            human.unitSide = this.unitSide;

            human.stats.currentHP = data.humanCurrentHP;
        }
        humanInsideData.Clear();

        sr.sprite = data.completeSprite;
    }

    public void ResumeInsideUnits(List<InsideGoldMinUnit> data)
    {
        this.humanInsideData = data.ToList();
        if(humanInsideData.Count > 0)
        {
            sr.sprite = hasHumanSprite;
        }
    }

}