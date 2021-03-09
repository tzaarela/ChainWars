using Assets._ChainWars.Scripts.Enums;
using Assets.Scripts;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : NetworkBehaviour
{
	private void Start()
	{
		if (!isLocalPlayer)
			return;

		Initialize();
	}

	private void Update()
	{
		if (!isLocalPlayer)
			return;

		if (Input.GetKeyDown(KeyCode.Escape))
			SceneController.Load(SceneType.LobbyScene);
	}


	private void Initialize()
	{
		Debug.Log($"localPlayer | host: {GameController.localPlayer.isHost} | email: { GameController.localPlayer.email} |");
	}

}
