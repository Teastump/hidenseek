using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
	public float maxGroundSpeed = 3f;
	public float sensitivity = 1f;
	public Camera playerCamera;
	[SerializeField] GameObject mesh;
	
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
	
	public bool paused
	{
		get { return _paused; }
		set { _paused = value; }
	}
	private bool _paused;

	protected Rigidbody rb;
	protected float distToGround;
	protected float velocity;
	protected MeshRenderer renderer;

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
	private float downForce = 1f;
	
	protected Animator anim;

	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody> ();
		anim = GetComponent<Animator> ();
		
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
		renderer = GetComponent<MeshRenderer>();
		
		//hud = GameController.instance.hud;
	}

	protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) 
		{
			stream.SendNext (rb.position);
			stream.SendNext (rb.velocity);
			stream.SendNext (rb.rotation);
			stream.SendNext (_stamina);
		} 
		else 
		{
			Vector3 syncPosition = (Vector3)stream.ReceiveNext ();
			Vector3 syncVelocity = (Vector3)stream.ReceiveNext ();
			Quaternion syncRotation = (Quaternion)stream.ReceiveNext ();
			_stamina = (float) stream.ReceiveNext ();

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
			velocity = rb.velocity.magnitude;
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
		
		Animate ();
	}
	
	// Update is called once per frame
	protected void FixedUpdate () 
	{
		if (stamina < maxStamina && recovering == true)
		{
			stamina += (maxStamina * (staminaRecoveryRate / 100f) * Time.fixedDeltaTime);
		}
		
		if (!IsGrounded ())
		{
			if (downForce < 1f) downForce += 5f * Time.fixedDeltaTime;
			if (downForce > 1f) downForce = 1f;
			
			Vector3 downVelocity = rb.velocity;
			downVelocity.y -= downForce;
			rb.velocity = downVelocity;
		}
	}

	protected void InputMovement()
	{
		if (!paused)
		{
			float forward = Input.GetAxisRaw ("Vertical");
			float side = Input.GetAxisRaw ("Horizontal");
			
			Vector3 moveDirNorm = ((transform.forward * forward) + (transform.right * side)).normalized;
		
			rb.velocity = MoveGround (moveDirNorm, rb.velocity);
		
			if (IsGrounded ())
			{		
				if (Input.GetButtonUp ("Jump"))
				{
					Jump ();
				}
			}
		}
	}

	protected void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	
	protected void InputRotation()
	{
		if (!paused)
		{
			hRot = Input.GetAxis ("Mouse X") * sensitivity;
			transform.Rotate (0f, hRot, 0f);
			
			vRot -= Input.GetAxis ("Mouse Y") * sensitivity;
			vRot = Mathf.Clamp(vRot, -verticalRange, verticalRange);
		
			playerCamera.transform.localRotation = Quaternion.Euler(vRot, 0f, 0f);
		}
	}
	
	protected void SyncedRotation()
	{
		rb.rotation = Quaternion.Lerp (syncStartRotation, syncEndRotation, syncTime / syncDelay);
	}
	
	protected bool IsGrounded()
	{
		return Physics.Raycast (transform.position, -Vector3.up, 0.2f);
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
			float distance = (GameController.instance.myPlayer.transform.position - transform.position).magnitude;
			
			if (distance > 40)
			{
				mesh.SetActive (false);
			}
			else
			{
				mesh.SetActive (true);
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
		
		if (gameObject == GameController.instance.myPlayer)
		{
			hud.Hurt ();
		}
	}
	
	protected void Die()
	{
		GameController.instance.PlayerDied (gameObject);
		Destroy (gameObject);
	}
	
	void Jump()
	{
		Vector3 upVelocity = rb.velocity;
		upVelocity.y = 20f;
		rb.velocity = upVelocity;
		
		downForce = 0f;
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
	
	protected virtual void Animate()
	{
		if (velocity < 1f)
		{
			anim.SetBool ("walking", false);
			anim.SetBool ("running", false);
		}
		else if (velocity <= 4.5f)
		{
			anim.SetBool ("walking", true);
			anim.SetBool ("running", false);
		}
		else
		{
			anim.SetBool ("walking", true);
			anim.SetBool ("running", true);
		}
	}
	
}
