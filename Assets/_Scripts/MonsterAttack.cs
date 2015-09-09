using UnityEngine;
using System.Collections;

public class MonsterAttack : MonoBehaviour {

	private Collider hurtTrigger;
	
	private int attackFrames;
	private int maxAttackFrames;
	private float damage;

	// Use this for initialization
	void Awake () 
	{
		hurtTrigger = GetComponent<Collider> ();
		hurtTrigger.enabled = false;
		
		maxAttackFrames = 15;
	}
	
	public void Attack (float damage)
	{
		this.damage = damage;
		hurtTrigger.enabled = true;
		attackFrames = 0;
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("Human"))
		{
			PlayerController hurtPlayer = other.GetComponent<PlayerController> ();
			hurtPlayer.TakeDamage(damage);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		++attackFrames;
		
		if (attackFrames >= maxAttackFrames)
		{
			hurtTrigger.enabled = false;
		}
	}
}
