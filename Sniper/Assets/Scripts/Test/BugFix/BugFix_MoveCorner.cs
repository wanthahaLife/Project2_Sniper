using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BugFix_MoveCorner : MonoBehaviour
{
    Rigidbody rigid;

    PlayerInputActions inputActions;
    Vector3 dir;
    float moveSpeed = 2.0f;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += Move;
        inputActions.Player.Move.canceled += Move;
    }
    private void FixedUpdate()
    {
        rigid.MovePosition(rigid.position + dir * Time.fixedDeltaTime * moveSpeed);
    }

    private void Move(InputAction.CallbackContext context)
    {
        dir = context.ReadValue<Vector3>();
    }



}
