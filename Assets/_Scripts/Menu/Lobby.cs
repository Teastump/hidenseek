using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Lobby : Photon.MonoBehaviour {

	public PlayerCard[] playerCards;
	
	public InputField timeLimitField;
	
	public string mapName
	{
		get { return _mapName; }
		private set { _mapName = value; }
	}
	private string _mapName = "hs_forest";
	
	public int timeLimit
	{
		get { return _timeLimit; }
		private set { photonView.RPC ("RPCSetTimer", PhotonTargets.AllBuffered, value); }
	}
	private int _timeLimit;
	
	void Awake()
	{
		if (!PhotonNetwork.isMasterClient)
		{
			timeLimitField.gameObject.SetActive (false);
		}
	}
	
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
		
		return false;
	}
	
	public void Clear()
	{
		foreach (PlayerCard playerCard in playerCards)
		{
			playerCard.player = null;
		}
	}
	
	public void ChooseMap(string map)
	{
		mapName = map;
	}
	
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
			timeLimitField.gameObject.SetActive (true);
		}
	}
	
	[PunRPC]
	void RPCSetTimer(int time)
	{
		_timeLimit = time;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
