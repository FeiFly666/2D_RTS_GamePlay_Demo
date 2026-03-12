using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngineInternal;

public class GroupSaveData
{
    public int ID;
    public int leaderID;
    public List<int> memberIDs = new List<int>();
    public int targetID;

    public SerializableVector3 targetPosition;

    public GroupSaveData()
    {

    }
    public GroupSaveData(UnitGroup group)
    {
        if (group.leader == null) return;
        this.ID = group.uniqueID;
        this.leaderID = group.leader.uniqueID;
        foreach (var member in group.members)
        {
            memberIDs.Add(member.uniqueID);
        }
        targetID = group.targetID;

        targetPosition = new SerializableVector3(group.movePosition);
    }
}
