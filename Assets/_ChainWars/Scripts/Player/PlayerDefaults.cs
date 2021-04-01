using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._ChainWars.Scripts.Player
{
	[CreateAssetMenu(fileName = "Player", menuName = "Player")]
	public class PlayerDefaults : ScriptableObject
	{
		[Header("Player")]
		[SerializeField] private float health;
		[SerializeField] private float runSpeed;
		[Header("Hook")]
		[SerializeField] private float hookDamage;
		[SerializeField] private float hookSpeed;
		[SerializeField] private float hookLength;
		[Header("Melee")]
		[SerializeField] private float meleeDamage;

		public float MeleeDamage { get => meleeDamage; set => meleeDamage = value; }
		public float HookDamage { get => hookDamage; set => hookDamage = value; }
		public float HookSpeed { get => hookSpeed; set => hookSpeed = value; }
		public float HookLength { get => hookLength; set => hookLength = value; }
		public float RunSpeed { get => runSpeed; set => runSpeed = value; }
		public float Health { get => health; set => health = value; }
	}
}
