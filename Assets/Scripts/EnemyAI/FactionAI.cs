using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionAI : MonoBehaviour
{
    public UnitSide unitSide;
    public List<Action> actionList;
    public FactionData faction;

    public AIStrategy strategy;
    public AIProduction production;
    public AIEconomy economy;
    public AITactical tacitical;
    private void Awake()
    {
        strategy = new AIStrategy(this);
        production = new AIProduction(this);
        economy = new AIEconomy(this);
        tacitical = new AITactical(this);
    }
    void Start()
    {
        faction = GameManager.Instance.factions[(int)unitSide];
        actionList = faction.buildings.Find(b => b.buildingType == BuildingType.Static).Actions;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
