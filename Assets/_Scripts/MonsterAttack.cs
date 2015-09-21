using UnityEngine;
using System.Collections;

public class MonsterAttack : MonoBehaviour {

	[SerializeField] Animator anim;
	
	private PhotonView photonView;

	private Collider hurtTrigger;
	private float damage;
	private int hits = 0;
	
	private bool hit;

	// Use this for initialization
	void Awake () 
	{
		photonView = anim.GetComponent<PhotonView> ();
		hurtTrigger = GetComponent<Collider> ();
		hurtTrigger.enabled = false;
	}
	
	public void Attack (float damage)
	{
		this.damage = damage;
		hit = false;
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("Human") && !hit)
		{
			hit = true;
			++hits;
			PlayerController hurtPlayer = other.GetComponent<PlayerController> ();
			hurtPlayer.TakeDamage(damage);
			
			Debug.Log ("Hits: " + hits);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && photonView.isMine)
		{
			hurtTrigger.enabled = true;
		}
		else
		{
			hurtTrigger.enabled = false;
			hits = 0;
		}
	}
}
