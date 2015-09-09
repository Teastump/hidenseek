using UnityEngine;
using System.Collections;

public class TestController : MonoBehaviour {

	public GameObject monsterPrefab;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings ("0.1");
	}
	
	void OnJoinedLobby()
	{
		CreateRoom ();
	}
	
	public void CreateRoom()
	{
		
		RoomOptions optn = null;
		
		if (optn == null)
		{
			optn = new RoomOptions ();
			optn.isOpen = true;
			optn.isVisible = true;
			optn.maxPlayers = 5;
			
			ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
			customProps["map"] = "hs_test";
			optn.customRoomProperties = customProps;
			
			string[] exposedProps = new string[1];
			exposedProps[0] = "map";
			
			optn.customRoomPropertiesForLobby = exposedProps;
		}
		
		PhotonNetwork.CreateRoom ("testRoom", optn, TypedLobby.Default);
		//PhotonNetwork.LoadLevel ("hs_test");
	}
	
	void OnJoinedRoom()
	{
		Debug.Log("Connected to Room");
		PhotonNetwork.Instantiate (monsterPrefab.name, Vector3.up * 5, Quaternion.identity, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
