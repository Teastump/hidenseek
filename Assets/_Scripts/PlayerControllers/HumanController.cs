using UnityEngine;
using System.Collections;

public class HumanController : PlayerController {

	public GameObject footstepSound;
	public GameObject breathSound;
	public GameObject throwable;
	public float throwCooldown;
	public float breathCooldown;

	private bool sprinting;
	private bool creeping;
	private float throwTimer = 0f;
	private float footstepTimer = 0f;
	private float footstepCooldown;
	private float breathTimer = 0f;
	
	
	// Update is called once per frame
	override protected void Update () {
		
		float deltaTime = Time.deltaTime;
		
		throwTimer += deltaTime;
		footstepTimer += deltaTime;
		breathTimer += deltaTime;
	
		base.Update ();
		
		if (photonView.isMine)
		{
			InputHumanAction ();
			CheckStamina ();
		}
		
		MakeFootstep ();
		CheckBreathing ();
	}
	
	void InputHumanAction()
	{
		if (!paused)
		{
			//Throw rock
			if (Input.GetButtonUp ("Throw") && throwTimer >= throwCooldown)
			{
			
				throwTimer = 0f;
			
				Vector3 pos = playerCamera.transform.position + playerCamera.transform.forward;
				Vector3 velocity = playerCamera.transform.forward * 10f + rb.velocity;
		
				photonView.RPC ("RPCThrow", PhotonTargets.All, pos, velocity);
			}
		
			//Sprint
			if (Input.GetButtonDown ("Sprint") && !creeping)
			{
				sprinting = true;
				maxGroundSpeed *= 2;
			}
			
			//Stop Sprint
			if (Input.GetButtonUp ("Sprint") && sprinting)
			{
				sprinting = false;
				maxGroundSpeed /= 2;
			}
			
			//Creep
			if (Input.GetButtonDown ("Creep") && !sprinting)
			{
				creeping = true;
				maxGroundSpeed /= 2;
			}
			
			//Stop Creep
			if (Input.GetButtonUp ("Creep") && creeping)
			{
				creeping = false;
				maxGroundSpeed *= 2;
			}
		}
	}
	
	void CheckStamina()
	{
		if (sprinting == true)
		{
			if (!UseStamina (20 * Time.deltaTime))
			{
				sprinting = false;
				maxGroundSpeed /= 2;
			}
		}
		if (creeping == true)
		{
			if (!UseStamina (10 * Time.deltaTime))
			{
				creeping = false;
				maxGroundSpeed *= 2;
			}
		}
	}
	
	void MakeFootstep()
	{
		footstepCooldown = 3f / velocity;
		
		if (footstepTimer >= footstepCooldown && velocity >= 2.5f)
		{
			footstepTimer = 0f;
				
			Debug.Log("Footstep sound should be made");
			
			Vector3 floorPos = rb.position;
			floorPos.y -= (distToGround - 0.1f);
			GameObject step = Instantiate (footstepSound, floorPos, rb.rotation) as GameObject;
			SoundSource footstep = step.GetComponent<SoundSource> ();
			footstep.Init (velocity);
		}
	}
	
	void CheckBreathing ()
	{
		if (GameController.instance.isMonster && !creeping)
		{
			float distance = (GameController.instance.myPlayer.transform.position - transform.position).magnitude;
			
			if (distance < 7.5f && breathTimer >= breathCooldown)
			{
				breathTimer = 0f;
				
				GameObject breath = Instantiate (breathSound, transform.position, transform.rotation) as GameObject;
				SoundSource breathing = breath.GetComponent<SoundSource> ();
				breathing.Init ((1f / (stamina + 1f)) + .02f);
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
	
	protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnPhotonSerializeView(stream, info);
		
		if (stream.isWriting)
		{
			stream.SendNext (creeping);
		}
		else
		{
			creeping = (bool) stream.ReceiveNext ();
		}
	}
}
