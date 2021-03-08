using Assets.Scripts.Models;
using Firebase.Database;
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

	private bool allClientsConnected;
	private int playerCount;
	private LobbyPlayer localPlayer;
	private Lobby localLobby;
	private Action onAllPlayersConnected;
	private Match match;
	private DatabaseReference lobbyReference;
	private DatabaseReference matchReference;

	public override void Awake()
	{
		base.Awake();

		if (debugMode)
			return;

		localPlayer = GameController.localPlayer;
		localLobby = GameController.lobby;
		lobbyReference = GameController.database.root.GetReference("lobbies").Child(localLobby.lobbyId);
		playerCount = localLobby.redPlayers.Count + localLobby.bluePlayers.Count;
	}

	public override async void Start()
	{
		base.Start();

		if(debugMode)
		{
			DebugStart();
			return;
		}

		GameController.database.root.GetReference("matches").ChildAdded += HandleOnMatchAdded;

		match = new Match(localLobby.redPlayers, localLobby.bluePlayers);

		if (localPlayer.isHost)
		{
			var json = JsonConvert.SerializeObject(match);
			var dbRef = GameController.database.root.GetReference("matches").Push();
			match.matchId = dbRef.Key;

			await dbRef.SetRawJsonValueAsync(json).ContinueWith(task =>
			{
				matchReference = GameController.database.root
				.GetReference("matches").Child(match.matchId);

				matchReference.Child("playersConnected").ValueChanged += HandlePlayersConnected;
				onAllPlayersConnected += HandleOnAllPlayersConnected;
				ConnectAndWaitForHostStart();

			});
		}
	}

	private void HandleOnMatchAdded(object sender, ChildChangedEventArgs e)
	{
		if(!localPlayer.isHost)
		{
			matchReference = GameController.database.root
				.GetReference("matches").Child(e.Snapshot.Key);

			match.matchId = e.Snapshot.Key;
			ConnectAndWaitForHostStart();
		}
	}

	private void HandlePlayersConnected(object sender, ValueChangedEventArgs e)
	{
		var playersConnected = JsonConvert.DeserializeObject<int>(e.Snapshot.GetRawJsonValue());
		Debug.Log(playersConnected + "/" + match.playerCount + "connected");
		if (match.playerCount == playersConnected)
			onAllPlayersConnected();
	}

	private void DebugStart()
	{
		Debug.Log("Debug Start!");
	}

	private void HandleOnAllPlayersConnected()
	{
		Debug.Log("All players connected");
		if (localPlayer.isHost)
			singleton.StartHost();
	}

	private void ConnectAndWaitForHostStart()
	{
		Debug.Log($"player {localPlayer.username} connected");

		if (!localPlayer.isHost)
			lobbyReference.Child("isHostStarted").ValueChanged += HandleOnClientHostStarted;

		GameController.database.root.GetReference("matches").Child(match.matchId)
			.Child("playersConnected").SetValueAsync(+1);
	}

	private void HandleOnClientHostStarted(object sender, Firebase.Database.ValueChangedEventArgs e)
	{
		if (JsonConvert.DeserializeObject<int>(e.Snapshot.GetRawJsonValue()) == 1)
		{
			lobbyReference.Child("hostSteamUserId").GetValueAsync().ContinueWithOnMainThread(task =>
			{
				var hostSteamUserId = JsonConvert.DeserializeObject<string>(task.Result.GetRawJsonValue());
				networkAddress = hostSteamUserId;
				StartClient();
			});
		}
	}

	

	public override void OnStartHost()
	{
		if (debugMode)
			return;

		Debug.Log("SteamUserId: " + Mirror.FizzySteam.FizzySteamworks.SteamUserID.ToString());

		GameController.database.root.GetReference("lobbies").Child(localLobby.lobbyId).Child("hostSteamUserId")
		.SetValueAsync(Mirror.FizzySteam.FizzySteamworks.SteamUserID.ToString()).ContinueWith(task => 
		{
			GameController.database.root.GetReference("lobbies")
			.Child(localLobby.lobbyId).Child("isHostStarted").SetValueAsync(1);
		});

		Debug.Log("Host server started");
	}

	
}