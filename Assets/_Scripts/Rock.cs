using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {

	public GameObject soundSource;

	public float lifetime;
	public int maxBounces;

	private int bounces;
	private float timer;
	// Use this for initialization
	void Start () 
	{
		bounces = 0;
		timer = 0f;	
		
		if (GameController.instance.isMonster)
		{
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			renderer.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;
		
		if (timer >= lifetime)
		{
			Destroy (gameObject);
		}
	}
	
	void OnCollisionEnter(Collision coll)
	{
		if (bounces < maxBounces)
		{
			ContactPoint contact = coll.contacts[0];
			Vector3 pos = contact.point + Vector3.up * 2;
			
		
			//if (GameController.instance.isMonster)
			//{
			GameObject rockSound = Instantiate (soundSource, pos, Quaternion.identity) as GameObject;
			
			SoundSource bounceSound = rockSound.GetComponent<SoundSource> ();
			bounceSound.Init (coll.relativeVelocity.magnitude);
			
			Debug.Log ("Rock Collision Velocity: " + coll.relativeVelocity.magnitude);
			//}
		
			++bounces;
		}
	}
}
