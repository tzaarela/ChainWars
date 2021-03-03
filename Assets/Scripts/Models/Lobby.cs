using Firebase.Database;
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
		public List<LobbyPlayer> lobbyPlayers;
		public List<LobbyPlayer> redPlayers;
		public List<LobbyPlayer> bluePlayers;

		public Action onLobbyRoomRefreshed;

		public Lobby(string name)
		{
			this.name = name;
			lobbyId = Guid.NewGuid().ToString();

			lobbyPlayers = new List<LobbyPlayer>();
			redPlayers = new List<LobbyPlayer>();
			bluePlayers = new List<LobbyPlayer>();
		}

		private void RefreshLobby(object sender, ChildChangedEventArgs e)
		{
			Debug.Log("Refreshing lobby...");
			onLobbyRoomRefreshed?.Invoke();
		}

		public void UpdateLobbyInDb()
		{
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildChanged += RefreshLobby;
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildAdded += RefreshLobby;
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).ChildRemoved += RefreshLobby;

			var json = JsonUtility.ToJson(this);
			GameController.database.dbContext.GetReference("lobbies").Child(lobbyId.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
			{
				if (task.IsFaulted)
					Debug.LogError(task.Exception);

				Debug.Log("Updated lobby successfully");
			});
		}

		public void AddToLobby(LobbyPlayer player, bool isHost)
		{
			if (isHost)
				hostPlayer = player;

			lobbyPlayers.Add(player);
			UpdateLobbyInDb();
		}

		public void RemoveFromLobby(LobbyPlayer player)
		{
			lobbyPlayers.RemoveAll(x => x.playerId == player.playerId);
			LeaveBlueTeam(player);
			LeaveRedTeam(player);
			UpdateLobbyInDb();
		}

		public void JoinBlueTeam(LobbyPlayer player)
		{
			LeaveRedTeam(player);
			bluePlayers.Add(player);
			playerCount++;
			UpdateLobbyInDb();
		}

		public void JoinRedTeam(LobbyPlayer player)
		{
			LeaveBlueTeam(player);
			redPlayers.Add(player);
			playerCount++;
			UpdateLobbyInDb();
		}

		public void LeaveBlueTeam(LobbyPlayer player)
		{
			playerCount--;
			bluePlayers.RemoveAll(x => x.playerId == player.playerId);
		}

		public void LeaveRedTeam(LobbyPlayer player)
		{
			playerCount--;
			redPlayers.RemoveAll(x => x.playerId == player.playerId);
		}
	}
}
