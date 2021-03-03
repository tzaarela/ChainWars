using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
	[Serializable]
	public class LobbyPlayer
	{
		public Guid playerId;
		public string username;
		public string email;
		public int wins;
		public int losses;
	}
}
