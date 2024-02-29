using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_KillScore : MonoBehaviour
{
    TextMeshProUGUI text;
    Player player;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        if (player != null)
        {
            player.killEnemy += KillScoreUpdate;
        }
    }

    void KillScoreUpdate(int killScore)
    {
        text.text = $"Kill : {killScore:d5}";
    }
}
