using UnityEngine;
using System.Collections;

public class MonsterPing : MonoBehaviour {

	public Light pingLight;
	public float decaySpeed;
	
	private bool bIsValid = false;
	
	public void Init(float strength)
	{
		pingLight.range = strength;
		bIsValid = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (pingLight.range > 1f)
		{
			pingLight.range = Mathf.Lerp (pingLight.range, 0f, decaySpeed);
		}
		else if (bIsValid)
		{
			Destroy (gameObject);
		}
	}
}
