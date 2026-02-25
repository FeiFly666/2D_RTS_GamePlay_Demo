using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEconomy
{
    private FactionAI AI;

    public AIEconomy(FactionAI AI)
    {
        this.AI = AI;
    }
    public void UpdateLogic()
    {
        if (AI.strategy.requestBuilding == null) return;

        BuildingAction requestBuilding = AI.strategy.requestBuilding;

        if (!AI.faction.CanAfford(requestBuilding.goldCost, requestBuilding.woodCost)) return;


    }
}
