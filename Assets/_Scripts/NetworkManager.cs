using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

/*Class used to handle connecting to photon network and syncing clients for game start*/
public class NetworkManager : Photon.MonoBehaviour {

	static public NetworkManager instance; //Global instance

	public GameObject playerPrefab;
	public GameObject monsterPrefab;
	public Text playerName;
	public Text newServerName;
	public JoinServerPanel serverPanel;
	public Lobby lobby;
	
	public Button enterName;
	public GameObject menuButtons;
	
	private bool playAsMonster = true;

	private RoomInfo[] roomsList;
	private GameObject selected;
	private int playersLoaded;
	private int playersSpawned;
	private float timeLimit;
	private string mapName;

	void Awake()
	{
		//Creates singleton structure
		if (instance == null) 
		{
			instance = this;
			DontDestroyOnLoad (this);
		} 
		else 
		{
			if (this != instance)
				Destroy (this.gameObject);
		}
		
		playersLoaded = 0;
	}

	// Use this for initialization
	void Start () 
	{
		PhotonNetwork.ConnectUsingSettings ("0.1");
		
		//If we are reloading from a map
		if (PhotonNetwork.player.name != "")
			ToggleEnterPlayerName();
	}
	
	//Changes player name
	public void ChangePlayerName()
	{
		PhotonNetwork.player.name = playerName.text;
		ToggleEnterPlayerName();
	}
	
	//Toggles between the Enter Player Name field and the main buttons
	void ToggleEnterPlayerName()
	{
		enterName.gameObject.SetActive(!enterName.gameObject.activeSelf);
		menuButtons.SetActive (!menuButtons.activeSelf);
	}
	
	//Quits the application
	public void Exit()
	{
		Application.Quit ();
	}

	//Creates a new room
	public void CreateRoom()
	{
		if (newServerName.text == null || newServerName.text == "") return; //If there's no text entered

		
		RoomOptions optn = new RoomOptions ();
		optn.isOpen = true;
		optn.isVisible = true;
		optn.maxPlayers = 5;
			
		ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
		customProps["map"] = "hs_forest";
		optn.customRoomProperties = customProps;
			
		string[] exposedProps = new string[1];
		exposedProps[0] = "map";
			
		optn.customRoomPropertiesForLobby = exposedProps;
		

		PhotonNetwork.CreateRoom (newServerName.text, optn, TypedLobby.Default); //Creates the room
		
		//PhotonNetwork.LoadLevel ("hs_test");
	}

	public void JoinRoom(string roomName)
	{
		PhotonNetwork.JoinRoom (roomName);
	}
	
	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom ();
	}
	
	//Refreshes room list whenever a new one appears
	void OnReceivedRoomListUpdate()
	{
		roomsList = PhotonNetwork.GetRoomList();

		if (serverPanel != null) 
		{
			serverPanel.RefreshRoomList (roomsList);
		}

		foreach (RoomInfo room in roomsList) 
		{
			Debug.Log (room.name);
		}
	}
	
	//Called when client joins a room
	void OnJoinedRoom()
	{
		Debug.Log("Connected to Room");
		
		lobby.gameObject.SetActive (true);
		
		//Adds all players to the lobby list
		foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
			lobby.AddPlayer (player);
		}
	}
	
	//Called when client leaves room
	void OnLeftRoom()
	{
		Debug.Log ("Left Room");
		
		//Clears the lobby if still in the main menu
		if (lobby != null)
		{
			lobby.gameObject.SetActive (false);
			lobby.Clear ();
		}
	}
	
	void OnPhotonPlayerConnected(PhotonPlayer other)
	{
		lobby.AddPlayer (other);
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer other)
	{
		lobby.RemovePlayer (other);
	}
	
	//Toggles if the player is playing as a monster
	public void ToggleMonster()
	{
		playAsMonster = !playAsMonster;
	}
	
	//Starts the game if all players are ready
	public void StartGame()
	{
		if (lobby.CheckReadyStatus ())
		{
			photonView.RPC ("RPCStart", PhotonTargets.All);	
		}
	}
	
	//Called in a map once someone wins
	public void RestartGame()
	{
		photonView.RPC ("RPCStart", PhotonTargets.All);
	}
	
	[PunRPC]
	private void RPCStart()
	{
		if (lobby != null)
		{
			timeLimit = lobby.timeLimit;
			mapName = lobby.mapName;
		}
		playersLoaded = 0;
		playersSpawned = 0;
		PhotonNetwork.LoadLevel (mapName);
		
		if (playAsMonster)
			selected = monsterPrefab;
		else
			selected = playerPrefab;
	}
	
	//Called by GameController once it has loaded into a level
	public void LevelDidLoad()
	{
		photonView.RPC ("PlayerLoaded", PhotonTargets.All);
	}
	
	[PunRPC]
	void PlayerLoaded()
	{
		++playersLoaded;
		
		Debug.Log (playersLoaded + " Players Loaded");
		
		//Once all players have loaded, spawn them and start the game timer
		if (playersLoaded == PhotonNetwork.room.playerCount)
		{
			GameController.instance.SpawnPlayer (selected);
			GameController.instance.StartTimer (timeLimit);
			PhotonNetwork.room.visible = false;
		}
	}
	
	//Called after a player is spawned
	public void PlayerSpawned()
	{
		photonView.RPC ("RPCPlayerSpawned", PhotonTargets.All);
	}
	
	[PunRPC]
	void RPCPlayerSpawned()
	{
		++playersSpawned;
		
		Debug.Log (playersSpawned + " Players Spawned");
		
		//Once all players have been spawned into the level
		if (playersSpawned == playersLoaded)
		{
			GameController.instance.GetPlayerList ();
		}
	}
}
