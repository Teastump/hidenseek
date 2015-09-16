using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {

	public StaminaBar staminaBar;
	public HealthBar healthBar;
	public Image hurtFlash;
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
	
	private float flashSpeed = 5f;
	
	public void Hurt()
	{
		Color flashColor = new Color(1, 0, 0, 0.15f);
		
		hurtFlash.color = flashColor;
	}
	
	void Update()
	{
		hurtFlash.color = Color.Lerp (hurtFlash.color, Color.clear, flashSpeed * Time.deltaTime);
	}
}
