using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
	[Serializable]
	public class Lobby
	{
		public Guid guid;
		public string name;
		public int playerCount = 0;
		public int playerMaxCount = 8;

		public LobbyPlayer hostPlayer;
		public List<LobbyPlayer> lobbyPlayers;
		public List<LobbyPlayer> redPlayers;
		public List<LobbyPlayer> bluePlayers;

		public Lobby(string name)
		{
			this.name = name;
			guid = Guid.NewGuid();

			lobbyPlayers = new List<LobbyPlayer>();
			redPlayers = new List<LobbyPlayer>();
			bluePlayers = new List<LobbyPlayer>();
		}
	}
}
