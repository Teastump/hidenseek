using UnityEngine;
using System.Collections;

public class Flashlight : MonoBehaviour {

	public Light flashlight;

	// Use this for initialization
	void Awake () {
		if (GameController.instance.isMonster)
		{
			Debug.Log ("Current Player is monster, turning off this player's lights");
			flashlight.enabled = false;
		}
	}
}
