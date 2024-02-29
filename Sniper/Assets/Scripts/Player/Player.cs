using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 이동 : rigidbody 이용
public class Player : MonoBehaviour, IAlive, IHiitable, IHaveDamage
{
    public Texture2D combatIcon;
    public Texture2D aimingIcon;
    [Header("플레이어 스텟 데이터")]
    public float moveSpeed = 5.0f;
    float currMoveSpeed = 5.0f;
    public float attackSpeed = 1.0f;
    public float maxHp = 100.0f;
    public float damage = 10.0f;
    float hp = 100.0f;

    [Header("플레이어 게임내부 데이터")]
    public float rotateSpeed = 2.0f;
    [Range(0f, 1f)]
    public float maxMoveHeight = 0.25f;
    [Range(0f, 90f)]
    public float maxSlopeAngle = 50.0f;
    public float invincibleTime = 1.0f;
    public float knockbackTime = 0.5f;
    public float knockbackNormal = 1.0f;
    public float knockbackStrong = 2.0f;

    bool isInvincible = false;
    int killScore = 0;

    [Header("Checkbox 데이터")]
    [Range(0f, 1f)]
    public float checkboxX = 0.2f;
    [Range(0f, 1f)]
    public float checkboxZ = 0.2f;

    PlayerInputActions inputActions;
    Rigidbody rigid;
    Transform character;
    Transform checkbox;
    CapsuleCollider capsuleCollider;
    Animator animator;

    const float SlopeCheckRay_MaxDistance = 1.0f;

    int hash_IsMove = Animator.StringToHash("IsMove");
    int hash_Fire = Animator.StringToHash("Fire");
    int hash_IsAiming = Animator.StringToHash("IsAiming");
    int hash_HitNormal = Animator.StringToHash("HitNormal");
    int hash_HitStrong = Animator.StringToHash("HitStrong");

    Vector3 inputDir;
    Vector3 shotPos;
    RaycastHit slopeInfo;

    public Action<int> killEnemy;
    public Action<float> onHit;
    public Action onDie;

    Cinemachine.CinemachineVirtualCamera virtualCamera;

    float HP
    {
        get => hp;
        set
        {
            hp = value;
            onHit?.Invoke(hp/maxHp);
            if (hp < 0.1f)
            {
                hp = 0.0f;
                Die();
            }
            else if(hp > maxHp)
            {
                hp = maxHp;
            }
        }
    }

    public bool onAir = false;
    /// <summary>
    /// 현재 에임 상태인지 확인하는 변수 (true면 에이밍 중)
    /// </summary>
    bool isAimingState = false;
    bool IsAiming
    {
        get => isAimingState;
        set
        {
            if (isAimingState)   // 현재 에임 상태이면
            {
                isAimingState = value;
                Cursor.SetCursor(combatIcon, new Vector2(combatIcon.width*0.5f, combatIcon.height*0.5f), CursorMode.Auto);
                RestoreMoveSpeed();
            }
            else            // 현재 에임 상태가 아니라면
            {
                isAimingState = value;
                Cursor.SetCursor(aimingIcon, new Vector2(aimingIcon.width * 0.5f, aimingIcon.height * 0.5f), CursorMode.Auto);
                MoveSpeedZero();
            }
            animator.SetBool(hash_IsAiming, isAimingState);
        }
    }
    readonly float SmoothMoveValue = 0.3f;

    int layerMask_Ground;
    int layerMask_Enemy;
    int excludedLayer_Enemy;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        character = transform.GetChild(0);
        checkbox = transform.GetChild(1);

        currMoveSpeed = moveSpeed;
        killScore = 0;

