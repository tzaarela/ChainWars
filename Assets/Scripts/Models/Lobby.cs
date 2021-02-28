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
		public int playerCount = 1;

		public Lobby(string name)
		{
			this.name = name;
			guid = Guid.NewGuid();
		}
	}
}
