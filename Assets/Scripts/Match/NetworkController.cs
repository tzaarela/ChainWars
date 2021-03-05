using Assets.Scripts.Models;
using Firebase.Extensions;
using Mirror;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkController : NetworkManager
{
	private string localPlayerId;
	private string gameLobbyId;

	private LobbyPlayer localPlayer;
	private Action onLocalPlayerFetched;

	public NetworkController()
	{
	}

	public override void Awake()
	{
		base.Awake();
		Debug.Log("Awake");
		gameLobbyId = GameController.gameLobbyId;
		localPlayerId = GameController.localPlayerId;
		//GameController.onFirebaseInitialized += Initialize;
	}

	public override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		GameController.database.dbContext.GetReference("lobbies")
			.Child(gameLobbyId).Child("isHostStarted").SetValueAsync(0);

		onLocalPlayerFetched += HandleOnLocalPlayerFetched;
		GetLocalPlayer();

	}

	private void HandleOnLocalPlayerFetched()
	{
		Debug.Log("localPlayer fetched");
		if (localPlayer.isHost)
			StartServerHost();
		else
			WaitForServerStart();

	}

	private void StartServerHost()
	{
		Debug.Log("Trying to start server...");
		StartCoroutine(StartServerCoroutine());
	}

	IEnumerator StartServerCoroutine()
	{
		//This needs a change later....
		yield return new WaitForSeconds(2f);
		singleton.StartHost();
	}

	private void WaitForServerStart()
	{
	
		Debug.Log("Waiting for server to start...");
		GameController.database.dbContext.GetReference("lobbies")
			.Child(gameLobbyId).Child("isHostStarted").ValueChanged += HandleOnClientHostStarted;
	}

	private void HandleOnClientHostStarted(object sender, Firebase.Database.ValueChangedEventArgs e)
	{
		if (JsonConvert.DeserializeObject<int>(e.Snapshot.GetRawJsonValue()) == 1)
		{
			networkAddress = "localhost";
			StartClient();
		}
	}

	private void GetLocalPlayer()
	{
		Debug.Log("getting local player...");
		GameController.database.dbContext.GetReference("lobbies")
		.Child(gameLobbyId).Child("lobbyPlayers").Child(localPlayerId).GetValueAsync()
		.ContinueWithOnMainThread(task => 
		{
		
			if (task.IsCanceled)
				Debug.Log("get localplayer canceled");

			if (task.IsFaulted)
				Debug.LogError(task.Exception);

			var json = task.Result.GetRawJsonValue();
			localPlayer = JsonConvert.DeserializeObject<LobbyPlayer>(json);
			onLocalPlayerFetched();
		});
	}


	public override void OnStartHost()
	{
		//if (localPlayer.isHost)
		//{
			GameController.database.dbContext.GetReference("lobbies")
			.Child(gameLobbyId).Child("isHostStarted").SetValueAsync(1);

			Debug.Log("Host server started");
		//}
	}
}