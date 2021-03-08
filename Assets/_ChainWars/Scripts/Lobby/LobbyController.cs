using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.Models;
using Firebase.Database;
using System.Linq;
using Newtonsoft.Json;
using Assets.Scripts;
using Firebase.Extensions;
using Assets._ChainWars.Scripts.Enums;

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
    private Lobby lobby;
    private DatabaseReference lobbiesReference;

    public static LobbyController Instance;

    private void Awake()
	{
        if (Instance == null)
            Instance = this;
	}
    
    private void Start()
    {
        lobbyPanels = new List<LobbyPanel>();
        redPlayerObjects = new List<GameObject>();
        bluePlayerObjects = new List<GameObject>();

        var dbUser = GameController.database.user;

        Debug.Log("Setting logged in player text");
        GameController.database.root.GetReference("players")
        .Child(dbUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            localLobbyPlayer = JsonConvert.DeserializeObject<LobbyPlayer>(task.Result.GetRawJsonValue());

             if (!debugStart)
                signedInText.text = localLobbyPlayer.username;
        });

        lobbiesReference = GameController.database.root.GetReference("lobbies");
        lobbiesReference.ValueChanged += LobbyController_ValueChanged;
        //GameController.database.dbContext.GetReference("lobbies").ChildRemoved += HandleOnLobbyRemoved;

        RefreshLobbiesAsync();
    }

	public void UnsubscribeEvents()
	{
        lobbiesReference.ValueChanged -= LobbyController_ValueChanged;
    }

	private void LobbyController_ValueChanged(object sender, ValueChangedEventArgs e)
	{
        RefreshLobbiesAsync();
    }

	private void OnDestroy()
	{
        lobbiesReference.ValueChanged -= LobbyController_ValueChanged;
    }

	public async void CreateLobbyAsync()
	{
        lobby = await GameController.database.CreateLobbyAsync(createLobbyNameText.text);
        localLobbyPlayer.isHost = true;
        lobby.AddToLobby(localLobbyPlayer);
        lobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshed;
        lobby.onGameStart += HandleOnGameStart;
		lobbyNameText.text = lobby.name;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
        RefreshLobbiesAsync();
    }

	private void HandleOnGameStart()
	{
        lobbiesReference.ValueChanged -= LobbyController_ValueChanged;
    }

    public async void RefreshLobbiesAsync()
	{
        Debug.Log("Refreshing lobbies...");
        lobbies = await GameController.database.GetLobbyListAsync();
        CreateLobbyGameObjects();
    }

    private void CreateLobbyGameObjects()
	{
        foreach (var item in lobbyPanels)
        {
            if (item == null)
                continue;

            Destroy(item.gameObject);
        }

        lobbyPanels.Clear();

        foreach (var lobby in lobbies)
        {
            if (lobby.isStarted == 1)
                return;

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
            Debug.Log("No lobby selected");
            return;
		}

        var lobby = lobbies.FirstOrDefault(x => x.lobbyId == selectedLobbyPanel.lobbyId);

        if (lobby == null)
		{
            Debug.LogError("No related lobby found");
            return;
		}

        localLobbyPlayer.isHost = false;
        lobby.AddToLobby(localLobbyPlayer);
        this.lobby = lobby;

        this.lobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshed;
        this.lobby.onHostLeft += HandleOnHostLeft;
		lobbyNameText.text = this.lobby.name;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
	}

	private void HandleOnHostLeft()
	{
        lobby.onLobbyRoomRefreshed -= HandleOnLobbyRoomRefreshed;
        lobby.onHostLeft -= HandleOnHostLeft;
        lobby = null;
        lobbyRoomWindow.SetActive(false);
        lobbyBrowserWindow.SetActive(true);
        RefreshLobbiesAsync();
    }

    public void LeaveLobby()
	{
        lobby.RemoveFromLobby(localLobbyPlayer);
        lobby.onLobbyRoomRefreshed -= HandleOnLobbyRoomRefreshed;
        lobby = null;
        lobbyRoomWindow.SetActive(false);
        lobbyBrowserWindow.SetActive(true);
        RefreshLobbiesAsync();
	}

    public void LogOut()
	{
        GameController.database.LogOut();
        SceneController.Load(SceneType.LoginScene);
	}

	private void HandleOnLobbyRoomRefreshed()
	{
        CreateLobbyRoomObjects();
	}

	private void CreateLobbyRoomObjects()
	{
        lobbiesReference.Child(lobby.lobbyId.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError(task.Exception);

            var json = task.Result.GetRawJsonValue();
            var lobby = JsonConvert.DeserializeObject<Lobby>(json);
           
            bluePlayerObjects.ForEach(x => Destroy(x));
            bluePlayerObjects.Clear();
            redPlayerObjects.ForEach(x => Destroy(x));
            redPlayerObjects.Clear();

            if(lobby.bluePlayers != null)
			{
                foreach (LobbyPlayer player in lobby.bluePlayers.Values)
			    {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, blueVerticalPanel)
                        .GetComponent<LobbyPlayerObject>();

                    playerPanelObject.username.text = player.username;
                    bluePlayerObjects.Add(playerPanelObject.gameObject);
			    }
			}

            if(lobby.redPlayers != null)
			{
                foreach (LobbyPlayer player in lobby.redPlayers.Values)
                {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, redVerticalPanel)
                        .GetComponent<LobbyPlayerObject>();

                    playerPanelObject.username.text = player.username;
                    redPlayerObjects.Add(playerPanelObject.gameObject);
                }
			}
        });
	}

	public void JoinRedTeam()
	{
        lobby.JoinRedTeam(localLobbyPlayer);
	}

    public void JoinBlueTeam()
	{
        lobby.JoinBlueTeam(localLobbyPlayer);
	}

    public void StartGame()
	{
        lobby.StartGame(localLobbyPlayer);
	}
}
