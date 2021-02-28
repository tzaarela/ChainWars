using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Assets.Scripts;

public class GameController : MonoBehaviour
{
	public static Database database;

    private void Awake()
    {
		database = new Database();
		database.InitializeFirebase();
		DontDestroyOnLoad(gameObject);
    }
}
