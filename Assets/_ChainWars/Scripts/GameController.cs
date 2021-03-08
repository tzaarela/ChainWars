using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Assets.Scripts;
using System;
using Assets.Scripts.Models;

public class GameController : MonoBehaviour
{
	public static Database database;
	public static Action onFirebaseInitialized;
	public static LobbyPlayer localPlayer;
	public static Lobby lobby;
	public static string localPlayerId;
	public static string gameLobbyId;

    private void Awake()
    {
		DontDestroyOnLoad(gameObject);
		database = new Database();
		//Invoke to let the rest init();
		Invoke("InitializeFirebase", 0.5f);
    }

	private void InitializeFirebase()
	{
		database.InitializeFirebase();
	}

	void OnApplicationQuit()
	{
		Debug.Log("Signing out user");
		database.auth.SignOut();
	}
}
