using UnityEngine;
using System.Collections;

public class MonsterController : PlayerController {

	public GameObject pingLight;
	public MonsterAttack attack;
	public float attackSpeed;
	public float damage;
	
	private float attackTimer = 0f;
	
	protected override void Start ()
	{
		base.Start ();
		
		maxGroundSpeed = 6f;
	}
	
	// Update is called once per frame
	override protected void Update () {
		
		attackTimer += Time.deltaTime;
		
		base.Update ();
		
		if (photonView.isMine)
		{
			InputMonsterAction ();
		}
	}
	
	void InputMonsterAction()
	{
		if (!paused)
		{
			if (Input.GetButtonUp ("Monster Ping"))
			{
				GameObject instance = Instantiate(pingLight, transform.position, transform.rotation) as GameObject;
				instance.GetComponent<MonsterPing>().Init (UseAllStamina ());
			}
		
			if (Input.GetButton ("Monster Attack"))
			{
				if (attackTimer >= attackSpeed && UseStamina (10f))
				{
					attackTimer = 0f;
					attack.Attack (damage);
					Debug.Log ("Swipe!");
				}
			}
		}
	}
}
