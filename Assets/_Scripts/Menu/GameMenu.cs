using UnityEngine;
using System.Collections;

/*Class to handle inputs for the in-game menu*/
public class GameMenu : MonoBehaviour {

	public void Resume()
	{
		GameController.instance.ShowMenu (false);
	}
	
	public void Disconnect()
	{
		PhotonNetwork.LeaveRoom ();
		Destroy (NetworkManager.instance.gameObject);
		Application.LoadLevel (0);
	}
	
	public void Exit()
	{
		Application.Quit ();
	}
	
	
}
