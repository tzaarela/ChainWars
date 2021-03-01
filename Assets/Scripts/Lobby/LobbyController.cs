using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using Assets.Scripts.Models;
using System;
using Firebase.Database;
using DG.Tweening;

public class LobbyController : MonoBehaviour
{
    public TextMeshProUGUI signedInText;
    public TextMeshProUGUI createLobbyNameText;
    public Transform lobbiesVerticalGroup;
    public GameObject lobbyPrefab;

    public bool debugStart;

    private List<GameObject> lobbyGameObjects;
    private List<Lobby> lobbies;
    private LobbyPlayer localLobbyPlayer;

    void Start()
    {
        lobbyGameObjects = new List<GameObject>();

        if (!debugStart)
        signedInText.text = GameController.database.user.Email;

        var dbUser = GameController.database.user;
        localLobbyPlayer = new LobbyPlayer();
        localLobbyPlayer.email = dbUser.Email;

        GameController.database.dbContext.GetReference("Lobbies").ChildAdded += HandleOnLobbyCreated;
        GameController.database.dbContext.GetReference("Lobbies").ChildRemoved += HandleOnLobbyRemoved;

        RefreshLobbies();
    }

	private void HandleOnLobbyRemoved(object sender, ChildChangedEventArgs e)
	{
        RefreshLobbies();
    }

    private void HandleOnLobbyCreated(object sender, ChildChangedEventArgs e)
	{
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
        lobbies = GameController.database.RefreshLobbies();
    }

    private void HandleOnLobbiesRefreshed(List<Lobby> lobbies)
	{
        Dispatcher.RunOnMainThread(() => 
        {
            CreateLobbiesGameObjects();
        });
	}

    private void CreateLobbiesGameObjects()
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
    }

	public void JoinLobby()
	{

	}
}
