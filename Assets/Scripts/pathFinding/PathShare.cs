using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

    public List<Node> TryBorrowPath(Unit requester, Unit target, out int index)
    {
        int targetID = target.uniqueID;
        index = 0;
        if (!records.ContainsKey(targetID)) return null;

        TargethPathRecord record = records[targetID];

        if (Time.time - record.timeRecord > 0.1f) return null;

        if ((requester.transform.position - record.sourcePosition).sqrMagnitude < 4f)
        {
            if (TilemapManager.Instance.CheckBlockBetween2Nodes(requester.transform.position, record.sourcePosition))
            {
                index = GetAnIndex(requester, record.path);
                return record.path;
            }
            
        }

        return null;
    }
    public int GetAnIndex(Unit member, List<Node>sharedPath)
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 1; i < Mathf.Min(sharedPath.Count, 20); i++)
        {
            if (!TilemapManager.Instance.CheckBlockBetween2Nodes(member.transform.position, sharedPath[i].GetNodePosition()))
            {
                continue;
            }
            float dist = Vector2.Distance(member.transform.position, sharedPath[i].GetNodePosition());
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}
