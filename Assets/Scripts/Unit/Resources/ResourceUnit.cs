using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class ResourceUnit : Unit
{
    public Animator anim;

    public int resourceAreaID;

    public int fullResourceNum;

    private int ResourceLeftNum;

    public int resourceLeftNum
    {
        set 
        {
            if (value < 0) value = 0;
            ResourceLeftNum = value;
            OnResourcesNumChanged();
        }
        get
        {
            return ResourceLeftNum;
        }
    }

    private SpriteRenderer sr => GetComponent<SpriteRenderer>();
    [SerializeField] private int maxWorkerNum = 2;//暂时不可变
    private Node standNode;
    private List<Node> availableSlots;
    private Dictionary<HumanUnit, Node> workerToSlot = new Dictionary<HumanUnit, Node>();

    public ResourceAction data;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        InitData();
    }
    protected override void Start()
    {
        base.Start();
        if(!GameManager.Instance.resources.Contains(this))
        {
            GameManager.Instance.resources.Add(this);
        }
        StartCoroutine(NotifyingResourceChanged());
        InitSlot();
        
    }
    private void InitData()
    {
        this.fullResourceNum = data.maxResourceNum;
        this.ResourceLeftNum = fullResourceNum;
    }
    IEnumerator NotifyingResourceChanged()
    {
        if (TilemapManager.Instance.isLoading)
        {
            yield break;
        }
        yield return new WaitForSeconds(0.02f);
        TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);
    }
    private void InitSlot()
    {
        standNode = TilemapManager.Instance.FindNode(this.transform.position);

        availableSlots = new List<Node>();

        List<Node> neighbors = TilemapManager.Instance.GetAllWalkableNeighbors(standNode);

        //坑位1注册为第一个可走neighbor
        availableSlots.Add(neighbors[0]);
        neighbors[0].occupant = this.gameObject;
        //查找不与第一个坑位过近的neighbor并注册为坑位2
        foreach (Node node in neighbors)
        {
            if (Mathf.Abs( node.GridX - availableSlots[0].GridX) + Mathf.Abs( node.GridY - availableSlots[0].GridY) < 2) continue;
            availableSlots.Add(node);
            node.occupant = this.gameObject;
            break;
        }
        //实在找不到离得远的就注册近处的吧
        if (availableSlots.Count < 2)
        {
            availableSlots.Add(neighbors[1]);
            neighbors[1].occupant = this.gameObject;
        }
    }
    public bool CanAddWorker => workerToSlot.Count < maxWorkerNum;
    public Node RequestSlot(HumanUnit unit)
    {
        if (workerToSlot.ContainsKey(unit)) return workerToSlot[unit];
        if (workerToSlot.Count >= availableSlots.Count) return null;

        foreach (Node slotNode in availableSlots)
        {
            if (!workerToSlot.ContainsValue(slotNode))
            {
                workerToSlot.Add(unit, slotNode);
                slotNode.occupant = unit.gameObject;
                return slotNode;
            }
        }
        return null;
    }
    public void WorkderReleaseSlot(HumanUnit unit)
    {
        if (workerToSlot.ContainsKey(unit))
        {
            workerToSlot[unit].occupant = this.gameObject;
            (unit as Worker).mySlot = null;
            workerToSlot.Remove(unit);
        }
    }
    private void ReleaseSlot()
    {
        foreach(var slot in availableSlots)
        {
            slot.occupant = null;
        }
    }
    private void OnResourcesNumChanged()
    {
        if (resourceLeftNum <= 0)
        {
            Death();
        }
    }
    public override void Death()
    {
        List<HumanUnit> worker = new List<HumanUnit>(); 
        foreach(var human in workerToSlot)
        {
            worker.Add(human.Key);
        }
        foreach(var unit in worker)
        {
            WorkderReleaseSlot(unit);
        }

        base.Death();
        if(GameManager.Instance.resources.Contains(this))
        {
            GameManager.Instance.resources.Remove(this);
        }
        ReleaseSlot();
        anim.SetTrigger("Death");
    }
    
    public void ResourceChopper()
    {
        TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);
        DestroyUnit();
    }
    public ResourceSaveData ToSaveData()
    {
        return new ResourceSaveData(this);
    }
    public void LoadData(ResourceSaveData data)
    {
        this.uniqueID = data.ID;
        this.resourceAreaID = data.ResourceAreaID;
        this.resourceLeftNum = data.leftResourceNum;

        //TilemapManager.Instance.NotifyingBuildingChanged(this.GetComponent<BoxCollider2D>().bounds);
    }
}
