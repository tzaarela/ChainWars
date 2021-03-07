using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using System;

public class LoginController : MonoBehaviour
{
	[Header("Login")]
	[SerializeField] private TMP_InputField emailInput;
	[SerializeField] private TMP_InputField passwordInput;
	[SerializeField] private TextMeshProUGUI statusText;

	[Header("Register")]
	[SerializeField] private TMP_InputField emailRegisterInput;
	[SerializeField] private TMP_InputField usernameRegisterInput;
	[SerializeField] private TMP_InputField passwordRegisterInput;
	[SerializeField] private TMP_InputField passwordVerifyRegisterInput;

	[Header("Windows")]
	[SerializeField] private GameObject loginWindow;
	[SerializeField] private GameObject registerWindow;

	private void Start()
	{
		GameController.database.onUserSignedIn += HandleOnUserSignedIn;
		GameController.database.onUserRegistered += HandleOnUserRegistered;
		GameController.database.onFirebaseInitialized += () => { statusText.text = "connected"; };
	}

	public void Login()
	{
		Debug.Log("Trying to login with mail: " + emailInput.text);
		GameController.database.SignInUser(emailInput.text, passwordInput.text);
	}

	public void DebugLogin()
	{
		emailInput.text = "test1@test.com";
		passwordInput.text = "123456";
	}

	public void DebugLogin2()
	{
		emailInput.text = "test2@test.com";
		passwordInput.text = "123456";
	}

	private void HandleOnUserSignedIn()
	{
		SceneController.Instance.ChangeScene(SceneType.LobbyScene);
	}

	public void ActivateRegisterWindow()
	{
		registerWindow.SetActive(true);
		loginWindow.SetActive(false);
	}

	public void ActivateLoginWindow()
	{
		loginWindow.SetActive(true);
		registerWindow.SetActive(false);
	}

	public void Register()
	{
		Debug.Log("Trying to register user mail: " + emailRegisterInput.text);
		GameController.database.RegisterUser(
			usernameRegisterInput.text, 
			emailRegisterInput.text, 
			passwordRegisterInput.text, 
			passwordVerifyRegisterInput.text);
	}
	private void HandleOnUserRegistered()
	{
		SceneController.Instance.ChangeScene(SceneType.LobbyScene);
		statusText.text = "User registered, please sign in!";
	}
}
