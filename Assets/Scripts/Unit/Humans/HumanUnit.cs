using static HumanBehaviourInterface;
using static CommonUtils;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using UnityEngine.UIElements;

public abstract class HumanUnit : Unit
{
    [SerializeField] public HumanAction data;
    public FactionData faction;

    public UnitRole role;

    protected float m_Velocity;
    private Vector3 m_LastPositon;

    public Vector3 CurrentVelocity;
    [SerializeField] private Vector3 HomePosition;
    [SerializeField] public Vector3 lastPathRequestTargetPos = new Vector3 (-1500, -1500, -1500);

    public Vector3 lastEnemyPos;

    private bool isFacingRight = true;

    public bool isBuildingUnit = false;

    public bool isRemote;
    public AI ai;
    public Animator anim;

    [Header("检测间隔")]
    [SerializeField] public float checkFrequency = 0.2f;
    public float checkTimer = 0f;
    [SerializeField] protected float pathFoundFrequency = 0.2f;
    public float pathFoundTimer = 0f;

    [Header("检测距离")]
    [SerializeField] public float detectRadius;
    [SerializeField] public float attackRadius;
    [SerializeField] public bool isAOE = false;

    public Unit target;
    public int targetID = -1;

    public Unit lastAttacker = null;
    public float lastAttackTime;

    public float lastTargetInDetectionTime;

    public bool isForcingTarget = false;
    public bool isReturningHome = false;

    public bool isNeedInitPosition = true;

    public bool HasRegisterTarget => target != null;
    protected int ComboCounter;

    public ITargetSelector targetSelector;
    public ICombatBehaviour combatBehaviour;

    public UnitStateMachine stateMachine;
    protected override void Awake()
    {
        base.Awake();
        ai = GetComponent<AI>();
        anim = GetComponentInChildren<Animator>();
        stateMachine = new UnitStateMachine(this);
        InitBaseBehaviors();
        InitData();
    }

    protected override void Start()
    {
        base.Start();
        if(stateMachine == null) stateMachine = new UnitStateMachine(this);
        //stateMachine.Change(new IdleState(this));

        faction = GameManager.Instance.factions[(int)unitSide];

        faction.AddPopWeight(data.popWeight);

        HomePosition = this.transform.position;

        m_LastPositon = transform.position;

        if (!isBuildingUnit)
        {
            if (!GameManager.Instance.liveHumanUnits.Contains(this))
            {
                GameManager.Instance.liveHumanUnits.Add(this);
            }

            ai.arriveTarget += UnitArriveHome;
        }
        if (isNeedInitPosition)
        {
            TransitionTo(UnitStateType.Idle);
            ai.ClearPath();
        }
        if(checkTimer > 0)
        {
            float randomCheckTime = Random.Range(0, 0.99999999f);
            checkTimer = Time.time + randomCheckTime;
        }

    }
    private void UnitArriveHome()
    {
        isReturningHome = false;
    }
    protected abstract void InitBaseBehaviors();

    protected void InitData()
    {
        this.detectRadius = this.data.detectRadius;
        this.attackRadius = this.data.attackRadius;
        this.stats.FullHP = this.data.fullHP;
        this.stats.Attack = this.data.attack;
        this.stats.Armor = this.data.armor;
        this.stats.CritChance = this.data.critChance;
        this.stats.CritMutiple = this.data.critMutiple;
        this.isAOE = this.data.isAOE;
        this.ai.movesSpeed = this.data.moveSpeed;

        if(this is Worker worker)
        {
            worker.MaxHoldResourceNum = this.data.maxHoldResourceNum;
            worker.ChopNum = this.data.chopNum;
        }

        this.target = null;
        this.targetID = -1;

        this.stats.currentHP = this.stats.FullHP;

        if (this is Warrior) role = UnitRole.Melee;
        else if (this is Archer) role = UnitRole.Ranged;
        else if (this is Worker) role = UnitRole.Worker;
    }
    // Update is called once per frame
    protected override void Update()
    {
        if (isDead) return;

        CurrentVelocity = (transform.position - m_LastPositon) / Time.deltaTime;

        m_Velocity = (transform.position - m_LastPositon).magnitude;
        m_LastPositon = transform.position;

        detectPosition = this.transform.position;
        detectPosition.y += 0.5f;


        if (this.ai.IsUnitInGroup && target != null)
        {
            if (IsTargetDetected(this.target) || (combatBehaviour != null && combatBehaviour.CanAttack(this, target)))
            {
                if (target != null && !target.isDead)
                {
                    ai.LeaveGroup();
                    checkTimer = -100; //强制重置计时器
                    pathFoundTimer = -100;
                    UpdateBehaviour();
                }
            }
        }


        if (isForcingTarget && !isReturningHome || IsForcingMoving && !isReturningHome || stateMachine.CurrentState is IdleState && !isForcingTarget && !HasRegisterTarget && !isReturningHome)
        {
            HomePosition = this.transform.position;
        }

        if (Time.time - checkTimer >= checkFrequency)
        {
            checkTimer = Time.time;
            UpdateBehaviour();
        }
    }


