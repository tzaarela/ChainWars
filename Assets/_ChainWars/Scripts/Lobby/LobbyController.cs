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
        localLobbyPlayer.isHost = true;

        lobby = await GameController.database.CreateLobbyAsync(createLobbyNameText.text);
        await lobby.AddPlayerAsync(localLobbyPlayer);
        lobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshedAsync;
        lobby.onGameStart += HandleOnGameStart;

		lobbyNameText.text = lobby.name;

        lobbiesReference.ValueChanged -= LobbyController_ValueChanged;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
        //RefreshLobbiesAsync();
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

            item.onLobbySelected -= HandleOnLobbySelected;
            Destroy(item.gameObject);
        }

        lobbyPanels.Clear();

        foreach (var lobby in lobbies)
        {
            if (lobby.isStarted == 1)
                continue;

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

	public async void JoinLobbyAsync()
	{
        var selectedLobbyPanel = lobbyPanels.FirstOrDefault(x => x.isSelected);

        if (selectedLobbyPanel == null)
		{
            Debug.Log("No lobby selected");
            return;
		}

        lobby = lobbies.FirstOrDefault(x => x.lobbyId == selectedLobbyPanel.lobbyId);

        if (lobby == null)
		{
            Debug.LogError("No related lobby found");
            return;
		}

        localLobbyPlayer.isHost = false;

        await lobby.AddPlayerAsync(localLobbyPlayer);
        lobby.onLobbyRoomRefreshed += HandleOnLobbyRoomRefreshedAsync;
        lobby.onHostLeft += HandleOnHostLeft;

        lobbiesReference.ValueChanged -= LobbyController_ValueChanged;

        lobbyNameText.text = lobby.name;
        lobbyBrowserWindow.SetActive(false);
        lobbyRoomWindow.SetActive(true);
	}

	private void HandleOnHostLeft()
	{
        lobby.onLobbyRoomRefreshed -= HandleOnLobbyRoomRefreshedAsync;
        lobby.onHostLeft -= HandleOnHostLeft;
        lobby = null;
        lobbyRoomWindow.SetActive(false);
        lobbyBrowserWindow.SetActive(true);
        RefreshLobbiesAsync();
    }

    public void LeaveLobby()
	{
        lobby.RemoveFromLobby(localLobbyPlayer);
        lobby.onLobbyRoomRefreshed -= HandleOnLobbyRoomRefreshedAsync;
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

	private async void HandleOnLobbyRoomRefreshedAsync()
	{
        var redTask = await lobbiesReference.Child(lobby.lobbyId).Child("redPlayers").GetValueAsync();
        var blueTask = await lobbiesReference.Child(lobby.lobbyId).Child("bluePlayers").GetValueAsync();


        if (redTask.GetRawJsonValue() != null)
        {
            var redPlayers = JsonConvert.DeserializeObject<Dictionary<string, LobbyPlayer>>(redTask.GetRawJsonValue());
            lobby.redPlayers = redPlayers;
        }
        else
            lobby.redPlayers.Clear();

        if (blueTask.GetRawJsonValue() != null)
        {
            var bluePlayers = JsonConvert.DeserializeObject<Dictionary<string, LobbyPlayer>>(blueTask.GetRawJsonValue());
            lobby.bluePlayers = bluePlayers;
        }
        else
            lobby.bluePlayers.Clear();


        CreateLobbyRoomObjects();
	}

	private void CreateLobbyRoomObjects()
	{
        
            bluePlayerObjects.ForEach(x => Destroy(x));
            bluePlayerObjects.Clear();
            redPlayerObjects.ForEach(x => Destroy(x));
            redPlayerObjects.Clear();

            if(lobby.bluePlayers.Count > 0)
			{
                foreach (LobbyPlayer player in lobby.bluePlayers.Values)
			    {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, blueVerticalPanel)
                        .GetComponent<LobbyPlayerObject>();

                    playerPanelObject.username.text = player.username;
                    bluePlayerObjects.Add(playerPanelObject.gameObject);
			    }
			}

            if(lobby.redPlayers.Count > 0)
			{
                foreach (LobbyPlayer player in lobby.redPlayers.Values)
                {
                    var playerPanelObject = Instantiate(lobbyPlayerPrefab, redVerticalPanel)
                        .GetComponent<LobbyPlayerObject>();

                    playerPanelObject.username.text = player.username;
                    redPlayerObjects.Add(playerPanelObject.gameObject);
                }
			}
	}

	public void OnApplicationQuit()
	{
        LeaveLobby();
	}

	public void JoinRedTeam()
	{
        lobby.JoinRedTeam(localLobbyPlayer);
	}

    public void JoinBlueTeam()
	{
        lobby.JoinBlueTeam(localLobbyPlayer);
	}

    public async void StartGameAsync()
	{
        //Move to onlobbyRefresh?
        //var lobbyJson = JsonConvert.SerializeObject(lobby);
        //await lobbiesReference.Child(lobby.lobbyId).SetRawJsonValueAsync(lobbyJson);

        lobby.StartGameAsync(localLobbyPlayer);
    }
}
