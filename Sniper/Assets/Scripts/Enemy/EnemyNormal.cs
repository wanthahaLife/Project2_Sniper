using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyNormal : EnemyBase
{
    int hash_IsCritical = Animator.StringToHash("IsCritical");

    protected override void DetectAction()
    {
        TargetPosition = player.transform.position;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shout") == true)            // 플레이어가 발견되어 Shout 애니메이션이 재생
        {
            SetStop();
            pursuitState?.Invoke();
            //float animTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;    // Shout 애니메이션이
            //if (animTime >= 1.0f)                                                       // 종료된 상황이 아니면
            //{

            //}
        }
        // 달리기 시작
        else
        {
            //animator.SetBool(hash_afterHit, false);
            SetMove(1.5f);
        }
    }

    protected override void AdditionalAttack()
    {
        if (isAttacked && AttackTarget != null)
        {
            if (Random.value < critialChance)    // 크리티컬 발동
            {
                currDamage = damage * critialDamageRatio;
                animator.SetBool(hash_IsCritical, true);
                
            }
            else
            {
                currDamage = damage;
                animator.SetBool(hash_IsCritical, false);
            }
            AttackTarget.HandleHit(currDamage);
        }
    }

}