    protected virtual void UpdateBehaviour()
    {
        if (target != null && target.isDead)
        {
            HandleTargetDeath();
            return;
        }
        stateMachine?.Update();
    }

    private void HandleTargetDeath()
    {
        target = null;
        targetID = -1;
        if (ai.currentGroup != null)
        {
            ai.LeaveGroup();
        }

        Unit newTarget = targetSelector.SetNewTarget(this);
        if (newTarget != null && !newTarget.isDead)
        {
            isForcingTarget = false;
            this.target = newTarget;
            targetID = newTarget.uniqueID;
            this.lastTargetInDetectionTime = Time.time;
            isReturningHome = false;
            TransitionTo(UnitStateType.Move);
            return;
        }

        if (!isForcingTarget)
        {
            isReturningHome = true;
            if((HomePosition - this.transform.position).sqrMagnitude >0.25f)
            {
                MoveToDestinationFrame(HomePosition);
                TransitionTo(UnitStateType.Move);
            }
            else
            {
                ai.ClearPath();
                TransitionTo(UnitStateType.Idle);
            }
        }
        else
        {
            isReturningHome = false;
            ai.isForcingMoving = false;
            HomePosition = this.transform.position;
            ai.ClearPath();
            TransitionTo(UnitStateType.Idle);
        }
        isForcingTarget = false;
    }

    /*public virtual void ExecuteChaseLogic()
    {
        if (Time.time - pathFoundTimer >= pathFoundFrequency)
        {
            pathFoundTimer = Time.time;
            if (target != null)
            {
                if (ai.IsUnitInGroup)
                {
                    if (ai.currentGroup.leader == this)
                        ai.currentGroup.UpdateGroupChase();
                    return;
                }
                Vector3 currentTargetPos = target.transform.position;
                if (ai.IsPathVaild())
                {
                    if (Vector2.Distance(currentTargetPos, lastEnemyPos) > .2f)
                    {
                        RequestNewPath(currentTargetPos);
                    }
                }
                else
                {
                    RequestNewPath(currentTargetPos);
                }
            }
        }
    }*/
    public bool CanRequestNewChasingPath() => Time.time - pathFoundTimer >= pathFoundFrequency;
    public void RequestNewPath(Vector3 pos)
    {
        lastEnemyPos = pos;
        if (target is not HumanUnit)
        {
            Node targetNode = TilemapManager.Instance.GetClosestInteractableNode(target, this.transform.position, this.gameObject);
            if (targetNode == null) return;
            pos = targetNode.GetNodePosition();
            lastEnemyPos = pos;
        }

        MoveToDestinationFrame(pos);
    }
    public void TransitionTo(UnitStateType newState) => stateMachine.Change(newState);
    public void SetHomePosition(Vector3 pos) => HomePosition = pos;
    public Vector3 GetLastDetectPosition()
    {
        Vector3 res = m_LastPositon;
        res.y += 0.5f;
        return res;
    }
    public void ForcingMoveToDestination(Vector2 destination)
    {
        isReturningHome = false;
        if (this is Worker worker)
        {
            worker.ResourceAreaID = -1;
            if(worker.currentResource != null)
            {
                worker.currentResource.WorkderReleaseSlot(this);
                worker.currentResource = null;
            }
        }
        UnassignTarget();
        ai.ForceMovingToDesitination(destination);
        TransitionTo(UnitStateType.Move);
    }
    public void ForcingMoveToDestination(List<Node> newPath, UnitGroup group)
    {
        isReturningHome = false;

        if(group.targetUnit is not ResourceUnit)
        {
            if (this is Worker worker)
            {
                worker.ResourceAreaID = -1;
            }
        }

        if (group.targetUnit == null)
        {
            UnassignTarget();
        }

        ai.ForceRegisterPath(newPath, group);
        TransitionTo(UnitStateType.Move);
    }
    public void MoveToDestinationFrame(Vector2 position)
    {
        Vector3 pos = new Vector3(position.x, position.y, this.transform.position.z);
        //FlipController(pos);
        ai.RegisterDestinationFrame(pos);
    }
    public void FlipController(Vector2 destinationPos)
    {
        if (destinationPos.x < this.transform.position.x && isFacingRight)
        {
            FlipSprite();
        }
        else if (destinationPos.x > this.transform.position.x && !isFacingRight)
        {
            FlipSprite();
        }

    }

