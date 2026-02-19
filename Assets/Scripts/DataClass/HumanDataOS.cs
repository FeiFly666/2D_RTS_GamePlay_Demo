using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HumanData", menuName = "DataOS/HumanData")]
public class HumanDataOS : ScriptableObject
{
    [SerializeField] private string HumanName;
    [SerializeField] private string HumanDescription;
    [SerializeField] private int GoldCost;
    [SerializeField] private int WoodCost;
    [SerializeField] private int PopWeight;
    [SerializeField] private float DetectRadius;
    [SerializeField] private float AttackRadius;
    [SerializeField] private float TrainingTime;
    [SerializeField] private int FullHP;
    [SerializeField] private int Attack;
    [SerializeField] private int Armor;
    [SerializeField] private int CritChance;
    [SerializeField] private float CritMutiple;
    [SerializeField] private float MoveSpeed;
    [SerializeField] private bool IsAOE;
    [SerializeField] private int MaxHoldResourceNum;
    [SerializeField] private int ChopNum;

    public string humanName => HumanName;
    public string humanDescription => HumanDescription;
    public int goldCost => GoldCost;
    public int woodCost => WoodCost;
    public int popWeight => PopWeight;
    public float detectRadius => DetectRadius;
    public float attackRadius => AttackRadius;
    public float trainingTime => TrainingTime;
    public int fullHP => FullHP;
    public int attack => Attack;
    public int armor => Armor;
    public int critChance => CritChance;
    public float critMutiple => CritMutiple;
    public float moveSpeed => MoveSpeed;
    public bool isAOE => IsAOE;
    public int maxHoldResourceNum => MaxHoldResourceNum;
    public int gatherNum => ChopNum;
}
