using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HPBar : MonoBehaviour
{
    Slider slider;
    Player player;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }
    private void Start()
    {

        player = GameManager.Instance.Player;
        if (player != null)
        {
            player.onHit += HpBarUpdate;
        }
    }

    void HpBarUpdate(float hpRatio)
    {
        slider.value = hpRatio;
    }
}