    protected void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void SetClickTarget(Unit unit)
    {
        if (!targetSelector.IsTargetReachable(this, unit, true)) { return; }
        UnassignTarget();
        TransitionTo(UnitStateType.Move);
        if (this is Worker worker)
        {
            if (unit is ResourceUnit resource)
            {
                if (resource.CanAddWorker)
                {
                    worker.ResourceAreaID = resource.resourceAreaID;
                }
                else
                {
                    ai.ClearPath();
                    TransitionTo(UnitStateType.Idle);
                    return;
                }
            }
            else
            {
                worker.ResourceAreaID = -1;
            }
        }
        this.target = unit;
        this.targetID = unit.uniqueID;
        this.isForcingTarget = true;
        this.isReturningHome = false;

        /* if (combatBehaviour.CanAttack(this, target))
         {
             if (target is BuildingUnit building)
             {
                 if (building.unitSide == this.unitSide)
                 {
                     TransitionTo(UnitStateType.Work);
                 }
                 else
                 {
                     TransitionTo(UnitStateType.Attack);
                 }
             }
             else if (target is ResourceUnit)
             {
                 TransitionTo(UnitStateType.Work);
             }
         }
         else*/
        TransitionTo(UnitStateType.Move);
    }
    public Vector3 GetDestination()
    {
        return ai.GetDestination();
    }

    public void UnassignTarget()
    {
        target = null;
        targetID = -1;
    }
    public void GiveUpChasingTarget()
    {
        UnassignTarget();
        MoveToDestinationFrame(HomePosition);
        //TransitionTo(UnitStateType.Idle);
    }

