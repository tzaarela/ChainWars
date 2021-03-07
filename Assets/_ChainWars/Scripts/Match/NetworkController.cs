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
	[SerializeField] bool debugMode;

	private string localPlayerId;
	private string gameLobbyId;

	private LobbyPlayer localPlayer;
	private Action onLocalPlayerFetched;

	public override void Awake()
	{
		base.Awake();

		if (debugMode)
			return;

		gameLobbyId = GameController.gameLobbyId;
		localPlayerId = GameController.localPlayerId;
	}

	public override void Start()
	{
		base.Start();

		if(debugMode)
		{
			DebugStart();
			return;
		}

		Initialize();
	}

	private void DebugStart()
	{
		Debug.Log("Debug Start!");
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
	public override void OnStartHost()
	{
		if (debugMode)
			return;

		Debug.Log("SteamUserId: " + Mirror.FizzySteam.FizzySteamworks.SteamUserID.ToString());

		GameController.database.dbContext.GetReference("lobbies").Child(gameLobbyId).Child("hostSteamUserId")
		.SetValueAsync(Mirror.FizzySteam.FizzySteamworks.SteamUserID.ToString()).ContinueWith(task => 
		{
			GameController.database.dbContext.GetReference("lobbies")
			.Child(gameLobbyId).Child("isHostStarted").SetValueAsync(1);
		});

		Debug.Log("Host server started");
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
			GameController.database.dbContext.GetReference("lobbies")
			.Child(gameLobbyId).Child("hostSteamUserId").GetValueAsync().ContinueWithOnMainThread(task =>
			{
				var hostSteamUserId = JsonConvert.DeserializeObject<string>(task.Result.GetRawJsonValue());
				networkAddress = hostSteamUserId;
				StartClient();
			});
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

	
}