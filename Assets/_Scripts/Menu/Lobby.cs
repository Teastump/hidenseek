using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*Class to handle lobby sync between connected players*/
public class Lobby : Photon.MonoBehaviour {

	public PlayerCard[] playerCards;
	
	[SerializeField] Button mapSelect;
	[SerializeField] InputField timeLimitField;
	[SerializeField] Button startButton;
	
	public string mapName
	{
		get { return _mapName; }
	}
	private string _mapName = "hs_forest";
	
	public int timeLimit
	{
		get { return _timeLimit; }
		private set { photonView.RPC ("RPCSetTimer", PhotonTargets.AllBuffered, value); }
	}
	private int _timeLimit;
	
	private ExitGames.Client.Photon.Hashtable customProps;
	
	void Awake()
	{
		//Only master client can start game and change timer
		if (!PhotonNetwork.isMasterClient) 
		{
			mapSelect.interactable = false;
			timeLimitField.interactable = false;
			startButton.interactable = false;
		}
		
		_timeLimit = 5;
		
		customProps = new ExitGames.Client.Photon.Hashtable();
		customProps["map"] = mapName;
		
		PhotonNetwork.room.SetCustomProperties(customProps);
	}
	
	//Adds a player to the lobby
	public bool AddPlayer(PhotonPlayer player)
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			if (playerCard.player == null)
			{
				playerCard.player = player;
				return true;
			}
		}
		
		return false; //No open slot available for some reason
	}
	
	//Removes a player from the lobby
	public bool RemovePlayer(PhotonPlayer player)
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			if (playerCard.player == player)
			{
				playerCard.player = null;
				return true;
			}
		}
		
		return false; //Player not found in lobby
	}
	
	//Clears the players from the lobby
	public void Clear()
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			playerCard.player = null;
		}
	}
	
	public void ChooseMap(string map)
	{
		customProps["map"] = mapName;
		
		PhotonNetwork.room.SetCustomProperties(customProps);
		
		photonView.RPC ("RPCChooseMap", PhotonTargets.AllBuffered, map);
	}
	
	[PunRPC]
	void RPCChooseMap(string map)
	{
		_mapName = map;
		
		mapSelect.GetComponentInChildren<Text>().text = "Map: " + map;
	}
	
	//Checks if all players are ready
	public bool CheckReadyStatus()
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			if (playerCard.player != null && !playerCard.ready)
				return false;
		}
		
		return true;
	}
	
	public void SetReadyForPlayer(PhotonPlayer player, bool status)
	{
		photonView.RPC ("RPCReady", PhotonTargets.AllBufferedViaServer, player, status);
	}
	
	[PunRPC]
	private void RPCReady(PhotonPlayer player, bool status)
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			if (playerCard.player == player)
			{
				playerCard.ready = status;
				Toggle.ToggleEvent temp = playerCard.readyToggle.onValueChanged;
				playerCard.readyToggle.onValueChanged = null;
				playerCard.readyToggle.isOn = status;
				playerCard.readyToggle.onValueChanged = temp;
			}
		}
	}
	
	public void SetMonsterForPlayer(PhotonPlayer player, bool status)
	{
		photonView.RPC ("RPCMonster", PhotonTargets.AllBufferedViaServer, player, status);
	}
	
	[PunRPC]
	private void RPCMonster(PhotonPlayer player, bool status)
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			if (playerCard.player == player)
			{
				Toggle.ToggleEvent temp = playerCard.isMonster.onValueChanged;
				playerCard.isMonster.onValueChanged = null;
				playerCard.isMonster.isOn = status;
				playerCard.isMonster.onValueChanged = temp;
			}
		}
	}
	
	public void SetTimer()
	{
		if (PhotonNetwork.isMasterClient)
		{
			timeLimit = int.Parse (timeLimitField.text);
			if (timeLimit <= 0)
			{
				timeLimit = 5;
				timeLimitField.text = "5";
			}
		}
	}
	
	void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (PhotonNetwork.isMasterClient)
		{
			mapSelect.interactable = true;
			timeLimitField.interactable = true;
			startButton.interactable = true;
		}
	}
	
	[PunRPC]
	void RPCSetTimer(int time)
	{
		_timeLimit = time;
		timeLimitField.text = time.ToString();
	}
}
