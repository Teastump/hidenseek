using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour {

	public float currentStamina
	{
		get { return player.stamina; }
		set 
		{
			if (value < 0)
				staminaBar.value = 0f;
			else
				staminaBar.value = value;
		}
	}
	public float maxStamina
	{
		get { return staminaBar.maxValue; }
		set { staminaBar.maxValue = value; }
	}
	
	public Slider staminaBar;
	
	public PlayerController player;
	
}
