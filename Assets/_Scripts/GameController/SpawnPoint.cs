using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {

	public enum SpawnType { Monster, Human, Both }
	
	public SpawnType spawnType;
	bool bIsValid;

	// Use this for initialization
	void Awake () {
		bIsValid = true;
	}
	
	public Transform GetValidSpawn(SpawnType forType)
	{
		if (bIsValid && (spawnType == SpawnType.Both || forType == spawnType) )
		{
			bIsValid = false;
			return transform;
		}
		else
			return null;
	}
	
	protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext (bIsValid);
		}
		else
		{
			bIsValid = (bool)stream.ReceiveNext();
		}
	}
}
