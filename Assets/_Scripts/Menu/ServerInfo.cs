using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ServerInfo : MonoBehaviour {

	public Text serverName;
	public Text mapName;
	public Text playerCount;

	public float clickDelay;

	private float clickTime;
	private int clicks = 0;


	// Use this for initialization
	void Awake () {
		gameObject.SetActive (false);
	}

	public void Init(string roomName, string mapName, int playerCount, int maxPlayers)
	{
		this.serverName.text = roomName;
		this.mapName.text = mapName;
		this.playerCount.text = playerCount.ToString () + "/" + maxPlayers.ToString ();

		gameObject.SetActive (true);
	}

	public void Join()
	{
		if (clicks > 0) 
		{
			if (Time.time - clickTime < clickDelay)
				++clicks;
			else
				clicks = 0;
		} 
		else 
		{
			clickTime = Time.time;
			++clicks;
		}

		if (clicks >= 2)
			NetworkManager.instance.JoinRoom (serverName.text, mapName.text);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
