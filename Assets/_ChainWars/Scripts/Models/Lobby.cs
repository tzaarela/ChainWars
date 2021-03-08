using Assets._ChainWars.Scripts.Enums;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Models
{
	[Serializable]
	public class Lobby
	{
		public string lobbyId;
		public int isStarted;
		public int isHostStarted;
		public string name;
		public int playerCount = 0;
		public int playerMaxCount = 8;
		
		public LobbyPlayer hostPlayer;
		public Dictionary<string, LobbyPlayer> lobbyPlayers;
		public Dictionary<string, LobbyPlayer> redPlayers;
		public Dictionary<string, LobbyPlayer> bluePlayers;

		public Action onLobbyRoomRefreshed;
		public Action onGameStart;
		public Action onHostLeft;


		private DatabaseReference lobbyReference;

		public Lobby(string name)
		{
			this.name = name;
			lobbyId = Guid.NewGuid().ToString();
		}

		private void RefreshLobby(object sender, ValueChangedEventArgs e)
		{
			Debug.Log("Refreshing lobby...");
			onLobbyRoomRefreshed?.Invoke();
		}

		public void AddToLobby(LobbyPlayer player)
		{
			lobbyPlayers = new Dictionary<string, LobbyPlayer>();
			redPlayers = new Dictionary<string, LobbyPlayer>();
			bluePlayers = new Dictionary<string, LobbyPlayer>();

			lobbyReference = GameController.database.root.GetReference("lobbies").Child(lobbyId.ToString());
			lobbyReference.Child("isStarted").SetValueAsync(0).ContinueWithOnMainThread(task => { 

				lobbyReference.Child("isStarted").ValueChanged += HandleOnGameStart;
			});

			lobbyReference.Child("redPlayers").ValueChanged += RefreshLobby;
			lobbyReference.Child("bluePlayers").ValueChanged += RefreshLobby;
			var playerRef = lobbyReference.Child("lobbyPlayers").Push();

			player.playerId = playerRef.Key;
			GameController.localPlayerId = player.playerId;
			GameController.localPlayer = player;
			GameController.lobby = this;

			lobbyPlayers.Add(player.playerId, player);

			var json = JsonConvert.SerializeObject(player);
			playerRef.SetRawJsonValueAsync(json);

		}

		public void RemoveFromLobby(LobbyPlayer player)
		{
			lobbyPlayers.Remove(player.playerId);
			LeaveBlueTeam(player);
			LeaveRedTeam(player);
			lobbyReference.Child("redPlayers").ValueChanged -= RefreshLobby;
			lobbyReference.Child("bluePlayers").ValueChanged -= RefreshLobby;
			lobbyReference.Child("isStarted").ValueChanged -= HandleOnGameStart;
			lobbyReference.Child("lobbyPlayers").Child(player.playerId).RemoveValueAsync();

			if (player.isHost)
			{
				HostLeftAsync();
			}
		}

		private async void HostLeftAsync()
		{
			await GameController.database.RemoveLobby(lobbyId);
			Debug.Log("Host left lobby: " + lobbyId);
			onHostLeft?.Invoke();
		}

		public void JoinBlueTeam(LobbyPlayer player)
		{
			if (IsAlreadyOnTeam(bluePlayers, player))
			{
				Debug.Log("Already on team");
				return;
			}

			LeaveRedTeam(player);
			bluePlayers.Add(player.playerId, player);
			playerCount++;

			var json = JsonConvert.SerializeObject(player);
			lobbyReference.Child("bluePlayers").Child(player.playerId).SetRawJsonValueAsync(json);
		}

		public void JoinRedTeam(LobbyPlayer player)
		{
			if (IsAlreadyOnTeam(redPlayers, player))
			{
				Debug.Log("Already on team");
				return;
			}

			LeaveBlueTeam(player);
			redPlayers.Add(player.playerId, player);
			playerCount++;

			var json = JsonConvert.SerializeObject(player);
			lobbyReference.Child("redPlayers").Child(player.playerId).SetRawJsonValueAsync(json);
		}

		public bool IsAlreadyOnTeam(Dictionary<string, LobbyPlayer> team, LobbyPlayer player)
		{
			if (team.ContainsKey(player.playerId))
				return true;

			return false;
		}

		public void LeaveBlueTeam(LobbyPlayer player)
		{
			if(bluePlayers.Remove(player.playerId))
			{
				playerCount--;
				lobbyReference.Child("bluePlayers").Child(player.playerId).RemoveValueAsync();
			}
		}

		public void LeaveRedTeam(LobbyPlayer player)
		{
			if(redPlayers.Remove(player.playerId))
			{
				playerCount--;
				lobbyReference.Child("redPlayers").Child(player.playerId).RemoveValueAsync();
			}
		}

		public void StartGame(LobbyPlayer player)
		{
			if(player.isHost)
			{
				lobbyReference.Child("isStarted").SetValueAsync(1);
			}
		}

		private void HandleOnGameStart(object sender, ValueChangedEventArgs e)
		{
			if (e.Snapshot.GetRawJsonValue() == null)
				return;

			var isStarted = JsonConvert.DeserializeObject<int>(e.Snapshot.GetRawJsonValue());

			if(isStarted == 1)
			{
				GameController.gameLobbyId = lobbyId;

				lobbyReference.Child("redPlayers").ValueChanged -= RefreshLobby;
				lobbyReference.Child("bluePlayers").ValueChanged -= RefreshLobby;
				lobbyReference.Child("isStarted").ValueChanged -= HandleOnGameStart;
				LobbyController.Instance.UnsubscribeEvents();
				SceneController.Load(SceneType.MatchScene);
			}
		}
	}
}
