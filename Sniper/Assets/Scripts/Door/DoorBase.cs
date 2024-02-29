using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : MonoBehaviour
{
    protected Transform hinge;
    Animator animator;

    bool isOpen = false;
    protected virtual bool IsOpen
    {
        get => isOpen;
        set
        {
            isOpen = value;
            animator.SetBool(hash_IsOpen, isOpen);
        }
    }

    int hash_IsOpen = Animator.StringToHash("IsOpen");
    int hash_AnywayOpen = Animator.StringToHash("AnywayOpen");

    private void Awake()
    {
        hinge = transform.GetChild(0);
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnEnterAction(other);
        
    }
    protected virtual void OnEnterAction(Collider other)
    {
        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        OnExitAction(other);
    }

    protected virtual void OnExitAction(Collider other)
    {
        Close();
    }

    protected virtual void Open()
    {
        IsOpen = true;
    }

    protected virtual void Close()
    {
        IsOpen = false;
    }
}
