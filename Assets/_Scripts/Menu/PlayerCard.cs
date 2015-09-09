using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour {

	public Text name;
	public Toggle isMonster;
	public Toggle readyToggle;
	
	public bool ready
	{
		get { return _ready; }
		set 
		{
			_ready = value;
			
			
		}
	}
	private bool _ready;
	
	public PhotonPlayer player
	{
		get { return _player; }
		set 
		{
			_player = value;
			
			if (_player != null)
			{
				name.text = _player.name;
				isMonster.gameObject.SetActive (true);
				readyToggle.gameObject.SetActive (true);
				if (_player == PhotonNetwork.player)
				{
					isMonster.interactable = true;
					readyToggle.interactable = true;
				}
				else 
				{
					isMonster.interactable = false;
					readyToggle.interactable = false;
				}
			}
			else
			{
				name.text = "Empty";
				isMonster.gameObject.SetActive (false);
				readyToggle.gameObject.SetActive (false);
			}
		}
	}
	private PhotonPlayer _player = null;
	
	private Lobby lobby;

	// Use this for initialization
	void Awake () 
	{
		player = null;
		lobby = GetComponentInParent<Lobby>();
	}
	
	public void ToggleReady()
	{
		lobby.SetReadyForPlayer (player, !ready);
	}
	
	public void ToggleMonster()
	{
		lobby.SetMonsterForPlayer (player, isMonster.isOn);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