    public bool IsForcingMoving => ai.isForcingMoving;
    public bool IsTargetDetected(Unit enemy)
    {
        if (enemy == null) return false;

        Vector3 enemyPosition = enemy.detectPosition;

        if (enemy is not HumanUnit)
        {
            Collider2D col = enemy.GetComponent<Collider2D>();
            Bounds b = col.bounds;
            if (col != null)
                //enemyPosition = col.ClosestPoint(this.detectPosition);
                enemyPosition = ClosestPoint(this.detectPosition, b);
            else
                enemyPosition = enemy.transform.position;
        }

        //var distance = Vector2.Distance(enemyPosition, detectPosition);

        return IsInRange(enemyPosition, detectPosition, detectRadius);
    }
    public bool IsTargetNoBlock()
    {
        Node currentNode = TilemapManager.Instance.FindNode(this.transform.position);
        Node targetNode = TilemapManager.Instance.FindNode(target.transform.position);


        if (this.target is not HumanUnit)
        {
            targetNode = TilemapManager.Instance.GetClosestInteractableNode(target, this.transform.position, this.gameObject);
        }

        if (currentNode == null || targetNode == null)
            return false;

        return TilemapManager.Instance.IsNoBlockBetween2Nodes(currentNode.GetNodePosition(), targetNode.GetNodePosition());
    }
    public Vector3 GetTargetAimPoint(Unit target = null)
    {
        Unit currentUnit = target == null? this.target : target;
        if (target == null) target = this.target;
        if (currentUnit == null) return Vector3.zero;
        if (target is not HumanUnit)
        {
            Collider2D col = target.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                //Debug.Log(col.ClosestPoint(detectPosition));
                //return col.ClosestPoint(detectPosition);
                Bounds b = col.bounds;
                return ClosestPoint(this.detectPosition, b);

            }
        }
        return target.detectPosition;
    }


    public Dictionary<int, float> myUnreachableCache = new Dictionary<int, float>();

    public void ClearLocalCache()
    {
        myUnreachableCache.Clear();
    }

    public virtual void OnExitWorkState()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(detectPosition, detectRadius);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(detectPosition, attackRadius);

        Gizmos.color = Color.yellow;
        float y = transform.position.y + sortingYOffset;
        Vector3 start = new Vector3(transform.position.x - 0.3f, y, 0);
        Vector3 end = new Vector3(transform.position.x + 0.3f, y, 0);
        Gizmos.DrawLine(start, end);
    }

    public override void Death()
    {
        base.Death();
        ai.arriveTarget -= UnitArriveHome;
        stateMachine.CurrentState.Exit();
        GameManager.Instance.liveHumanUnits.Remove(this);
        if(stateMachine.CurrentState is not EnteringState)
        {
            faction.ReleasePopWeight(this.data.popWeight);
            anim.SetTrigger("Death");
        }
        else
        {
            DestroyUnit();
        }

    }
    public abstract void PreformAttackAnimation();
    public void AnimationFinishTrigger1()
    {
        anim.SetBool("Attack_Horizontal", false);
        anim.SetBool("Attack_Up", false);
        anim.SetBool("Attack_Down", false);

        //AnimationCounterAdd();
    }
    public void AnimationFinishTrigger2()
    {
        anim.SetBool("Attack_Up", false);
        anim.SetBool("Attack_SlantedUp", false);
        anim.SetBool("Attack_Horizontal", false);
        anim.SetBool("Attack_SlantedDown", false);
        anim.SetBool("Attack_Down", false);
    }
    public void AnimationFinishTrigger3()
    {
        anim.SetBool("Build", false);
        anim.SetBool("Chop", false);
    }
    public void AnimationCounterRandomAdd()
    {
        ComboCounter = (ComboCounter + Random.Range(0, 5)) % 2;
        anim.SetInteger("comboCounter", this.ComboCounter);
    }

    public UnitSaveData ToSaveData()
    {
        lastPathRequestTargetPos = ai.GetDestination();
        return new UnitSaveData(this);
    }

    public void LoadData(UnitSaveData data)
    {

        this.uniqueID = data.ID;
        this.unitSide = (UnitSide)data.unitSide;
        this.transform.position = new Vector3(data.position.x, data.position.y, data.position.z);

        this.stats.currentHP = data.currentHP;

        this.isForcingTarget = data.isForcingTarget;
        this.targetID = data.targetID;

        this.ai.isForcingMoving = data.isForcingMoving;

        this.lastPathRequestTargetPos = new Vector3(data.destiantion.x, data.destiantion.y, data.destiantion.z);

        this.isReturningHome = data.isReturningHome;

        if(this is Worker worker)
        {
            worker.holdResourceNum = data.holdResourceNum;

            worker.ResourceAreaID = data.resourceAreaID;

            worker.currentChopNum = data.currentChopNum;
        }

        this.isNeedInitPosition = false;

        faction = GameManager.Instance.factions[(int)unitSide];
    }
    public void ResumeLogic()
    {
        if (targetID != -1)
        {
            target = GameManager.Instance.liveHumanUnits.Find(h => h.uniqueID == targetID);

            if (target == null)
            {
                target = GameManager.Instance.buildings.Find(b => b.uniqueID == targetID);
            }
            if(target == null)
            {
                target = GameManager.Instance.resources.Find(r => r.uniqueID == targetID);
            }
        }

        
        if (ai.IsUnitInGroup) return;

        if (target != null)
        {
            if (this is Worker work && target is BuildingUnit building && building.unitSide == this.unitSide && work.ResourceAreaID != -1)
            {
                target = null;
                targetID = -1;
                TransitionTo(UnitStateType.Deliver);
                return;
            }
            if (combatBehaviour.CanAttack(this,target))
            {
                if (this is Worker && target is (BuildingUnit or ResourceUnit))
                    TransitionTo(UnitStateType.Work);
                else
                    TransitionTo(UnitStateType.Attack);
            }
            else
            {
                MoveToDestinationFrame(target.transform.position);
                TransitionTo(UnitStateType.Move);

            }
            return;
        }
        if (lastPathRequestTargetPos != new Vector3(-1500, -1500, -1500) && (this.transform.position - lastPathRequestTargetPos).sqrMagnitude > 0.16f)
        {
            if (!IsForcingMoving)
                MoveToDestinationFrame(lastPathRequestTargetPos);
            else
                ForcingMoveToDestination(lastPathRequestTargetPos);
            TransitionTo(UnitStateType.Move);
            return;
        }
        ai.ClearPath();
        TransitionTo(UnitStateType.Idle);
    }
}