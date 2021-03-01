using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using System;

public class LoginController : MonoBehaviour
{
	public TextMeshProUGUI emailText;
	public TextMeshProUGUI passwordText;
	public TextMeshProUGUI statusText;

	private void Start()
	{
		GameController.database.onUserSignedIn += HandleOnUserSignedIn;
		GameController.database.onUserRegistered += HandleOnUserRegistered;
		GameController.database.onFirebaseInitialized += () => { statusText.text = "connected"; };
	}

	public void Login()
	{
		Debug.Log("Trying to login with mail: " + emailText.text);
		GameController.database.SignInUser(emailText.text, passwordText.text);
	}

	private void HandleOnUserSignedIn()
	{
		SceneController.Instance.ChangeScene(SceneType.LobbyScene);
	}

	public void Register()
	{
		Debug.Log("Trying to register user mail: " + emailText.text);
		GameController.database.RegisterUser(emailText.text, passwordText.text);
	}
	private void HandleOnUserRegistered()
	{
		statusText.text = "User registered, please sign in!";
	}
}
