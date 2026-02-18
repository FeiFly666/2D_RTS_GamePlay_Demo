using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanAction", menuName = "Action/HumanAction")]

public class HumanAction : Action
{
    [SerializeField] private Sprite HumanSprite;
    [SerializeField] private GameObject HumanPrefab;

    [SerializeField] public HumanDataOS HumanData;


    public Sprite humanSprite => HumanSprite;
    public GameObject humanPrefab => HumanPrefab;
    public string humanName => HumanData.humanName;
    public string humanDescription => HumanData.humanDescription;
    public int goldCost => HumanData.goldCost;
    public int woodCost => HumanData.woodCost;
    public float detectRadius => HumanData.detectRadius;
    public float attackRadius => HumanData.attackRadius;
    public float trainingTime => HumanData.trainingTime;
    public int fullHP => HumanData.fullHP;
    public int attack => HumanData.attack;
    public int armor => HumanData.armor;
    public int critChance => HumanData.critChance;
    public float critMutiple => HumanData.critMutiple;
    public float moveSpeed => HumanData.moveSpeed;
    public bool isAOE => HumanData.isAOE;
    public int maxHoldResourceNum => HumanData.maxHoldResourceNum;
    public int chopNum => HumanData.gatherNum;
    public override void ExecuteAction(UnitSide unitSide)
    {

    }

}

