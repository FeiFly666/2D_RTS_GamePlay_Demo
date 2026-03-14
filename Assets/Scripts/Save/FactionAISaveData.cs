using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class FactionAISaveData
{
    public UnitSide side;

    public int attackTimes;
    public int nextAttackNum;

    public List<int> groupMembers = new List<int>();
    public int groupTargetID;

    public bool prepareForAttack;

    public bool attack;

    public UnitSide targetSide;
    public UnitSide lastTargetSide;

    public FactionAISaveData()
    {

    }
    public FactionAISaveData(FactionAI ai)
    {
        this.side = ai.unitSide;
        this.attackTimes = ai.attackTimes;
        this.nextAttackNum = ai.nextAttackNum;
        this.prepareForAttack = ai.prepareForAttack;
        this.attack = ai.attack;

        foreach(var unit in ai.tacitical.groupMembers)
        {
            groupMembers.Add(unit.uniqueID);
        }

        if(ai.tacitical.attackGroup != null)
        {
            if(ai.tacitical.attackGroup.targetID != -1)
            {
                groupTargetID = ai.tacitical.attackGroup.targetID;
            }
        }

        this.targetSide = ai.targetSide;
        this.lastTargetSide = ai.tacitical.lastTargetSide;
    }
}
