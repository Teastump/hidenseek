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
				RenderSettings.ambientIntensity = 1f;
				RenderSettings.reflectionIntensity = 1f;
				playerAmbientLight.enabled = true;
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
		
		GetPlayerList ();
		timer = -1000f;
		gameOver = true;
	}
	
	void Start()
	{
		NetworkManager.instance.LevelDidLoad ();
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
	
	/*
	public void SetFlashlights()
	{
		GetPlayerList ();
		if (isMonster)
		{
			foreach (GameObject player in players)
			{
				if (player != myPlayer)
					player.GetComponentInChildren<Light>().enabled = false;
			}
		}
	}
	*/
	
	public void SpawnPlayer(GameObject playerPrefab)
	{
		
		int rand = Random.Range (0, spawnPoints.Length);
		int index = rand;
		
		do
		{
			SpawnPoint spawnPoint = spawnPoints[index];
			Transform point;
			
			if (playerPrefab.GetComponent<MonsterController> () != null)
				point = spawnPoint.GetValidSpawn (SpawnPoint.SpawnType.Monster);
			else
				point = spawnPoint.GetValidSpawn (SpawnPoint.SpawnType.Human);
				
			if (point != null)
			{
				myPlayer = PhotonNetwork.Instantiate (playerPrefab.name, point.position, point.rotation, 0) as GameObject; 
				NetworkManager.instance.PlayerSpawned ();
				return;
			}
			
			++index;
			if (index >= spawnPoints.Length)
			{
				index = 0;
			}
		} while (index != rand);
		
		Debug.Log ("Not enough spawn points on the map! Failed to spawn player");
	}
	
	public void StartTimer(float timeLimit)
	{
		this.timeLimit = timeLimit * 60f;
		timer = 0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!gameOver)
		{
			timer += Time.deltaTime;
		
			float timeRemaining = timeLimit - timer;
		
			timeLimitLabel.text = "Time Remaining: " + (Mathf.Floor (timeRemaining / 60f)).ToString ("F0") + ":" + (timeRemaining % 60f).ToString ("00");
		
			if (timer >= timeLimit)
			{
				HumansWin ();
			}
			foreach (GameObject player in players)
			{
				if (player.GetComponent<HumanController> () != null)
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
}
