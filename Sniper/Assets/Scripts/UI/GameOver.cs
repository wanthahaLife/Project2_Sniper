using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    Transform gameOver;
    private void Awake()
    {
        gameOver = transform.GetChild(2);
    }

    private void Start()
    {
        Player player = GameManager.Instance.Player;
        if(player != null)
        {
            player.onDie += SetGameOver;
        }
    }

    public void FinishButtonClick()
    {
        
        SetGameOver();
    }

    void SetGameOver()
    {
        Time.timeScale = 0f;
        gameOver.transform.localScale = Vector3.one;
    }
}
