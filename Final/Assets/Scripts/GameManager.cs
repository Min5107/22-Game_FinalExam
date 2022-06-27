using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;

    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        // Chage Stage
        if (stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();
            
            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else 
        { 
          //Game Clear  
          Time.timeScale = 0;
          Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
          btnText.text = "Clear!";
          UIRestartBtn.SetActive(true);
          
        }
        // Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }
            
        else
        {
            UIhealth[0].color = new Color(1, 0, 0, 0.4f);
            // Player Die Effect
            player.OnDie();
            // Result UI
            // Retry Button UI
            UIRestartBtn.SetActive(true);
        }
    }
    public void HealthUp()
    {

        if (health <= 2)
        {
            UIhealth[health].color = new Color(1, 1, 1, 1f);
            health++;
            
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (health > 1)
            {
                PlayerReposition();
            }
            HealthDown();
        }
        

    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0,0,-1);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Test");
    }
}
