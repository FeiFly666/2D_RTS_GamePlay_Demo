using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceAction", menuName = "Action/ResourceAction")]

public class ResourceAction : Action
{
    [SerializeField] private GameObject ResourcePrefab;
    [SerializeField] private int MaxResourceNum;

    public GameObject resourcePrefab => ResourcePrefab;
    public int maxResourceNum => MaxResourceNum;
    public override void ExecuteAction(UnitSide unitSide)
    {

        
    }
}
