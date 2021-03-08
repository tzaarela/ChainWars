using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class Match
{
	public string matchId;
	public int playerCount;
	public int playersConnected;
	public Dictionary<string, LobbyPlayer> redPlayers;
	public Dictionary<string, LobbyPlayer> bluePlayers;

	public int redKillCount;
	public int blueKillCount;

	public Match(Dictionary<string, LobbyPlayer> redPlayers, Dictionary<string, LobbyPlayer> bluePlayers)
	{
		this.redPlayers = redPlayers;
		this.bluePlayers = bluePlayers;
		playerCount = redPlayers.Count + bluePlayers.Count;
	}

}
