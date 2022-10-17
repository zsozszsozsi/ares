using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ParticlesManager : MonoBehaviour
{
	public enum Particles { ExplosionSmall, ExplosionMed, ExplosionBig,
		Muzzleflash, 
		MetalHit, SmokeSmall }

	//Make this script a singleton
	private static ParticlesManager _instance;
	public static ParticlesManager Instance { get { return _instance; } }

	public GameObject explosionSmallEffect;
    public GameObject explosionMedEffect;
    public GameObject explosionBigEffect;
	public GameObject muzzleFlashEffect;
	public GameObject metalHitEffect;
	public GameObject smokeSmallEffect;


	void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("More than one ParticlesManager in scene!");
			return;
		}
		_instance = this;
	}

	[PunRPC]
	public void SpawnParticleNetworked(int particle, float x, float y, float z)
	{
		SpawnParticle((Particles)particle, new Vector3(x, y, z));
	}

	public void SpawnParticle(Particles particle, Vector3 position, float size = 1f)
	{
		GameObject toSpawn;

		switch (particle)
		{
			case Particles.ExplosionSmall:
				toSpawn = explosionSmallEffect;
				break;
			case Particles.ExplosionMed:
				toSpawn = explosionMedEffect;
				break;
			case Particles.ExplosionBig:
				toSpawn = explosionBigEffect;
				break;
			case Particles.Muzzleflash:
				toSpawn = muzzleFlashEffect;
				break;
			case Particles.MetalHit:
				toSpawn = metalHitEffect;
				break;
			case Particles.SmokeSmall:
				toSpawn = smokeSmallEffect;
				break;
			default:
				Debug.LogError("Unhandled particles case!");
				return;
		}

		GameObject spawned = GameObject.Instantiate(toSpawn, position, Quaternion.identity);
		if(size != 1f)
			spawned.transform.localScale = new Vector3(size, size, size);
	}

}
