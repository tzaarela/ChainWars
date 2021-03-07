using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			SceneController.Instance.ChangeScene(SceneType.LobbyScene);
	}

	private void Initialize()
	{
		
	}

}
