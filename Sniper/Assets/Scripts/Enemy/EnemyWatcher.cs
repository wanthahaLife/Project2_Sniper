using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWatcher : EnemyBase
{
    [Header("터렛 데이터")]
    public float SleepTime = 2.0f;
    float currSleepTime = 0.0f;
    public float awakeTime = 2.0f;
    float currAwakeTime = 0.0f;
    public float attackPointSpeed = 2.0f;

    Transform attackPoint;
    Player targetData = null;

    bool isAwake = false;

    protected override void Awake()
    {
        base.Awake();

        attackPoint = transform.GetChild(2);

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currSleepTime = SleepTime;
        currAwakeTime = 0.0f;
        attackPoint.localPosition = Vector3.zero;
        attackPoint.localScale = Vector3.zero;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetData = other.GetComponent<Player>();
            isAttacked = true;
            StartCoroutine(periodicAttack);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        attackPoint.localPosition = Vector3.zero;
        attackPoint.localScale = Vector3.zero;
    }
    protected override void AttackAction()
    {
        if (isPursuit)
        {
            attackPoint.localScale = Vector3.one;
            base.AttackAction();

            if (TraceAttackPoint())
            {
                AttackTarget = targetData;
                OnAttack(true);
            }
            else
            {
                animator.SetBool(hash_IsAttackRange, false);
                AttackTarget = null;
            }
        }
        else
        {
            AwakePeriod();
        }
    }

    protected override void AdditionalAttack()
    {
        Tentacle tentacle = EnemyFactory.Instance.GetTentacle(attackPoint.position);
        tentacle.OnInitialized(attackPoint);
    }



    bool TraceAttackPoint()
    {
        attackPoint.position = Vector3.Lerp(attackPoint.position, targetData.transform.position, Time.deltaTime * attackPointSpeed);
        float dif = (attackPoint.position - targetData.transform.position).sqrMagnitude;
        return dif < 0.1f;
    }

    protected override void DetectAction()
    {
        base.DetectAction();
        animator.SetBool(hash_IsWalk, true);
    }

    protected override bool PursuitAdditionalCondition()
    {
        return isAwake|isAttacked;
    }

    void AwakePeriod()
    {
        // 자고있을 때
        if (!isAwake && currSleepTime > SleepTime)
        {
            animator.SetBool(hash_IsWalk, true);
            isAwake = true;
            currAwakeTime = 0f;
        }
        // 깨있을 때
        if(isAwake && currAwakeTime > awakeTime)
        {
            animator.SetBool(hash_IsWalk, false);
            isAwake = false;
            currSleepTime = 0f;
        }
    }

    protected override void StayAction()
    {
        SetStop();
    }

    protected override void TimeCheck(float deltaTime)
    {
        base.TimeCheck(deltaTime);
        currAwakeTime += deltaTime;
        currSleepTime += deltaTime;
    }

    protected override void BasicStateAction()
    {
        base.BasicStateAction();
        AwakePeriod();
    }
}
