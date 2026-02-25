using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum UnitState
{
    Idel,
    Move,
    Attack,
    Work,
    Deliver,
    Dead
}

public enum UnitType
{
    unit,
    building
}
public enum UnitSide
{
    A, B, C, D, E
}

public enum HpBarType
{
    Unit,
    SmallBuilding,
    LargeBuilding
}

public class Unit : MonoBehaviour
{
    public List<Action> Actions = new List<Action>();

    public int uniqueID;


    public UnitType unitType;

    public UnitSide unitSide;

    public Vector3 detectPosition;
    [SerializeField] public float sortingYOffset = 0.2f;
    [SerializeField] public float hpBarOffset;

    public GameObject HpBar;
    public HpBarType hpbarType;

    public UnitStats stats;
    [SerializeField]public UnityEngine.Rendering.SortingGroup sg;
    private int lastFinalOrder;
    [SerializeField ]public SpriteRenderer sr;

    public bool isDead = false;

    public System.Action OnSelected;
    public System.Action OnDeselected;
    public System.Action OnUnitDead;

    protected virtual void Awake()
    {
        sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
        sr = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<UnitStats>();

        uniqueID = GameManager.Instance.GetAnID();

        UpdateSortingGroup();
    }
    protected virtual void Start()
    {

        this.gameObject.layer = LayerMask.NameToLayer("Side" + (int)unitSide);

        if (unitType == UnitType.building)
        {
            this.detectPosition = transform.position;
            this.detectPosition.y += 1;
        }

        GameManager.Instance.RegisterSideUnit(this);

        if(this is not ResourceUnit)
        {
           UIHealthBar hpBar = HPBarManager.Instance.GetAnHpBar(this);
            if (hpBar != null)
            {
                this.HpBar = hpBar.gameObject;
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //UpdateBehaviour();

    }
    protected virtual void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (sg == null || this is HumanUnit human && human.isBuildingUnit) return;
        if (this is BuildingUnit) return;
        UpdateSortingGroup();
        //UpdateZAxisSort();
    }

    private void UpdateSortingGroup()
    {
        float logicalY = transform.position.y + sortingYOffset;


        int yOrder = Mathf.FloorToInt(-logicalY * 20);


        int tieBreaker = Mathf.Abs(gameObject.GetInstanceID()) % 10;

        int finalOrder = (yOrder * 10) + tieBreaker;

        //int finalOrder = -Mathf.FloorToInt(logicalY / 0.2f);

        if (finalOrder != lastFinalOrder)
        {
            sg.sortingOrder = Mathf.Clamp(finalOrder, -32768, 32767);
            lastFinalOrder = finalOrder;
        }

    }
    private void UpdateZAxisSort()
    {
        float logicalY = transform.position.y + sortingYOffset;

        float sortZ = logicalY * 0.01f;

        float tieBreaker = (Mathf.Abs(gameObject.GetInstanceID() % 100)) * 0.0001f;

        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, currentPos.y, sortZ + tieBreaker);
    }

    public virtual void Death()
    {
        isDead = true;
        OnUnitDead?.Invoke();

        if(HpBar != null)
        {
            HpBar.SetActive(false);
            HpBar.GetComponent<UIHealthBar>().OnUnitDestroy();
        }
        GameManager.Instance.UnregisterSideUnit(this);
    }

    public void DestroyUnit() => Destroy(this.gameObject);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float y = transform.position.y + sortingYOffset;
        Vector3 start = new Vector3(transform.position.x - 0.3f, y, 0);
        Vector3 end = new Vector3(transform.position.x + 0.3f, y, 0);
        Gizmos.DrawLine(start, end);
    }

}
