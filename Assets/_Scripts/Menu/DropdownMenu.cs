using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DropdownMenu : MonoBehaviour {

	[SerializeField] string[] mapNames;
	
	[SerializeField] Text mainButton;
	[SerializeField] Lobby lobby;
	
	[SerializeField] Transform menuPanel;
	[SerializeField] GameObject buttonPrefab;

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < mapNames.Length; ++i)
		{
			GameObject button = Instantiate (buttonPrefab) as GameObject;
			button.GetComponentInChildren<Text>().text = mapNames[i];
			string mapName = mapNames[i];
			button.GetComponent<Button>().onClick.AddListener
			(
				() => {SetMap (mapName);}
			);
			button.transform.parent = menuPanel;
		}
	}
	
	void SetMap(string mapName)
	{
		lobby.ChooseMap(mapName);
		menuPanel.gameObject.SetActive(false);
	}
}
