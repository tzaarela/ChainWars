using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using Assets.Scripts.Models;
using System;
using Firebase.Database;
using DG.Tweening;
using System.Linq;

public class LobbyController : MonoBehaviour
{
    [Header("Lobby")]
    [SerializeField] private TextMeshProUGUI signedInText;
    [SerializeField] private TextMeshProUGUI createLobbyNameText;
    [SerializeField] private Transform lobbiesVerticalGroup;
    [SerializeField] private GameObject lobbyPanelPrefab;

    [Header("LobbyRoom")]
    [SerializeField] private Transform redVerticalPanel;
    [SerializeField] private Transform blueVerticalPanel;
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    [Header("Windows")]
    [SerializeField] private GameObject lobbyBrowserWindow;
    [SerializeField] private GameObject lobbyRoomWindow;

    [SerializeField] bool debugStart;

    private List<LobbyPanel> lobbyPanels;
    private List<Lobby> lobbies;
    private List<GameObject> redPlayerObjects;
    private List<GameObject> bluePlayerObjects;

    private LobbyPlayer localLobbyPlayer;
    private Lobby joinedLobby;

    void Start()
    {
        lobbyPanels = new List<LobbyPanel>();
        redPlayerObjects = new List<GameObject>();
        bluePlayerObjects = new List<GameObject>();


        var dbUser = GameController.database.user;

        GameController.database.dbContext.GetReference("players").Child(dbUser.UserId).GetValueAsync().ContinueWith(task =>
        {
            localLobbyPlayer = JsonUtility.FromJson<LobbyPlayer>(task.Result.GetRawJsonValue());

            Dispatcher.RunOnMainThread(() => 
            {
                if (!debugStart)
                signedInText.text = localLobbyPlayer.username;
            });
        });


        GameController.database.dbContext.GetReference("lobbies").ChildAdded += HandleOnLobbyCreated;
        GameController.database.dbContext.GetReference("lobbies").ChildRemoved += HandleOnLobbyRemoved;

        RefreshLobbies();
    }

	private void HandleOnLobbyRemoved(object sender, ChildChangedEventArgs e)
	{
        RefreshLobbies();
    }

    private void HandleOnLobbyCreated(object sender, ChildChangedEventArgs e)
	{
        Debug.Log("lobby created");
        RefreshLobbies();
	}

	public void CreateLobby()
	{
        GameController.database.onLobbyCreated += RefreshLobbies;
        var lobby = GameController.database.CreateLobby(createLobbyNameText.text);
        lobby.AddToLobby(localLobbyPlayer, true);
        joinedLobby = lobby;
        joinedLobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshed;
        lobbyNameText.text = joinedLobby.name;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
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
        foreach (var item in lobbyPanels)
        {
            Destroy(item.gameObject);
        }

        lobbyPanels.Clear();

        foreach (var lobby in lobbies)
        {
            var lobbyPanel = Instantiate(lobbyPanelPrefab, lobbiesVerticalGroup).GetComponent<LobbyPanel>();
            lobbyPanel.lobbyName.text = lobby.name;
            lobbyPanel.lobbyId = lobby.lobbyId;
            lobbyPanel.onLobbySelected += HandleOnLobbySelected;
            lobbyPanels.Add(lobbyPanel);
        }
    }

	private void HandleOnLobbySelected(LobbyPanel selectedLobbyPanel)
	{
		foreach (var lobbyPanel in lobbyPanels)
		{
            lobbyPanel.DeselectPanel();
		}
        selectedLobbyPanel.SelectPanel();
	}

	public void JoinLobby()
	{
        var selectedLobbyPanel = lobbyPanels.FirstOrDefault(x => x.isSelected);

        if (selectedLobbyPanel == null)
		{
            Debug.LogError("No lobby selected");
            return;
		}

        var lobby = lobbies.FirstOrDefault(x => x.lobbyId == selectedLobbyPanel.lobbyId);

        if (lobby == null)
		{
            Debug.LogError("No related lobby found");
            return;
		}

        lobby.AddToLobby(localLobbyPlayer, false);
        joinedLobby = lobby;
        joinedLobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshed;
        lobbyNameText.text = joinedLobby.name;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
	}

	public void LeaveLobby()
	{
        joinedLobby.RemoveFromLobby(localLobbyPlayer);
        joinedLobby.onLobbyRoomRefreshed -= HandleOnLobbyRoomRefreshed;
        joinedLobby = null;
        lobbyRoomWindow.SetActive(false);
        lobbyBrowserWindow.SetActive(true);
	}
	private void HandleOnLobbyRoomRefreshed()
	{
        CreateLobbyRoomObjects();
	}

	private void CreateLobbyRoomObjects()
	{
        Debug.Log("Creating lobby room objects");
        GameController.database.dbContext.GetReference("lobbies").Child(joinedLobby.lobbyId.ToString()).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError(task.Exception);

            var json = task.Result.GetRawJsonValue();
            var lobby = JsonUtility.FromJson<Lobby>(json);

            Dispatcher.RunOnMainThread(() => 
            { 
                bluePlayerObjects.ForEach(x => Destroy(x));
                bluePlayerObjects.Clear();
                redPlayerObjects.ForEach(x => Destroy(x));
                redPlayerObjects.Clear();

                foreach (var player in lobby.bluePlayers)
			    {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, blueVerticalPanel).GetComponent<LobbyPlayerObject>();
                    playerPanelObject.username.text = player.username;
                    bluePlayerObjects.Add(playerPanelObject.gameObject);
			    }

                foreach (var player in lobby.redPlayers)
                {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, redVerticalPanel).GetComponent<LobbyPlayerObject>();
                    playerPanelObject.username.text = player.username;
                    redPlayerObjects.Add(playerPanelObject.gameObject);
                }
            });
        });
	}

	public void JoinRedTeam()
	{
        joinedLobby.JoinRedTeam(localLobbyPlayer);
	}

    public void JoinBlueTeam()
	{
        joinedLobby.JoinBlueTeam(localLobbyPlayer);
	}

    public void StartGame()
	{

	}
}