        SetCheckBox();

    }

    private void Start()
    {
        layerMask_Ground = 1 << LayerMask.NameToLayer("Ground");    // 해당 레이어 마스크의 비트 켜주기
        layerMask_Enemy = 1 << LayerMask.NameToLayer("Enemy");
        excludedLayer_Enemy = (-1) - (layerMask_Enemy);
        Cursor.SetCursor(combatIcon, Vector2.zero, CursorMode.Auto);
        virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();

        HP = maxHp;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += Move;
        inputActions.Player.Move.canceled += Move;
        inputActions.Player.Fire.performed += Fire;
        inputActions.Player.Aim.performed += Aiming;
    }
    private void OnDisable()
    {
        inputActions.Player.Aim.performed -= Aiming;
        inputActions.Player.Fire.performed -= Fire;
        inputActions.Player.Move.canceled -= Move;
        inputActions.Player.Move.performed -= Move;
        inputActions.Disable();
    }
    private void FixedUpdate()
    {
        MoveAction();
        if (isAimingState)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 lookDir = hit.point;
                lookDir.y = character.position.y;
                character.LookAt(lookDir);
            }
        }
    }

    private void Aiming(InputAction.CallbackContext context)
    {
        IsAiming = !isAimingState;
    }

    private void Fire(InputAction.CallbackContext context)
    {
        if (isAimingState)
        {
            animator.SetTrigger(hash_Fire);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, excludedLayer_Enemy))
            {
                shotPos = hit.point;
                shotPos.y = transform.position.y;
                character.LookAt(shotPos);
                HitPosition hitPosition = hit.transform.GetComponent<HitPosition>();
                //Debug.DrawRay(ray.origin, ray.direction * 10.0f, Color.black, 5f);
                if (hitPosition != null)
                {
                    hitPosition.HandleHit(damage);
                }
            }
            IsAiming = false;
            MoveSpeedZero();
        }
    }

    private void Move(InputAction.CallbackContext context)
    {
        inputDir = context.ReadValue<Vector3>();
        animator.SetBool(hash_IsMove, inputDir != Vector3.zero);
        //animator.SetBool(hash_IsMove, !context.canceled);
    }

    /// <summary>
    /// 체크박스 크기 설정
    /// </summary>
    void SetCheckBox()
    {
        float size = capsuleCollider.radius * 2;
        checkboxX *= size;
        checkboxZ *= size;
    }

    /// <summary>
    /// 바닥에 닿아있는지 확인하는 체크박스용 함수
    /// </summary>
    /// <returns></returns>
    bool OnGround(float y)
    {
        Vector3 boxSize = new Vector3(checkboxX, y, checkboxZ);
        return Physics.CheckBox(checkbox.position, boxSize, Quaternion.identity, layerMask_Ground);
    }

    /// <summary>
    /// 입력 받은 방향으로 움직임
    /// </summary>
    private void MoveAction()
    {
        float checkboxY = maxMoveHeight;
        Vector3 dir = inputDir;
        if (OnGround(checkboxY))
        {
            rigid.constraints = RigidbodyConstraints.FreezeRotation;
            if (IsSlope())                      // 경사면인지 확인
            {
                checkboxY = 0.01f;
                dir = SlopeDirection();         // 방향벡터를 경사각도, 입력값으로 바꾸고
                rigid.useGravity = false;       // 중력 사용 x(미끄러짐 방지)
                rigid.drag = 100000;            // 마찰력 큰 수 
            }
            else if (!rigid.useGravity)          // 경사면이 아니고 이전에 경사면에 들어왔으면          
            {
                rigid.useGravity = true;        // 중력과 마찰력 원래대로
                rigid.drag = 0;
            }

            if ((inputDir.z != 0 || inputDir.x != 0) && SlopeAvailable())     // x,z 축 입력이 들어오고, 점프(땅에 발이 떨어진 상태)가 아닐 때 움직일 수 있음
            {
                MoveThreshold();
                rigid.MovePosition(rigid.position + currMoveSpeed * Time.fixedDeltaTime * dir);
                RotateDirection(new Vector3(inputDir.x, 0, inputDir.z));
            }
        }
        else
        {
            rigid.useGravity = true;        // 중력과 마찰력 원래대로
            rigid.drag = 0;
        }
    }
    /// <summary>
    /// 고개 입력 방향으로 돌리기
    /// </summary>
    /// <param name="dir">입력 방향</param>
    void RotateDirection(Vector3 dir)
    {
        Quaternion rotate = Quaternion.Slerp(character.rotation, Quaternion.LookRotation(dir) , Time.fixedDeltaTime * rotateSpeed);
        //Quaternion rotate = Quaternion.LookRotation(dir);
        character.rotation = rotate;
    }
    /// <summary>
    /// 경사면인지 확인하는 함수 (경사면이면 true, 아니면 false)
    /// </summary>
    /// <returns>경사면이면 true, 아니면 false</returns>
    bool IsSlope()
    {
        Vector3 rayPoint = transform.position;
        rayPoint.y += 0.5f;
        
        Ray ray = new(rayPoint, Vector3.down);    // 현재 위치에서 아래로 레이 쏨
        if(Physics.Raycast(ray, out slopeInfo, SlopeCheckRay_MaxDistance, layerMask_Ground))   // 아래로 쏜 레이에 Ground 레이어가 걸리면
        {
            float angle = Vector3.Angle(Vector3.up, slopeInfo.normal);  // 바닥의 노말 벡터와 수평 벡터의 각도 비교 후
            if (angle > maxSlopeAngle)
            {
                return false;
            }
            else if (angle > 0.0f)                                           // 0 이 아니면 (같은 각도가 아니면) 경사
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 경사면 방향을 가르키는 방향 벡터 찾기
    /// </summary>
    /// <returns>경사면 방향 벡터</returns>
    Vector3 SlopeDirection()
    {
        // ProjectOnPlane(A, B) : B를 노말벡터로 가지는 평면에 A를 투영
        return Vector3.ProjectOnPlane(inputDir, slopeInfo.normal).normalized;
    }
    bool SlopeAvailable()
    {
        if (Vector3.Angle(slopeInfo.normal, Vector3.up) < maxSlopeAngle)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 넘을 수 있는 언덕을 파악하고 넘는 함수
    /// </summary>
    void MoveThreshold()
    {
        Vector3 rayPoint = checkbox.position;
        Ray ray = new(rayPoint, character.forward);    // 캐릭터의 앞방향(회전포함)으로 레이 쏨
        RaycastHit hitInfo;
        rayPoint.y += 0.01f;
        if(Physics.Raycast(ray, out hitInfo, capsuleCollider.radius+0.2f, layerMask_Ground))
        {
            rayPoint.y += maxMoveHeight;
            ray = new(rayPoint, character.forward);
            if (!Physics.Raycast(ray, out hitInfo, capsuleCollider.radius+0.2f, layerMask_Ground))
            {
                rigid.position += new Vector3(0f, maxMoveHeight* SmoothMoveValue, 0f);
            }
            else
            {
                // 90도 코너에 계속 비비면 벽타고 올라가는 버그 수정
                rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
        }
    }

    public void MoveSpeedZero()
    {
        currMoveSpeed = 0f;
        inputActions.Player.Move.Disable();
    }

    public void RestoreMoveSpeed()
    {
        currMoveSpeed = moveSpeed;
        inputActions.Player.Move.Enable();
    }

    public void HandleHit(float damage, AttackType attackType = AttackType.Normal)
    {
        if (!isInvincible)
        {
            MoveSpeedZero();
            rigid.AddForce(-character.forward * 1.0f, ForceMode.Impulse);
            StartCoroutine(OnInvincible(invincibleTime));

            switch (attackType)
            {
                case AttackType.Normal:
                    animator.SetTrigger(hash_HitNormal);
                    break;
                case AttackType.Strong:
                    animator.SetTrigger(hash_HitStrong);
                    break;
            }
            HP -= damage;
            Invoke("RestoreMoveSpeed", knockbackTime);
        }
    }

    IEnumerator OnInvincible(float time)
    {
        isInvincible = true;
        yield return new WaitForSeconds(time);
        isInvincible = false;
    }

    public void Die()
    {
        onDie?.Invoke();
    }

    public float GetDamge()
    {
        return damage;
    }

    public void KillEnemy()
    {
        killScore++;
        killEnemy?.Invoke(killScore);
    }


#if UNITY_EDITOR
    Vector3 gizmoUse_clickPoint = Vector3.zero;
    private void OnDrawGizmos()
    {
        // Physics 체크박스 기즈모
        Gizmos.color = Color.red;
        Vector3 boxsize = new Vector3(checkboxX, maxMoveHeight, checkboxZ);
        Gizmos.DrawWireCube(transform.position, boxsize);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(gizmoUse_clickPoint, 0.01f);
    }


#endif

}
