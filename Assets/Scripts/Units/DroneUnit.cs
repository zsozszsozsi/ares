using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneUnit : Unit
{
	public int damage;
	public float range; //when are we close enough to explode

	public Vector3 moveDir = Vector3.forward;

	public PlayerBase targetBase;
	public Vector3 targetBasePos;



	[Tooltip("Temporary helper: should the speed be constant?")]
	public bool normalize = true;


	private void Update()
	{
		//only controll this unit if we are the owner - otherwise photon transformView handles the networking
		if (view.IsMine)
		{
			oldPosition = transform.position; // network miatt
			CheckPathfindingTarget();

			Move();
			movement = transform.position - oldPosition; // network miatt

			if (Vector3.Distance(trans.position, targetBasePos) < range)
			{
				AttackBase();
			}
		}

        if (!view.IsMine)
        {
			transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * moveSpeed);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 100);
			return;
		}
	}

	public override void Move()
	{
		moveDir = nextPathfindingTargetPos - trans.position;
		moveDir.y = 0; //dont move up/down

		//if(moveDir.magnitude > range && normalize) //if still out of range
			moveDir.Normalize(); //TEMP, allows it to slow down when near
								 //but also gives it jankier movement

		transform.Translate(moveDir * moveSpeed * Time.deltaTime);
	}

	[PunRPC]
	public override void Place(int plNum)
	{
		base.Place(plNum);

		if(owner == GameManager.Faction.Earth)
		{
			targetBase = GameManager.Instance.marsBase;
			if (plNum == 1)
				Debug.LogWarning("Spawning the enemy's unit!");

		} else if (owner == GameManager.Faction.Mars)
		{
			targetBase = GameManager.Instance.earthBase;
			if (plNum == 0)
				Debug.LogWarning("Spawning the enemy's unit!");
		}

		targetBasePos = targetBase.transform.position;
		targetBasePos.y = transform.position.y; //ignoring y
	}

	public void AttackBase()
	{
		//TODO: get view in prettier way?
		targetBase.gameObject.GetPhotonView().RPC("getDamage", RpcTarget.All, damage);
		ParticlesManager.Instance.GetComponent<PhotonView>().RPC("SpawnParticleNetworked", RpcTarget.All,
			(int)deathParticle, transform.position.x, transform.position.y, transform.position.z);
		PhotonNetwork.Destroy(gameObject); //explode
	}


}
