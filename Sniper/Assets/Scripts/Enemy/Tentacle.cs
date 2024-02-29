using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tentacle : RecycleObject
{
    public AttackType attackType = AttackType.Strong;

    Transform defaultParent;

    Animator animator;

    IHaveDamage attacker;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        float animLen = animator.GetCurrentAnimatorClipInfo(0).Length;
        StartCoroutine(LifeOver(animLen));
    }

    protected override void PreDisableAction()
    {
        if (defaultParent != null)
        {
            transform.parent = defaultParent;
        }

    }

    public void OnInitialized(Transform parent)
    {
        defaultParent = transform.parent;
        transform.parent = parent;
        attacker = transform.GetComponentInParent<IHaveDamage>();
    }
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.HandleHit(attacker.GetDamge(), attackType);
        }
    }
}
