using Firebase.Database;
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
		public string name;
		public int playerCount = 0;
		public int playerMaxCount = 8;

		public LobbyPlayer hostPlayer;
		public Dictionary<string, LobbyPlayer> lobbyPlayers;
		public Dictionary<string, LobbyPlayer> redPlayers;
		public Dictionary<string, LobbyPlayer> bluePlayers;

		public Action onLobbyRoomRefreshed;

		public Lobby(string name)
		{
			this.name = name;
			lobbyId = Guid.NewGuid().ToString();
		}

		private void RefreshLobby(object sender, ChildChangedEventArgs e)
		{
			Debug.Log("Refreshing lobby...");
			onLobbyRoomRefreshed?.Invoke();
		}

		//public void UpdateLobbyInDb()
		//{

		//	var json = JsonUtility.ToJson(this);

		//	GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
		//	{
		//		if (task.IsFaulted)
		//			Debug.LogError(task.Exception);

		//		Debug.Log("Updated lobby successfully");
		//	});
		//}

		public void AddToLobby(LobbyPlayer player, bool isHost)
		{

			lobbyPlayers = new Dictionary<string, LobbyPlayer>();
			redPlayers = new Dictionary<string, LobbyPlayer>();
			bluePlayers = new Dictionary<string, LobbyPlayer>();

			if (isHost)
				hostPlayer = player;

			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildChanged += RefreshLobby;
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildAdded += RefreshLobby;
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildRemoved += RefreshLobby;


			var playerRef = GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("lobbyPlayers").Push();

			player.playerId = playerRef.Key;

			lobbyPlayers.Add(player.playerId, player);

			var json = JsonConvert.SerializeObject(player);
			playerRef.SetRawJsonValueAsync(json);

		}

		public void RemoveFromLobby(LobbyPlayer player)
		{
			lobbyPlayers.Remove(player.playerId);
			LeaveBlueTeam(player);
			LeaveRedTeam(player);

			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("lobbyPlayers").Child(player.playerId).RemoveValueAsync();
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
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("bluePlayers").Child(player.playerId).SetRawJsonValueAsync(json);
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
			var playerRef = GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("redPlayers").Child(player.playerId).SetRawJsonValueAsync(json);
		}

		public bool IsAlreadyOnTeam(Dictionary<string, LobbyPlayer> team, LobbyPlayer player)
		{
			if (team.ContainsKey(player.playerId))
				return true;

			return false;
		}

		public void LeaveBlueTeam(LobbyPlayer player)
		{
			playerCount--;
			bluePlayers.Remove(player.playerId);
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("bluePlayers").Child(player.playerId).RemoveValueAsync();
		}

		public void LeaveRedTeam(LobbyPlayer player)
		{
			playerCount--;
			redPlayers.Remove(player.playerId);
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString())
				.Child("redPlayers").Child(player.playerId).RemoveValueAsync();
		}
	}
}
