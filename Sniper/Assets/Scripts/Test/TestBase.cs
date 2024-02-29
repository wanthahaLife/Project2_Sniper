using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestBase : MonoBehaviour
{
    protected TestInputActions testInput;
    private void Awake()
    {
        testInput = new TestInputActions();
    }

    private void OnEnable()
    {
        testInput.Enable();
        testInput.Test.Test1.performed += OnTest1;
        testInput.Test.Test2.performed += OnTest2;
        testInput.Test.Test3.performed += OnTest3;
        testInput.Test.Test4.performed += OnTest4;
        testInput.Test.Test5.performed += OnTest5;
        testInput.Test.LeftClick.performed += OnLeftClick;
        testInput.Test.RightClick.performed += OnRightClick;
    }

    protected virtual void OnTest1(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    protected virtual void OnTest2(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    protected virtual void OnTest3(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    protected virtual void OnTest4(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    protected virtual void OnTest5(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnLeftClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    protected virtual void OnRightClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    
}

