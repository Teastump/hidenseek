using UnityEngine;
using System.Collections;

public class SoundSource : MonoBehaviour {

	public Light monsterLightSource;
	public float decaySpeed;
	public AudioClip[] sounds;
	public AudioSource audioSource;

	public void Init(float strength)
	{
		if (!GameController.instance.isMonster)
		{
			monsterLightSource.enabled = false;
		}
		else
		{
			monsterLightSource.intensity = strength;
		}
		
		float distance = (GameController.instance.myPlayer.transform.position - transform.position).magnitude;
		
		audioSource.PlayOneShot(GetRandomAudioClip(), (strength * 2) / (distance + 4f));
	}
	
	AudioClip GetRandomAudioClip()
	{
		int rand = Random.Range (0, sounds.Length);
		return sounds[rand];
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (monsterLightSource.range > 1f)
		{
			monsterLightSource.range = Mathf.Lerp (monsterLightSource.range, 0f, decaySpeed);
		}
		else
		{
			Destroy (gameObject);
		}
	}
}
