﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Deathmatch : GameMode
{

	protected override void Start ()
    {
        base.Start();
	}
	
	protected override void Update ()
    {
        if (nextRoundTrigger)
        {
            if (timeBeforeNextRound > 0)
                timeBeforeNextRound -= Time.deltaTime;
            else
            {
                base.KillTeam(null);
                BeginRound();
            }
        }

        base.Update();
    }

    public override void KillTeam(Team team)
    {
        if (!nextRoundTrigger)
        {
            for (int i = 0; i < GameManager.singleton.teams.Length; i++)
            {
                Team t = GameManager.singleton.teams[i].GetComponent<Team>();
                if (t != team && team != null)
                {
                    AddScore(t.name);
                    NetManager.GetInstance().SendScoreUpdate();
                    GameManager.singleton.WriteToLog(t.name + " won the round with " + timeRemaining + " seconds remaining");
                }
            }

            currentRound++;
            if (currentRound >= gameRoundLimit)
            {
                gameActive = false;
                GameManager.singleton.activePlayers = 0;

                // Set the offline scene to "Title" then stop the server and switch to it
                NetManager.GetInstance().offlineScene = "Title"; // change to server you want to change to
                if (GameManager.singleton.isLocalPlayer)
                {
                    NetManager.GetInstance().StopClient();
                }
                else
                {
                    NetManager.GetInstance().StopServer();
                }
            }

            gameActive = true;
            timeRemaining = GameConstants.TimeBeforeNextRound;
        }

        nextRoundTrigger = true;
    }

    public override void AddScore(string name)
    {
        if (name == "Team 0")
            team1Score++;
        else
            team2Score++;

        gameActive = false;
    }
}