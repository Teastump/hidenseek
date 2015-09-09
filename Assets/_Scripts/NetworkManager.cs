using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class NetworkManager : Photon.MonoBehaviour {

	static public NetworkManager instance;

	public GameObject playerPrefab;
	public GameObject monsterPrefab;
	public Text newServerName;
	public JoinServerPanel serverPanel;
	public Lobby lobby;
	
	private bool playAsMonster = true;

	private RoomInfo[] roomsList;
	private GameObject selected;
	private int playersLoaded;
	private int playersSpawned;
	private float timeLimit;
	private string mapName;

	void Awake()
	{
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
		PhotonNetwork.player.name = "Treestump";
	}

	public void CreateRoom()
	{
		if (newServerName.text == null || newServerName.text == "") return;

		RoomOptions optn = null;

		if (optn == null)
		{
			optn = new RoomOptions ();
			optn.isOpen = true;
			optn.isVisible = true;
			optn.maxPlayers = 5;
			
			ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
			customProps["map"] = "hs_forest";
			optn.customRoomProperties = customProps;
			
			string[] exposedProps = new string[1];
			exposedProps[0] = "map";
			
			optn.customRoomPropertiesForLobby = exposedProps;
		}

		PhotonNetwork.CreateRoom (newServerName.text, optn, TypedLobby.Default);
		
		//PhotonNetwork.LoadLevel ("hs_test");
	}

	public void JoinRoom(string roomName, string mapName)
	{
		PhotonNetwork.JoinRoom (roomName);
		//PhotonNetwork.LoadLevel (mapName);
	}
	
	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom ();
	}
	
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
	
	void OnJoinedRoom()
	{
		Debug.Log("Connected to Room");
		
		/*
		if (playAsMonster)
			selected = monsterPrefab;
		else
			selected = playerPrefab;
		
		PhotonNetwork.Instantiate (selected.name, Vector3.up * 5, Quaternion.identity, 0);
		GameController.instance.GetPlayer ();
		//GameController.instance.SetFlashlights ();
		*/
		
		lobby.gameObject.SetActive (true);
		
		foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
			lobby.AddPlayer (player);
		}
	}
	
	void OnLeftRoom()
	{
		Debug.Log ("Left Room");
		
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
	
	public void ToggleMonster()
	{
		playAsMonster = !playAsMonster;
	}
	
	public void StartGame()
	{
		if (lobby.CheckReadyStatus ())
		{
			photonView.RPC ("RPCStart", PhotonTargets.All);	
		}
	}
	
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
	
	public void LevelDidLoad()
	{
		photonView.RPC ("PlayerLoaded", PhotonTargets.All);
	}
	
	[PunRPC]
	void PlayerLoaded()
	{
		++playersLoaded;
		
		Debug.Log (playersLoaded + " Players Loaded");
		
		if (playersLoaded == PhotonNetwork.room.playerCount)
		{
			GameController.instance.SpawnPlayer (selected);
			GameController.instance.StartTimer (timeLimit);
			PhotonNetwork.room.visible = false;
		}
	}
	
	public void PlayerSpawned()
	{
		photonView.RPC ("RPCPlayerSpawned", PhotonTargets.All);
	}
	
	[PunRPC]
	void RPCPlayerSpawned()
	{
		++playersSpawned;
		
		Debug.Log (playersSpawned + " Players Spawned");
		
		if (playersSpawned == playersLoaded)
		{
			GameController.instance.GetPlayerList ();
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
