using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour
{
    TextMeshProUGUI hpText;
    TextMeshProUGUI killText;
    TextMeshProUGUI scoreText;
    Button restart;
    Player player;

    int hp;
    int kill;
    int score;

    private void Awake()
    {
        Transform child = transform.GetChild(1);
        hpText = child.GetComponent<TextMeshProUGUI>();
        child = transform.GetChild(2);
        killText = child.GetComponent<TextMeshProUGUI>();
        child = transform.GetChild(3);
        scoreText = child.GetComponent<TextMeshProUGUI>();
        restart = child.GetComponentInChildren<Button>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        if (player != null)
        {
            player.onHit += HpScore;
            player.killEnemy += KillScore;
            player.onDie += Score;
        }
    }

    void HpScore(float hp)
    {
        this.hp = (int)hp * 100;
        hpText.text = $"HP : {this.hp:d3}";
    }

    void KillScore(int kill)
    {
        this.kill = kill;
        killText.text = $"Kill : {this.kill:d5}";
    }
    void Score()
    {
        if (hp == 0)
        {
            hp = 1;
        }
        score = kill * hp;
        Debug.Log(hp + " " + kill);
        scoreText.text = $"Score : {score:d5}";
    }


}
