using UnityEngine;
using System.Collections;

public class SpawnPoint : Photon.MonoBehaviour {

	public enum SpawnType { Monster, Human, Both }
	
	public SpawnType spawnType;
	bool bIsValid;

	// Use this for initialization
	void Awake () {
		bIsValid = true;
	}
	
	public Transform GetValidSpawn(SpawnType forType)
	{
		photonView.RPC ("SetInvalid", PhotonTargets.Others);
	
		if (bIsValid && (spawnType == SpawnType.Both || forType == spawnType || forType == SpawnType.Both) )
		{
			bIsValid = false;
			return transform;
		}
		else
			return null;
	}
	
	[PunRPC]
	void SetInvalid()
	{
		bIsValid = false;
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
