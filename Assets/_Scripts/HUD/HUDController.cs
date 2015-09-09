using UnityEngine;
using System.Collections;

public class HUDController : MonoBehaviour {

	public StaminaBar staminaBar;
	public HealthBar healthBar;
	public PlayerController myPlayer
	{
		get { return _player; }
		set 
		{
			_player = value;
			_player.hud = this;
			
			staminaBar.player = _player;
			healthBar.player = _player;
			
			if (_player is MonsterController)
			{
				healthBar.gameObject.SetActive (false);
			}
		}
	}
	private PlayerController _player;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
