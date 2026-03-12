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

    public bool prepareForAttack;

    public bool attack;

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
    }
}
