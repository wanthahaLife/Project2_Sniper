using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyBase : RecycleObject, IAlive, IHiitable, IHaveDamage
{
    [Header("몬스터 스텟")]
    /// <summary>
    /// 이동 속도
    /// </summary>
    public float moveSpeed = 5.0f;
    /// <summary>
    /// 현재 이동 속도
    /// </summary>
    public float currMoveSpeed;
    /// <summary>
    /// 현재 이동 속도 설정용 프로퍼티
    /// </summary>
    protected float CurrMoveSpeed
    {
        get => currMoveSpeed;
        set
        {
            currMoveSpeed = value;
            nmAgent.speed = currMoveSpeed;
        }
    }
    /// <summary>
    /// 뛰기 시작 했을 때 속도 증가 비율
    /// </summary>
    public float runSpeedRatio = 1.5f;
    /// <summary>
    /// 점프력
    /// </summary>
    public float jump = 2.0f;
    /// <summary>
    /// 영향 받을 중력 수치
    /// </summary>
    public float gravity = 10.0f;
    /// <summary>
    /// 최대 체력
    /// </summary>
    public float maxHp = 10.0f;
    /// <summary>
    /// 현재 체력
    /// </summary>
    protected float hp;
    /// <summary>
    /// 현재 체력 처리용 프로퍼티
    /// </summary>
    protected float HP
    {
        get => hp;
        set
        {
            hp = value;
            if (hp < 0.1f)
            {
                Die();
            }
        }
    }
    /// <summary>
    /// 공격력
    /// </summary>
    public float damage = 5.0f;
    /// <summary>
    /// 적용되는 현재 데미지
    /// </summary>
    protected float currDamage = 5.0f;
    /// <summary>
    /// 물체(플레이어)를 찾을 수 있는 범위
    /// </summary>
    public float detectionRange = 3.0f;
    /// <summary>
    /// 탐색 범위를 벗어난 경우 추적 시간
    /// </summary>
    public float pursuitTime = 5.0f;
    /// <summary>
    /// 현재 추적 시간
    /// </summary>
    protected float currPursuitTime = 5.0f;
    /// <summary>
    /// 공격 속도 (줄어들수록 빨라짐)
    /// </summary>
    public float attackSpeed = 1.0f;
    /// <summary>
    /// 공격 속도 처리용 프로퍼티
    /// </summary>
    protected float AttackSpeed
    {
        get => attackSpeed;
        set
        {
            attackSpeed = value;
            animator.SetFloat(hash_AttackSpeed, attackSpeed);
            waitTime_Attack = new WaitForSeconds(attackSpeed);
        }
    }
    /// <summary>
    /// 공격 범위
    /// </summary>
    public float attackRange = 0.7f;
    public float AttackRange
    {
        get => attackRange;
        set
        {
            attackRange = value;
            attackCollider.radius = attackRange;
        }
    }
    /// <summary>
    /// 크리티컬 확률
    /// </summary>
    public float critialChance = 0.1f;
    /// <summary>
    /// 크리티컬 발동 시 데미지 증가 비율
    /// </summary>
    public float critialDamageRatio = 1.5f;
    /// <summary>
    /// 피격 시 정지 시간
    /// </summary>
    public float knockbackTime = 0.5f;
    /// <summary>
    /// 피격 시 정지 시간 처리용 프로퍼티
    /// </summary>
    public float KnockbackTime
    {
        get => knockbackTime;
        set
        {
            knockbackTime = value;
            animator.SetFloat(hash_KnockbackTime, 1.0f/knockbackTime); // 피격 애니메이션 속도 조절
        }
    }
    /// <summary>
    /// 피격 시 현재 진행된 정지 시간
    /// </summary>
    protected float currKnockbackTime = 0.5f;
    /// <summary>
    /// 대기 시간
    /// </summary>
    public float stayTime = 0;
    /// <summary>
    /// 현재 대기 시간
    /// </summary>
    protected float currStayTime = 1.0f;

    /// <summary>
    /// 웨이포인트에 적용되는 인덱스 (웨이포인트에서 몇번째 몬스터인지 파악용)
    /// </summary>
    protected int enemyIndexInWaypoint = 0;
    /// <summary>
    /// 웨이포인트에 적용되는 인덱스 프로퍼티 (웨이포인트에서 몇번째 몬스터인지 파악용)
    /// </summary>
    public int EnemyIndex
    {
        get => enemyIndexInWaypoint;
        set => enemyIndexInWaypoint = value;
    }
    /// <summary>
    /// 캐릭터 탐지용 프로퍼티
    /// </summary>
    protected bool IsDetect => Physics.CheckSphere(transform.position, detectionRange, layer_Player);
    /// <summary>
    /// 감지 후 추적 중인지 확인하는 변수 (true면 추적중)
    /// </summary>
    protected bool isPursuit = false;
    /// <summary>
    /// 사망했는지 확인하는 변수 (true면 사망)
    /// </summary>
    protected bool isDie = false;
    /// <summary>
    /// 공격 중인지 확인하는 변수 (true면 공격중)
    /// </summary>
    protected bool isAttacked = false;
    /// <summary>
    /// 정지 시간이 됐는지 확인하는 프로퍼티 (true면 정지)
    /// </summary>
    protected bool IsStay => currStayTime < stayTime;
    /// <summary>
    /// 공격 사이의 시간
    /// </summary>
    protected WaitForSeconds waitTime_Attack;
    /// <summary>
    /// 공격 속도를 위한 코루틴
    /// </summary>
    protected IEnumerator periodicAttack;
    /// <summary>
    /// 이동 방향 (회전에만 사용중)
    /// </summary>
    Vector3 moveDir = Vector3.zero;
    /// <summary>
    /// 목적지에 도착했는지 확인(true면 도착)
    /// </summary>
    protected bool IsArrived => (targetPosition - transform.position).sqrMagnitude < attackRange;
    /// <summary>
    /// 현재 목적지의 위치좌표
    /// </summary>
    public Vector3 targetPosition;
    /// <summary>
    /// 목적지의 위치 좌표를 통해 NavMesh 경로와 방향 벡터를 구하는 프로퍼티
    /// </summary>
    protected Vector3 TargetPosition
    {
        get => targetPosition;
        set
        {
            if (targetPosition != value && value != null)
            {
                targetPosition = value;
                nmAgent.SetDestination(targetPosition);
            }
        }
    }
    // 자식 및 컴포넌트들
    protected Transform character;
    protected Transform hitPosition;
    protected Animator animator;
    protected CharacterController characterController;
    protected NavMeshAgent nmAgent;
    protected SphereCollider attackCollider;

    // 외부 게임 오브젝트들
    protected Player player;
    protected Player AttackTarget;
    protected EnemyWaypoint waypoint;

    /// <summary>
    /// 추적을 시작함을 알리는 델리게이트
    /// </summary>
    public Action pursuitState;
    /// <summary>
    /// 기본 상태로 돌아갔음을 알리는 델리게이트
    /// </summary>
    public Action basicState;
    /// <summary>
    /// 사망 했음을 알리는 델리게이트
    /// </summary>
    Action onDie;

    /// <summary>
    /// 플레이어의 레이어
    /// </summary>
    int layer_Player;

    // 애니메이션 해시들
    protected int hash_IsWalk = Animator.StringToHash("IsWalk");
    int hash_IsDetect = Animator.StringToHash("IsDetect");
    int hash_OnDie = Animator.StringToHash("OnDie");
    int hash_Hit = Animator.StringToHash("Hit");
    int hash_AttackSpeed = Animator.StringToHash("AttackSpeed");
    int hash_KnockbackTime = Animator.StringToHash("KnockbackTime");
    protected int hash_IsAttackRange = Animator.StringToHash("IsAttackRange");



    protected virtual void Awake()
    {
        character = transform.GetChild(0);
        hitPosition = transform.GetChild(1);

        animator = character.GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        nmAgent = GetComponent<NavMeshAgent>();
        attackCollider = GetComponent<SphereCollider>();

        HitPosition hitPos = hitPosition.GetComponent<HitPosition>();
        if(hitPos != null) 
        {
            basicState += hitPos.SetBasicColor;
            pursuitState += hitPos.SetPursuitColor;
        }

        periodicAttack = PeriodicAttack();
    }

    protected virtual void Start()
    {
        player = GameManager.Instance.Player;
        if(player != null)
        {
            onDie += player.KillEnemy;
        }
        layer_Player = 1 << LayerMask.NameToLayer("Player");
        hitPosition.GetComponent<HitPosition>().onHit += HandleHit;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        hp = maxHp;
        CurrMoveSpeed = 0.0f;
        currDamage = damage;
        currPursuitTime = pursuitTime;
        AttackSpeed = attackSpeed;
        KnockbackTime = knockbackTime;
        AttackTarget = null;
        currStayTime = 0;
        currKnockbackTime = knockbackTime;
        TargetPosition = transform.position;
        AttackRange = attackRange;

        isDie = false;
        isAttacked = false;

        SetMove();
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAttack(true);
            AttackTarget = other.GetComponent<Player>();
            StartCoroutine(periodicAttack);
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAttack(false);
            AttackTarget = null;
            StopCoroutine(periodicAttack);
        }
    }
    protected void OnAttack(bool isAttacked)
    {
        this.isAttacked = isAttacked;
        animator.SetBool(hash_IsAttackRange, isAttacked);
    }

    /// <summary>
    /// 공격 간격을 체크하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator PeriodicAttack()
    {
        while (true)
        {
            if (AttackTarget != null)
            {
                AdditionalAttack();
            }
            yield return waitTime_Attack;
        }
    }
    /// <summary>
    /// 공격시 추가 행동 처리용 함수
    /// </summary>
    protected virtual void AdditionalAttack()
    {
        if (AttackTarget != null)
        {
            AttackTarget.HandleHit(currDamage);
        }
    }
    /// <summary>
    /// 걷는 행동 처리용 함수
    /// </summary>
    protected void OnWalk()
    {
        if (IsArrived)
        {
            OnArrived();
            return;
        }
        SetMove();
        animator.SetBool(hash_IsWalk, true);
        TargetPosition = waypoint.currentWaypoint[enemyIndexInWaypoint].position;

    }
    private void Update()
    {
        TimeCheck(Time.deltaTime);
        CheckMoveType();
        moveDir = nmAgent.nextPosition - transform.position;
        moveDir.y = transform.position.y;
        /*if (moveDir.sqrMagnitude > 0.001f)
            character.rotation = Quaternion.LookRotation(moveDir);*/
        //nmAgent.SetDestination(TargetPosition);
    }
    /// <summary>
    /// 현재 행동을 처리하는 함수
    /// </summary>
    protected virtual void CheckMoveType()
    {
        isPursuit = PursuitPlayer();

        if (currKnockbackTime < knockbackTime)      // 현재 넉백 시간을 맞았을 때 0으로 만들기 때문에 먼저 체크
        {
            SetStop();
        }

        if (isAttacked)                             // 공격 중일 때는 움직이지 않음
        {
            AttackAction();
        }
        else if (isPursuit)       // 발견, 추격 중
        {
            DetectAction();
        }
        else if (IsStay)                            // 이동 중 제자리에 서 있는 대기시간
        {
            StayAction();
            BasicStateAction();
        }
        else if (waypoint != null)                  // 웨이포인트가 있으면 웨이포인트 설정에 맞게 이동
        {
            OnWalk();
            BasicStateAction();
        }
    }
    protected virtual void AttackAction()
    {
        SetStop();
    }
    protected virtual void StayAction()
    {
        SetStop();
        animator.SetBool(hash_IsWalk, false);
    }
    protected virtual void BasicStateAction()
    {
        basicState?.Invoke();
    }

    /// <summary>
    /// 플레이어를 발견했을 때 행동을 처리하는 함수
    /// </summary>
    protected virtual void DetectAction()
    {
        currStayTime = 0;

    }
    /// <summary>
    /// 플레이어를 추격 해야하는지 확인하는 함수
    /// </summary>
    /// <param name="isPursuit">추격 중 확인 (true면 추격)</param>
    /// <returns>true면 추격, false면 추격 아님</returns>
    protected bool PursuitPlayer(bool isPursuit = false)
    {
        isPursuit |= (IsDetect & PursuitAdditionalCondition());                          // 플레이어가 감지 범위에 들어 왔거나 추격 중일때
        if (isPursuit)
            currPursuitTime = 0.0f;
        isPursuit = pursuitTime > currPursuitTime;
        animator.SetBool(hash_IsDetect, isPursuit);
        nmAgent.stoppingDistance = attackRange-0.1f;
        return isPursuit;
    }

    protected virtual bool PursuitAdditionalCondition()
    {
        return true;
    }

    /// <summary>
    /// 다음 웨이포인트 세팅하는 함수
    /// </summary>
    protected virtual void OnArrived()
    {
        TargetPosition = waypoint.GetWaypoint(enemyIndexInWaypoint);
        currStayTime = 0;           // 대기 시간 초기화
    }

    /// <summary>
    /// 생성될 때 자신의 웨이포인트에 대한 정보를 지정하는 함수
    /// </summary>
    /// <param name="waypoint"></param>
    public void SetWaypoint(EnemyWaypoint waypoint)
    {
        this.waypoint = waypoint;
        TargetPosition = waypoint.currentWaypoint[enemyIndexInWaypoint].position;
    }

    /// <summary>
    /// 사용되는 모든 시간을 업데이트하는 함수
    /// </summary>
    /// <param name="deltaTime">현재 프레임에 증가된 시간</param>
    protected virtual void TimeCheck(float deltaTime)
    {
        currPursuitTime += deltaTime;
        currStayTime += deltaTime;
        currKnockbackTime += deltaTime;
    }

    /// <summary>
    /// 움직일 수 있도록 세팅하는 함수
    /// </summary>
    /// <param name="speedRatio">이동속도 증가비율</param>
    protected void SetMove(float speedRatio = 1.0f)
    {
        if (!isDie)
        {
            nmAgent.isStopped = false;
            nmAgent.updatePosition = true;
            CurrMoveSpeed = moveSpeed * speedRatio;
        }
    }
    /// <summary>
    /// 움직일 수 없도록 세팅하는 함수
    /// </summary>
    protected void SetStop()
    {
        nmAgent.isStopped = true;
        nmAgent.velocity = Vector3.zero;
        nmAgent.updatePosition = false;
        CurrMoveSpeed = 0.0f;
        nmAgent.stoppingDistance = 0.0f;

    }

    /// <summary>
    /// 피격시 실행되는 함수
    /// </summary>
    /// <param name="damage"></param>
    public virtual void HandleHit(float damage, AttackType attackType = AttackType.Normal)
    {
            HP -= damage;

        if (!isDie)
        {
            currKnockbackTime = 0;
            //animator.SetBool(hash_afterHit, true);
            animator.SetTrigger(hash_Hit);
            PursuitPlayer(true);
        }
    }
    /// <summary>
    /// 사망 처리용 함수
    /// </summary>
    public void Die()
    {
        isDie = true;
        onDie?.Invoke();
        SetStop();
        StartCoroutine(DeadAnimationCor());
    }
    /// <summary>
    /// 사망 처리용 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadAnimationCor()
    {
        animator.SetTrigger(hash_OnDie);                   // 사망 애니메이션 재생
        float animLengh = animator.GetCurrentAnimatorClipInfo(0).Length;    // 현재 재생 중인 (사망) 애니메이션의 길이
        yield return new WaitForSeconds(animLengh);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 지정된 위치로 이동시키는 함수 (생성시 사용)
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    /// <summary>
    /// 캐릭터 넓이를 리턴하는 함수
    /// </summary>
    /// <returns>캐릭터의 넓이</returns>
    public float GetWidth()
    {
        return characterController.radius;
    }
    /// <summary>
    /// 캐릭터 높이를 리턴하는 함수
    /// </summary>
    /// <returns>캐릭터의 높이</returns>
    public float GetHeight()
    {
        return characterController.height;
    }

    public float GetDamge()
    {
        return currDamage;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 감지범위, 공격범위 표시용 기즈모
    /// </summary>
    private void OnDrawGizmos()
    {
        // 감지 범위
        Gizmos.color = new Color(1.0f, 0, 0, 0.1f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = new Color(0, 0, 1.0f, 0.2f);
        Vector3 vec = transform.position;
        vec.y += 0.5f;
        Gizmos.DrawWireSphere(vec, attackRange);
    }
#endif

}
