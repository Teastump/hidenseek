using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	public float currentHealth
	{
		get { return player.health; }
		set 
		{
			if (value < 0f)
				healthSlider.value = 0f;
			else
				healthSlider.value = value;
		}
	}
	public float maxHealth
	{
		get { return healthSlider.maxValue; }
		set { healthSlider.maxValue = value; }
	}
	
	public Slider healthSlider;
	
	public PlayerController player;
}
