using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
	public float maxGroundSpeed = 4f;
	public float sensitivity = 1f;
	public Camera playerCamera;
	
	public float health
	{
		get { return _health; }
		private set 
		{
			_health = value;
			
			if (hud != null)
				hud.healthBar.currentHealth = value;
			
			if (_health <= 0)
			{
				Die ();
			}
		}
	}
	private float _health;
	
	public float stamina
	{
		get { return _stamina; }
		private set 
		{
			if (value < _stamina)
			{
				recovering = false;
				timer = 0f;
			}
		
			if (value < 0f)
				_stamina = 0f;
			else
				_stamina = value;
			
			if (hud != null)
				hud.staminaBar.currentStamina = _stamina;
		}
	}
	private float _stamina;
	public float maxStamina;

	public HUDController hud;
	
	public float staminaRecoveryPause;
	public float staminaRecoveryRate; //Percentage rate of recovery

	protected Rigidbody rb;
	protected float distToGround;
	protected float velocity;

	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion syncStartRotation = Quaternion.identity;
	private Quaternion syncEndRotation = Quaternion.identity;
	
	private float verticalRange = 90f;
	
	private float hRot = 0f;
	private float vRot = 0f;
	
	private float friction = 8f;
	private float groundAccelerate = 80f;
	
	private float timer = 0f;
	private bool recovering = false;

	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody> ();
		
		if (!photonView.isMine)
		{
			playerCamera.enabled = false;
			playerCamera.GetComponent<AudioListener>().enabled = false;
			Light flashlight = playerCamera.GetComponentInChildren<Light>();
			//if (flashlight != null && GameController.instance.isMonster)
			//	flashlight.enabled = false;
		}
		
		health = 100f;
		stamina = maxStamina;
		
		distToGround = GetComponent<Collider>().bounds.extents.y;
		
		//hud = GameController.instance.hud;
	}

	protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) 
		{
			stream.SendNext (rb.position);
			stream.SendNext (rb.velocity);
			stream.SendNext (rb.rotation);
		} 
		else 
		{
			Vector3 syncPosition = (Vector3)stream.ReceiveNext ();
			Vector3 syncVelocity = (Vector3)stream.ReceiveNext ();
			Quaternion syncRotation = (Quaternion)stream.ReceiveNext ();

			velocity = syncVelocity.magnitude;

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = rb.position;
			syncEndRotation = syncRotation;
			syncStartRotation = rb.rotation;
		}
	}

	virtual protected void Update()
	{
		if (photonView.isMine)
		{
			InputMovement ();
			InputRotation ();
			StaminaRecovery ();
		}
		else
		{
			SyncedMovement ();
			SyncedRotation ();
			
			CheckRenderDistance ();
		}
	}
	
	// Update is called once per frame
	protected void FixedUpdate () 
	{
		if (stamina < maxStamina && recovering == true)
		{
			stamina += (maxStamina * (staminaRecoveryRate / 100f) * Time.fixedDeltaTime);
		}
	}

	protected void InputMovement()
	{
		/*
		if (Input.GetKey(KeyCode.W))
			rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.S))
			rb.MovePosition(rb.position - transform.forward * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.D))
			rb.MovePosition(rb.position + transform.right * speed * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.A))
			rb.MovePosition(rb.position - transform.right * speed * Time.deltaTime);
		*/
		
		if (IsGrounded ())
		{
			float forward = 0f;
			float side = 0f;
		
			if (Input.GetKey (KeyCode.W))
				forward += 1f;
			if (Input.GetKey (KeyCode.S))
				forward -= 1f;
			if (Input.GetKey (KeyCode.D))
				side += 1f;
			if (Input.GetKey (KeyCode.A))
				side -= 1f;
			
			Vector3 moveDirNorm = ((transform.forward * forward) + (transform.right * side)).normalized;
		
			rb.velocity = MoveGround (moveDirNorm, rb.velocity);
			//rb.MovePosition(rb.position + MoveGround (moveDirNorm, rb.velocity));
				
			if (Input.GetKeyUp (KeyCode.Space))
				rb.AddForce (Vector3.up * 5, ForceMode.VelocityChange);
		}
			
	}

	protected void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	
	protected void InputRotation()
	{
		hRot = Input.GetAxis ("Mouse X") * sensitivity;
		transform.Rotate (0f, hRot, 0f);
		
		vRot -= Input.GetAxis ("Mouse Y") * sensitivity;
		vRot = Mathf.Clamp(vRot, -verticalRange, verticalRange);
		
		//Camera.main.transform.Rotate(vRot, 0f, 0f);
		playerCamera.transform.localRotation = Quaternion.Euler(vRot, 0f, 0f);
	}
	
	protected void SyncedRotation()
	{
		rb.rotation = Quaternion.Lerp (syncStartRotation, syncEndRotation, syncTime / syncDelay);
	}
	
	protected bool IsGrounded()
	{
		return Physics.Raycast (transform.position, -Vector3.up, distToGround + 0.1f);
	}
	
	private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
	{
		float projVel = Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir.
		float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment
		
		// If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
		if(projVel + accelVel > max_velocity)
			accelVel = max_velocity - projVel;
		
		return prevVelocity + accelDir * accelVel;
	}
	
	private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity)
	{
		// Apply Friction
		float speed = prevVelocity.magnitude;
		if (speed != 0) // To avoid divide by zero errors
		{
			float drop = speed * friction * Time.fixedDeltaTime;
			prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
		}
		
		// ground_accelerate and max_velocity_ground are server-defined movement variables
		return Accelerate(accelDir, prevVelocity, groundAccelerate, maxGroundSpeed);
	}
	
	private void CheckRenderDistance()
	{
		if (GameController.instance.isMonster)
		{
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			float distance = (GameController.instance.myPlayer.transform.position - transform.position).magnitude;
			
			if (distance > 40)
			{
				renderer.enabled = false;
			}
			else
			{
				renderer.enabled = true;
			}
			
		}
	}
	
	public void TakeDamage (float damage)
	{
		photonView.RPC ("RPCTakeDamage", PhotonTargets.All, damage);
	}
	
	[PunRPC]
	protected void RPCTakeDamage(float damage)
	{
		health -= damage;
		Debug.Log ("Player took " + damage + " damage.\nCurrent health: " + health + ".");
	}
	
	protected void Die()
	{
		GameController.instance.PlayerDied (gameObject);
		Destroy (gameObject);
	}
	
	void OnGUI()
	{
		if (photonView.isMine)
		{
			GUILayout.Label (rb.velocity.magnitude.ToString());
		}
	}
	
	///////////////////////////////
	
	protected bool UseStamina(float staminaCost)
	{
		if (staminaCost <= stamina)
		{
			stamina -= staminaCost;
			return true;
		}
		
		return false;
	}
	
	protected float UseAllStamina()
	{
		float temp = stamina;
		stamina = 0f;
		
		return temp;
	}
	
	void StaminaRecovery()
	{
		timer += Time.deltaTime;
		
		if (timer >= staminaRecoveryPause)
		{
			recovering = true;
		}
	}
	
}
