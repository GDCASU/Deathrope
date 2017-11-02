﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Created by Paul G 9/16/17

public class CanvasScript : MonoBehaviour
{

    public Text scorep1;
    public int p1ScoreNum;
    public Text scorep2;
    public int p2ScoreNum;
    public Text timer;
    public Text speedp1;
    public Text speedp2;
    public Text points;
    public GameObject debugMenu;

    // Use this for initialization
    void Start()
    {
        p1ScoreNum = 0;
        p2ScoreNum = 0;
        scorep1.text = "" + p1ScoreNum;
        scorep2.text = "" + p2ScoreNum;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //Debug.Log("Key was pressd");
            //debugMenu.SetActive(!debugMenu.activeSelf);
        }
        if (GameManager.singleton.matchStarted)
        {
            scorep1.text = GameManager.singleton.teams[0].GetComponent<Team>().points.ToString();
            scorep2.text = GameManager.singleton.teams[1].GetComponent<Team>().points.ToString();
            if (GameManager.singleton.countdownOver)
            {
                float time = GameManager.singleton.deathMatch.time;
                timer.text = Mathf.FloorToInt(time / 60).ToString("00") + ":" + Mathf.FloorToInt(time % 60).ToString("00");
            }
            if (GameManager.singleton.countdownTimer <= 1 && GameManager.singleton.countdownTimer >= 0)
            {
                timer.text = "FIGHT!!!";
            }
        }
        else
        {
            timer.text = Mathf.FloorToInt(GameManager.singleton.countdownTimer).ToString();
        }
    }
}
