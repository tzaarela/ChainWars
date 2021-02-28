using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using Assets.Scripts.Models;
using System;

public class LobbyController : MonoBehaviour
{
    public TextMeshProUGUI signedInText;
    public TextMeshProUGUI createLobbyNameText;
    public Transform lobbiesVerticalGroup;
    public GameObject lobbyPrefab;

    public bool debugStart;

    private List<GameObject> lobbyGameObjects;

    void Start()
    {
        lobbyGameObjects = new List<GameObject>();

        if (!debugStart)
        signedInText.text = GameController.database.user.Email;

        RefreshLobbies();
    }

    public void CreateLobby()
	{
        GameController.database.onLobbyCreated += RefreshLobbies;
        GameController.database.CreateLobby(createLobbyNameText.text);
	}
    public void RefreshLobbies()
	{
        GameController.database.onLobbiesRefreshed += HandleOnLobbiesRefreshed;
        GameController.database.RefreshLobbies();
	}

	private void HandleOnLobbiesRefreshed(List<Lobby> lobbies)
	{
        Dispatcher.RunOnMainThread(() => 
        {
		    foreach (var item in lobbyGameObjects)
		    {
                Destroy(item);
		    }

		    foreach (var lobby in lobbies)
		    {
                var lobbyGameObject = Instantiate(lobbyPrefab, lobbiesVerticalGroup);
                lobbyGameObject.GetComponent<LobbyPanel>().lobbyName.text = lobby.name;
                lobbyGameObjects.Add(lobbyGameObject);
		    }
        });
	}

	public void JoinLobby()
	{

	}
}
