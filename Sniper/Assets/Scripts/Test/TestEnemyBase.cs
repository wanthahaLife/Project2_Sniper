using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TestEnemyBase : TestBase
{
    //NavMeshAgent agent;

    [SerializeField] LayerMask layerMasks;

    public float rotateSpeed = 2.0f;
    public float moveSpeed = 3.0f;

    Vector3 inputDir = Vector3.zero;

    private void Start()
    {
       // agent = GetComponent<NavMeshAgent>();


    }

    protected override void OnLeftClick(InputAction.CallbackContext context)
    {
        GetClickPoint();
    }

    void GetClickPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());     // ScreenPointToRay(Input.mousePosition) 도 가능
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo)) 
        {
            inputDir = hitInfo.point;
        }
    }

    private void Update()
    {
        ClickToMove();
    }
    void ClickToMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, inputDir, moveSpeed * Time.deltaTime);
    }
}
