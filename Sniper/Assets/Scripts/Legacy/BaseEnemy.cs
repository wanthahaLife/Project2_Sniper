using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BaseEnemy : RecycleObject//, IAlive, IHiitable
{
    /*
    /// <summary>
    /// 이동 속도
    /// </summary>
    public float moveSpeed = 5.0f;
    /// <summary>
    /// 현재 이동 속도
    /// </summary>
    float currMoveSpeed = 5.0f;
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
    float hp;
    /// <summary>
    /// 현재 체력 처리용 프로퍼티
    /// </summary>
    float HP
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
    float currDamage = 5.0f;
    /// <summary>
    /// 물체(플레이어)를 찾을 수 있는 범위
    /// </summary>
    public float detectionRange = 3.0f;
    /// <summary>
    /// 탐색 범위를 벗어난 경우 추적 시간
    /// </summary>
    public float pursuitTime = 3.0f;
    /// <summary>
    /// 공격 속도 (줄어들수록 빨라짐)
    /// </summary>
    public float attackSpeed = 1.0f;
    /// <summary>
    /// 공격 속도 처리용 프로퍼티
    /// </summary>
    float AttackSpeed
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
    /// <summary>
    /// 크리티컬 확률
    /// </summary>
    public float critialChance = 0.1f;
    /// <summary>
    /// 크리티컬 발동 시 데미지 증가 비율
    /// </summary>
    public float critialDamageRatio = 1.5f;
    /// <summary>
    /// 
    /// </summary>
    int enemyIndex = 0;
    public int EnemyIndex
    {
        get => enemyIndex;
        set => enemyIndex = value;
    }
    Vector3 moveDirection = Vector3.zero;

    bool isDie = false;
    bool isAttacked = false;
    bool isMove = false;

    float time_CurrPursuit = 5.0f;
    WaitForSeconds waitTime_Attack;

    bool DetectPlayer
    {
        get
        {
            bool isDetect = Physics.CheckSphere(transform.position, detectionRange, layer_Player);
            if (isDetect)
                time_CurrPursuit = 0.0f;
            isDetect = pursuitTime > time_CurrPursuit;
            animator.SetBool(hash_IsDetect, isDetect);
            return isDetect;
        }
    }

    IEnumerator periodicAttack;

    
    
    Transform target;
    Transform character;
    Transform hitPosition;
    Animator animator;
    CharacterController characterController;
    NavMeshAgent nmAgent;

    Player player;
    Player AttackTarget;
    EnemyWaypoint waypoint;

    // 레이어
    int layer_Player;

    // 애니메이션 해시
    int hash_IsWalk = Animator.StringToHash("IsWalk");
    int hash_IsDetect = Animator.StringToHash("IsDetect");
    int hash_IsAttackRange = Animator.StringToHash("IsAttackRange");
    int hash_IsCritical = Animator.StringToHash("IsCritical");
    int hash_Dead = Animator.StringToHash("Dead");
    int hash_AttackSpeed = Animator.StringToHash("AttackSpeed");

    Transform Target
    {
        get => target;
        set
        {
            if (!DetectPlayer)
            {
                target = value;
                moveDirection = target.position - transform.position;
            }
        }
    }


    private void Awake()
    {
        character = transform.GetChild(0);
        hitPosition = transform.GetChild(1);

        animator = character.GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        nmAgent = GetComponent<NavMeshAgent>();
        
        
        moveDirection = Vector3.forward;
        HP = maxHp;
        currMoveSpeed = moveSpeed;
        currDamage = damage;
        time_CurrPursuit = pursuitTime;
        AttackSpeed = attackSpeed;

        isAttacked = false;
        isDie = false;
        isMove = false;
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        layer_Player = 1 << LayerMask.NameToLayer("Player");
        hitPosition.GetComponent<HitPosition>().onHit += HandleHit;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        hp = maxHp;
        currMoveSpeed = moveSpeed;
        currDamage = damage;
        time_CurrPursuit = pursuitTime;
        AttackSpeed = attackSpeed;
        AttackTarget = null;

        isDie = false;
        isAttacked = false;
        isMove = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Update()
    {
       CheckMoveType(Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAttack(true);
            AttackTarget = other.GetComponent<Player>();
            periodicAttack = PeriodicAttack();
            StartCoroutine(periodicAttack);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAttack(false);
            AttackTarget = null;
            StopCoroutine(periodicAttack);
        }
    }
    /// <summary>
    /// 공격 시작할 경우 세팅하는 함수
    /// </summary>
    /// <param name="isAttacked"></param>
    void OnAttack(bool isAttacked)
    {
        this.isAttacked = isAttacked;
        animator.SetBool(hash_IsAttackRange, isAttacked);
        currMoveSpeed = 0;
    }

    /// <summary>
    /// 공격 속도마다 공격하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator PeriodicAttack()
    {
        while(AttackTarget != null)
        {
            OnCritical();
            AttackTarget.HandleHit(currDamage);
            yield return waitTime_Attack;
        }
    }

    void OnCritical()
    {
        if (isAttacked)
        {
            if (UnityEngine.Random.value < critialChance)    // 크리티컬 발동
            {
                currDamage = damage * critialDamageRatio;
                animator.SetBool(hash_IsCritical, true);
            }
            else
            {
                currDamage = damage;
                animator.SetBool(hash_IsCritical, false);
            }
        }
    }

    void CheckMoveType(float deltaTime)
    {
        if (isDie)      // 사망하면 바로 빠져나감
            return;

        time_CurrPursuit += deltaTime;

        if (DetectPlayer)   
        {
            currMoveSpeed = moveSpeed * 1.5f;
            moveDirection = player.transform.position - transform.position;
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shout") == true)            // 플레이어가 발견되어 Shout 애니메이션이 재생
            {
                float animTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;    // Shout 애니메이션이
                if(!(animTime >= 1.0f))                                                     // 종료된 상황이 아니면
                    return;                                                                 // 아래 내용은 실행하지 않고 함수 종료
            }
            else if (!isAttacked)
            {
                Move(deltaTime, moveSpeed * runSpeedRatio);
            }
        }
        else if (waypoint != null)
        {
            currMoveSpeed = moveSpeed;
            Target = waypoint.currentWaypoint[enemyIndex];
            if ((target.position - transform.position).sqrMagnitude < 0.1f)
            {
                OnArrived();
            }
            if (!isAttacked)
            {
                animator.SetBool(hash_IsWalk, true);
                Move(deltaTime, moveSpeed);
            }
        }
    }

    void Move(float deltaTime, float speed)
    {
        if(moveDirection.sqrMagnitude > 0.001f)
            character.rotation = Quaternion.LookRotation(moveDirection);
        characterController.Move(speed * deltaTime * moveDirection.normalized);
    }

    void OnArrived()
    {
        Target = waypoint.GetWaypoint(enemyIndex);
    }

    public void SetWaypoint(EnemyWaypoint waypoint)
    {
        this.waypoint = waypoint;
        Target = waypoint.currentWaypoint[enemyIndex];
    }


    public void HandleHit(float damage)
    {
        HP -= damage;
        Debug.Log($"몬스터 체력: {HP}");
    }
    public void Die()
    {
        isDie = true;
        StartCoroutine(DeadAnimationCor());
    }
    IEnumerator DeadAnimationCor()
    {
        float animLengh = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        animator.SetTrigger(hash_Dead);
        yield return new WaitForSeconds(animLengh);
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public float GetWidth()
    {
        return characterController.radius;
    }
    public float GetHeight()
    {
        return characterController.height;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0, 0, 0.1f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(0, 0, 1.0f, 0.2f);
        Vector3 vec = transform.position;
        vec.y += 0.5f;
        Gizmos.DrawWireSphere(vec, attackRange);
    }*/
}
