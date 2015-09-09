using UnityEngine;
using System.Collections;

public class SoundSource : MonoBehaviour {

	public Light monsterLightSource;
	public float decaySpeed;
	
	//private bool bIsValid = false;

	public void Init(float strength)
	{
		monsterLightSource.intensity = strength;
		//bIsValid = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (monsterLightSource.range > 1f)
		{
			monsterLightSource.range = Mathf.Lerp (monsterLightSource.range, 0f, decaySpeed);
		}
		else //if (bIsValid)
		{
			Destroy (gameObject);
		}
	}
}
