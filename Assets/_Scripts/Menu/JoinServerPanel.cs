using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/*Lists all servers that can be connected to*/
public class JoinServerPanel : MonoBehaviour {

	public GameObject serverInfoObject;

	private int serverCount;
	private List<GameObject> serverList = new List<GameObject>();

	// Use this for initialization
	void Start () 
	{
		serverCount = 0;
	}

	//Refreshes list of servers
	public void RefreshRoomList(RoomInfo[] roomList)
	{
		ClearServerList ();

		foreach (RoomInfo room in roomList) 
		{
			GameObject instance = GameObject.Instantiate (serverInfoObject) as GameObject;
			instance.transform.SetParent (this.transform, false);
			Image serverPanel = instance.GetComponent<Image> ();
			serverPanel.rectTransform.anchoredPosition = new Vector2(0, -15 - serverCount*15);
			ServerInfo serverInfo = serverPanel.GetComponent<ServerInfo> ();
			serverInfo.Init(room.name, room.customProperties["map"].ToString (), room.playerCount, room.maxPlayers);
			serverList.Add (instance);
			++serverCount;
		}
	}

	void ClearServerList ()
	{
		foreach (GameObject server in serverList) 
		{
			Destroy (server);
		}

		serverList.Clear ();
		serverCount = 0;
	}
}
