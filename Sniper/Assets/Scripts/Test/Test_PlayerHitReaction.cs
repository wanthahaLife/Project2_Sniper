using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_PlayerHitReaction : TestBase
{
    Player player;

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player.HandleHit(1);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        player.HandleHit(1, AttackType.Strong);
    }
}
