using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Assets.Scripts;
using System;

public class GameController : MonoBehaviour
{
	public static Database database;

	public static Action onFirebaseInitialized;

    private void Awake()
    {
		DontDestroyOnLoad(gameObject);
    }

	private void Start()
	{
		database = new Database();
		//Invoke to let the rest init();
		Invoke("InitializeFirebase", 0.5f);
	}

	private void InitializeFirebase()
	{
		database.InitializeFirebase();
	}
}
