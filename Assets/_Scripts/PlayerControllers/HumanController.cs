using UnityEngine;
using System.Collections;

public class HumanController : PlayerController {

	public GameObject footstepSound;
	public GameObject throwable;
	public float throwCooldown;

	private bool sprinting;
	private bool creeping;
	private float throwTimer = 0f;
	private float footstepTimer = 0f;
	private float footstepCooldown;
	
	// Update is called once per frame
	override protected void Update () {
		
		throwTimer += Time.deltaTime;
		footstepTimer += Time.deltaTime;
	
		base.Update ();
		
		if (photonView.isMine)
		{
			InputHumanAction ();
			CheckStamina ();
		}
		
		MakeFootstep ();
	}
	
	void InputHumanAction()
	{
		//Throw rock
		if (Input.GetButtonUp ("Fire1") && throwTimer >= throwCooldown)
		{
			
			throwTimer = 0f;
			
			Vector3 pos = playerCamera.transform.position + playerCamera.transform.forward;
			Vector3 velocity = playerCamera.transform.forward * 10f + rb.velocity;
		
			photonView.RPC ("RPCThrow", PhotonTargets.All, pos, velocity);
		}
		
		//Sprint
		if (Input.GetKeyDown (KeyCode.LeftShift) && !creeping)
		{
			sprinting = true;
			maxGroundSpeed *= 2;
		}
		
		//Stop Sprint
		if (Input.GetKeyUp (KeyCode.LeftShift) && sprinting)
		{
			sprinting = false;
			maxGroundSpeed /= 2;
		}
		
		//Creep
		if (Input.GetKeyDown (KeyCode.LeftControl) && !sprinting)
		{
			creeping = true;
			maxGroundSpeed /= 2;
		}
		
		//Stop Creep
		if (Input.GetKeyUp (KeyCode.LeftControl) && creeping)
		{
			creeping = false;
			maxGroundSpeed *= 2;
		}
	}
	
	void CheckStamina()
	{
		if (sprinting == true)
		{
			if (!UseStamina (5 * Time.deltaTime))
			{
				sprinting = false;
				maxGroundSpeed /= 2;
			}
		}
	}
	
	void MakeFootstep()
	{
		if (GameController.instance.isMonster)
		{
			footstepCooldown = 4f / velocity;
		
			if (footstepTimer >= footstepCooldown && velocity >= 3)
			{
				footstepTimer = 0f;
			
				Vector3 floorPos = rb.position;
				floorPos.y -= (distToGround - 0.1f);
				GameObject step = Instantiate (footstepSound, floorPos, rb.rotation) as GameObject;
				SoundSource footstep = step.GetComponent<SoundSource> ();
				footstep.Init (velocity);
			}
		}
	}
	
	[PunRPC]
	void RPCThrow(Vector3 origin, Vector3 velocity)
	{
		GameObject instance = Instantiate (throwable, origin, Quaternion.identity) as GameObject;
		
		Rigidbody rb = instance.GetComponent<Rigidbody> ();
		rb.velocity = velocity;
	}
}
