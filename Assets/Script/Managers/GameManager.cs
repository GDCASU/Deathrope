﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/*
 * GameManager controls spawning the players in the beginning of the game, 
 * it controls the amount of players allowed in the game.
 * 
 * Date created: 9/15/17 Connor Pillsbury
 * Revised: 9/20/17 Connor Pillsbury
 */

// Developer:   Kyle Aycock
// Date:        9/21/17
// Description: Fixed documentation (changed to C# XML)
//              Changed spawn point system to allow for individual spawn points for each player of a team
//              Replaced IsDestroyed with KillTeam, which resets team positions and advances round counter by 1
//              Added round counter, returns to title screen after a match (set of rounds) completes

// Developer:   Paul Gellai
// Date:        9/27/17
// Description: Added matchStarted boolean to make sure that the game timer does not begin counting down until 
// the countdown timer has finished. Added countdown timer for 3..2..1 at beginning of rounds.

// Developer:   Nick Arnieri
// Date:        10/20/2017
// Description: Switch usage of game rules to DeathMatchRules instead of hard coded determination

// Developer:   Kyle Aycock
// Date:        11/17/17
// Description: Changed spawning system & controls to work with networking, added documentation and
//              rearranged update method. Need to fix titlescreen-skip functionality

// Developer:   Kyle Aycock
// Date:        11/17/17
// Description: Turns out this wasn't networked properly, dunno why I thought it was

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton;

    public GameObject teamPlayerPrefab;
    public GameObject teamAiPrefab;
    public GameObject[] teams;
    public GameMode currentGame;

    public int maxRounds;
    //public int currentRound;
    public bool gameActive;

    public Color[,] colorPairs = { { new Color(255, 0, 0), new Color(255, 50, 0) }, { new Color(0, 0, 255), new Color(0, 150, 255) } }; //red, orange, blue, cyan
    public string level;



    //[SyncVar]
    //public int team1Score;
    //[SyncVar]
    //public int team2Score;


    [SyncVar]
    public bool matchStarted = false;
    //public bool useTitleScreen;

    [SyncVar]
    public bool countdownOver = false;

    [SyncVar]
    public float countdownTimer;


    private int numberOfPlayers;
    private Vector3[] spawnPoints;
    private int firstTeamLayer;
    private string outputPath;
    public int activePlayers;

    // Use this for initialization
    void Awake()
    {
        Debug.Log("GameManager is Awake. NetID: " + GetComponent<NetworkIdentity>().netId);
        //make singleton
        if (singleton)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;
        DontDestroyOnLoad(this);

        //setup logging
        outputPath = Application.dataPath + "/gamelog.txt";
        if (!File.Exists(outputPath)) File.Create(outputPath).Close();
        Debug.Log("Logging match results to: " + outputPath);

        activePlayers = 0;

        //SetGameMode(typeof(Deathmatch));

        //handle title screen skip in editor (currently unsupported until i get around to fixing it)
        //if (!useTitleScreen)
            //StartGame(SceneManager.GetActiveScene().name, maxRounds);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartClient()
    {
        Debug.Log("Spawned on client. NetID: " + GetComponent<NetworkIdentity>().netId);
    }

    [Server]
    public void KillTeam(GameObject player)
    {
        currentGame.KillTeam(player);
    }

    /// <summary>
    /// Sets the spawn points for the given team
    /// Calls SpawnTeam to spawn said team
    /// </summary>
    /// <param name="team">Team number</param>
    /// <param name="spawn1">Spawn point for player 1</param>
    /// <param name="spawn2">Spawn point for player 2</param>
    [Server]
    public void SetSpawn(int team, Vector3 spawn1, Vector3 spawn2)
    {
        spawnPoints[team * 2] = spawn1;
        spawnPoints[team * 2 + 1] = spawn2;
        SpawnTeam(team);
    }

    /// <summary>
    /// Spawns the given team object, passing it necessary info
    /// </summary>
    /// <param name="num">Which team to spawn/param>
    [Server]
    public void SpawnTeam(int num)
    {
        Debug.Log("Spawning team " + num);
        teams[num] = Instantiate(teamPlayerPrefab);
        teams[num].name = "Team " + num;
        teams[num].layer = firstTeamLayer + num;
        teams[num].GetComponent<Team>().SetSpawnPoints(spawnPoints[num * 2], spawnPoints[num * 2 + 1]);
        teams[num].GetComponent<Team>().SetColors(colorPairs[num, 0], colorPairs[num, 1]);
        NetworkServer.Spawn(teams[num]);
        NetManager.GetInstance().SpawnReadyPlayers(num);
    }

    [Server]
    public void SetNumberOfPlayers(int num)
    {
        numberOfPlayers = num;
        teams = new GameObject[num];
        spawnPoints = new Vector3[num * 2];
    }

    [Server]
    public void StartGame()
    {
        //NetworkServer.Spawn(gameObject);
        currentGame.BeginRound();
        NetManager.GetInstance().ServerChangeScene(level);
        NetworkServer.SendToAll(NetManager.ExtMsgType.StartGame, new NetManager.PingMessage());
        WriteToLog("Starting new game, level: " + level + " out of " + maxRounds + " rounds");
    }

    [Client]
    public void OnStartGame(NetworkMessage netMsg)
    {
        currentGame.BeginRound();
    }

    [Server]
    public GameObject SpawnPlayer(Player ply)
    {
        return teams[ply.playerId/2].GetComponent<Team>().SpawnPlayer(ply);
    }

    public void WriteToLog(string msg)
    {
        Debug.Log("Game Output: " + msg);
        StreamWriter sw = new StreamWriter(File.Open(outputPath, FileMode.Append));
        sw.WriteLine(msg);
        sw.Close();
    }

    // Set the current game mode to be played (Deathmatch is default)
    public void SetGameMode(System.Type mode)
    {
        Destroy(GetComponent<GameMode>());

        if (mode == typeof(Deathmatch))
            currentGame = gameObject.AddComponent<Deathmatch>();

        else if (mode == typeof(Soccer))
            currentGame = gameObject.AddComponent<Soccer>();
    }

    [ClientRpc]
    public void RpcResetClients()
    {
        NetManager.GetInstance().StopClient();
    }
}