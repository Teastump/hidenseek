using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public static GameController instance;
	
	public SpawnPoint[] spawnPoints;

	public Light playerAmbientLight;
	public Text timeLimitLabel;
	public HUDController hud;
	public GameMenu menu;

	public bool isMonster
	{
		get 
		{
			return _monster;
		}
		private set
		{
			_monster = value;
			if (_monster == true)
			{
				RenderSettings.ambientIntensity = 0f;
				RenderSettings.reflectionIntensity = 0f;
				playerAmbientLight.enabled = false;
			}
			else
			{
				RenderSettings.ambientIntensity = 0f;
				RenderSettings.reflectionIntensity = 0f;
				playerAmbientLight.enabled = false;
			}
		}
	}
	private bool _monster;
	
	public GameObject myPlayer
	{
		get 
		{
			return _player;
		}
		private set
		{
			if (_player == null && value != null)
			{
				_player = value;
				if (_player.GetComponent<MonsterController>() != null)
				{
					isMonster = true;
				}
				else
				{
					isMonster = false;
				}
				
				hud.myPlayer = _player.GetComponent<PlayerController> ();
			}
		}
	}
	private GameObject _player = null;
	
	private List<SpawnPoint> monsterSpawns = new List<SpawnPoint>();
	private List<SpawnPoint> humanSpawns = new List<SpawnPoint>();
	
	private List<GameObject> players = new List<GameObject>();
	private float timeLimit;
	private float timer;
	private bool gameOver;

	void Awake() 
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}
		
		timer = -1000f;
		gameOver = true;
	}
	
	void Start()
	{
		NetworkManager.instance.LevelDidLoad ();
		LockCursor ();
	}
	
	public void GetPlayer()
	{
		PlayerController[] players = FindObjectsOfType<PlayerController> ();
		foreach (PlayerController player in players)
		{
			PhotonView pView = player.GetComponent<PhotonView>();
			if (pView.isMine)
			{
				myPlayer = player.gameObject;
				break;
			}
		}
	}
	
	public void GetPlayerList()
	{
		players.Clear();
		
		PlayerController[] playerArray = FindObjectsOfType<PlayerController> ();
		foreach (PlayerController player in playerArray)
		{
			players.Add(player.gameObject);
		}
		
		gameOver = false;
	}
	
	public void SpawnPlayer(GameObject playerPrefab)
	{
		SplitSpawnPoints();
		List<SpawnPoint> spawnList;
		
		if (playerPrefab.GetComponent<MonsterController>() != null)
			spawnList = monsterSpawns;
		else
			spawnList = humanSpawns;
		
		int rand = Random.Range (0, spawnList.Count);
		int index = rand;
		
		do
		{
			SpawnPoint spawnPoint = spawnList[index];
			Transform point;
			
			point = spawnPoint.GetValidSpawn(SpawnPoint.SpawnType.Both);
				
			if (point != null)
			{
				myPlayer = PhotonNetwork.Instantiate (playerPrefab.name, point.position, point.rotation, 0) as GameObject; 
				NetworkManager.instance.PlayerSpawned ();
				return;
			}
			
			++index;
			if (index >= spawnList.Count)
			{
				index = 0;
			}
		} while (index != rand);
		
		Debug.Log ("Not enough spawn points on the map! Failed to spawn player");
	}
	
	void SplitSpawnPoints()
	{
		foreach (SpawnPoint spawnPoint in spawnPoints)
		{
			if (spawnPoint.spawnType == SpawnPoint.SpawnType.Both)
			{
				monsterSpawns.Add (spawnPoint);
				humanSpawns.Add (spawnPoint);
			}
			else if (spawnPoint.spawnType == SpawnPoint.SpawnType.Human)
			{
				humanSpawns.Add (spawnPoint);
			}
			else
			{
				monsterSpawns.Add (spawnPoint);
			}
		}
	}
	
	public void StartTimer(float limit)
	{
		timeLimit = limit * 60f;
		timer = 0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetButtonDown ("Cancel"))
		{
			Debug.Log ("Escape pressed, value of menu game object: " + menu.gameObject.activeSelf);
			ShowMenu (!menu.gameObject.activeSelf);
		}
	
		if (!gameOver)
		{
			timer += Time.deltaTime;
		
			float timeRemaining = timeLimit - timer + 1f;
		
			timeLimitLabel.text = "Time Remaining: " + (Mathf.Floor ((timeRemaining) / 60f)).ToString ("F0") + ":" + Mathf.Floor (timeRemaining % 60f).ToString ("00");
		
			if (timer >= timeLimit)
			{
				HumansWin ();
			}
			foreach (GameObject player in players)
			{
				if (player != null && player.GetComponent<HumanController> () != null)
				{
					return;
				}
			}
		
			MonsterWins ();
		}
	}
	
	public void PlayerDied(GameObject player)
	{
		Debug.Log ("Player died, removing from player list");
	
		players.Remove (player);
	}
	
	void HumansWin()
	{
		Debug.Log ("Humans win!");
		
		gameOver = true;
		if (PhotonNetwork.isMasterClient)
			StartCoroutine (RestartGame (5));
	}
	
	void MonsterWins()
	{
		Debug.Log ("Monster wins!");
		
		gameOver = true;
		if (PhotonNetwork.isMasterClient)
			StartCoroutine (RestartGame(5));
	}
	
	IEnumerator RestartGame(float sec)
	{
		Debug.Log ("Game restarting in 5...");
	
		yield return new WaitForSeconds(sec);
		
		Debug.Log ("Game should have restarted...");
		
		NetworkManager.instance.RestartGame ();
	}
	
	void OnApplicationFocus(bool focusStatus)
	{
		Debug.Log ("Application focus switched");
		if (focusStatus)
		{
			Debug.Log ("Cursor should lock");
			LockCursor ();
		}
		else
		{
			Debug.Log ("Cursor should unlock");
			UnlockCursor ();
		}
	}
	
	void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		myPlayer.GetComponent<PlayerController>().paused = false;
	}
	
	void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		myPlayer.GetComponent<PlayerController>().paused = true;
	}
	
	public void ShowMenu(bool value)
	{
		if (value)
			UnlockCursor ();
		else
			LockCursor ();
			
		menu.gameObject.SetActive (value);
		hud.gameObject.SetActive (!value);
	}
}
