using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._ChainWars.Scripts.Player
{
	public class Player
	{
		private float health;
		private float runSpeed;
		private float hookDamage;
		private float hookSpeed;
		private float hookLength;
		private float meleeDamage;

		public float MeleeDamage { get => meleeDamage; set => meleeDamage = value; }
		public float HookDamage { get => hookDamage; set => hookDamage = value; }
		public float HookSpeed { get => hookSpeed; set => hookSpeed = value; }
		public float HookLength { get => hookLength; set => hookLength = value; }
		public float RunSpeed { get => runSpeed; set => runSpeed = value; }
		public float Health { get => health; set => health = value; }
	}
}
