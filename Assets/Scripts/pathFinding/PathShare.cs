using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class TargethPathRecord
{
    public int targetID;
    //public int areaID;

    public List<Node> path;

    public float timeRecord;
    public Vector3 sourcePosition;
}

public class PathShare
{
    private Dictionary<int,TargethPathRecord> records = new Dictionary<int, TargethPathRecord> ();

    public void BoardcastPath(Unit target, List<Node> path)
    {
        if(target == null || path == null || path.Count < 5) return;

        int id = target.uniqueID;
        if (!records.ContainsKey(id))
        {
            records[id] = new TargethPathRecord();
            records[id].targetID = id;
        }

        records[id].path = path;
        records[id].timeRecord = Time.time;
        records[id].sourcePosition = path[0].GetNodePosition();

    }

    public List<Node> TryBorrowPath(Unit requester, Unit target)
    {
        int targetID = target.uniqueID;
        if (!records.ContainsKey(targetID)) return null;

        TargethPathRecord record = records[targetID];

        if (Time.time - record.timeRecord > 0.15f) return null;

        if ((requester.transform.position - record.sourcePosition).sqrMagnitude < 4f)
        {
            if (TilemapManager.Instance.CheckBlockBetween2Nodes(requester.transform.position, record.sourcePosition))
            {
                return record.path;
            }
            
        }

        return null;
    }
}
