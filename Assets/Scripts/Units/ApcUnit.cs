using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ApcUnit : Unit
{
	public int damage;
	public int range; //when are we close enough to explode

	public Vector3 moveDir = Vector3.forward;

	public PlayerBase targetBase;
	public Vector3 targetBasePos;

	[Tooltip("Temporary helper: should the speed be constant?")]
	public bool normalize = true;

	//shooting variables---------
	public float fireRate = 0.20f;

	public GameObject target;
	public GameObject projectile;

	public float shootRange = 10f; 

	public Transform headRotatePoint;
	public Transform gunRotatePoint;
	public Transform shootingPoint;

	public float cooldown;

	public float lockSpeed = 5f;
	public float rotationOffset = 5f;

	public bool isTargetLost = false;

	public GameObject spawnedBullet;
	//---------------------------------

	public float yPos = 0.2f;

	/// <summary>
	/// Initialize the bullet after instantiate
	/// </summary>
	/// <param name="bulletViewId">the spawned bullet's photon view id</param>
	[PunRPC]
	public void InitializeBullet(int bulletViewId)
	{
		spawnedBullet = PhotonView.Find(bulletViewId).gameObject;

		if (target == null)
		{
			Destroy(spawnedBullet);
			return;
			// ha inicializálás kozben meghalt az enemy vagy valami?
		}

		spawnedBullet.GetComponent<Projectile>().target = target;
		spawnedBullet.GetComponent<Projectile>().targetStartPos = target.transform.position;
		spawnedBullet.GetComponent<Projectile>().turret = gameObject;
		spawnedBullet.GetComponent<Projectile>().laserStartPos = shootingPoint.transform;

		spawnedBullet.GetComponent<Projectile>().transform.LookAt(target.transform); // for trace projectile rotation

		spawnedBullet.GetComponent<Projectile>().isInitialized = true;
	}

	/// <summary>
	/// Spawning the bullet, then initialize it's variables
	/// </summary>
	protected void SpawnBullet()
	{
		spawnedBullet = PhotonNetwork.Instantiate(projectile.name, shootingPoint.position, Quaternion.identity);
		view.RPC("InitializeBullet", RpcTarget.All, spawnedBullet.GetPhotonView().ViewID);
	}

	public override void FindClosestTurret()
    {

		GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyName);
		float shortestDistance = Mathf.Infinity;
		GameObject closestEnemy = null;

		foreach (GameObject enemy in enemies)
		{
			float dist = Vector3.Distance(transform.position, enemy.transform.position);
			if (dist < shortestDistance)
			{
				shortestDistance = dist;
				closestEnemy = enemy;
			}
		}

		if (shortestDistance <= shootRange)
		{
			Vector3 end = Vector3.Normalize(closestEnemy.transform.position - transform.position);

			Ray ray = new Ray(transform.position, end);

			RaycastHit hit;

			if (!Physics.Raycast(ray))
				target = closestEnemy;
			else
			{
				if (Physics.Raycast(ray, out hit))
				{
					if (hit.transform.gameObject.tag != "BlockedTower")
					{
						Debug.Log(hit.transform.gameObject.tag);
						target = closestEnemy;
					}
				}
			}
		}
		else
		{
			target = null;
		}

	}

	public void LockShootTarget()
    {
		Vector3 dirHead = target.transform.position - headRotatePoint.position;
		Quaternion lookRotationHead = Quaternion.LookRotation(dirHead);

		headRotatePoint.rotation = Quaternion.Lerp(headRotatePoint.rotation, Quaternion.Euler(0f,lookRotationHead.eulerAngles.y,0f), Time.deltaTime*lockSpeed);
        if (Quaternion.Angle(headRotatePoint.rotation, Quaternion.Euler(0f, lookRotationHead.eulerAngles.y, 0f)) < rotationOffset)
        {
			Vector3 targetPosGun = gunRotatePoint.InverseTransformPoint(target.transform.position);
			float gunAngleFinal = -Mathf.Atan2(targetPosGun.y, targetPosGun.z) * Mathf.Rad2Deg;
			float gunAngleLimited = Mathf.Clamp(gunAngleFinal, -10, 3);//le 3, fel -10

			gunRotatePoint.localRotation = Quaternion.Lerp(gunRotatePoint.localRotation, Quaternion.Euler(gunAngleLimited, 0f, 0f), lockSpeed * Time.deltaTime);
			
			if (Quaternion.Angle(gunRotatePoint.localRotation, Quaternion.Euler(gunAngleLimited, 0f, 0f)) < rotationOffset)
			{

				Shoot();
			}

		}

		isTargetLost = true;
	}

	public void Shoot()
    {
		cooldown = fireRate;
		SpawnBullet();
	}

	public void IdleRotation()
    {
        if (isTargetLost)
        {
			headRotatePoint.rotation = Quaternion.Lerp(headRotatePoint.rotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * lockSpeed);
			gunRotatePoint.localRotation = Quaternion.Lerp(gunRotatePoint.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * lockSpeed);

			if (Quaternion.Angle(headRotatePoint.rotation, Quaternion.Euler(0f, 0f, 0f)) < 0.05f &&
				Quaternion.Angle(gunRotatePoint.localRotation, Quaternion.Euler(0f, 0f, 0f)) < 0.05f)
			{
				isTargetLost = false;
			}
		}
    }

	private void Update()
	{
		//only controll this unit if we are the owner - otherwise photon transformView handles the networking
		if (view.IsMine)
		{
			oldPosition = transform.position; //network miatt

			cooldown -= Time.deltaTime;

			if (trans.position.y != yPos)
			{
				trans.position = new Vector3(trans.position.x, yPos, trans.position.z);
			}

			if (target != null)
            {
				if(cooldown <= 0f)
                {
					LockShootTarget();
                }
            }
            else
            {
				CheckPathfindingTarget();

				Move();

				IdleRotation();

				if (Vector3.Distance(trans.position, targetBasePos) < range)
				{
					AttackBase();
				}
            }

			movement = transform.position - oldPosition;
		}

        if (!view.IsMine)
        {
			transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * moveSpeed);
			headRotatePoint.rotation = Quaternion.Lerp(headRotatePoint.rotation, networkHeadRotation, Time.deltaTime * lockSpeed);
			gunRotatePoint.rotation = Quaternion.Lerp(gunRotatePoint.rotation, networkGunRotation, Time.deltaTime * lockSpeed);
			//a rotate towards nagyon lassu
			return;
		}
	}

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
			stream.SendNext(transform.position);
			stream.SendNext(headRotatePoint.rotation);
			stream.SendNext(gunRotatePoint.rotation);
			stream.SendNext(movement);
        }
        else
        {
			networkPosition = (Vector3)stream.ReceiveNext();
			networkHeadRotation = (Quaternion)stream.ReceiveNext();
			networkGunRotation = (Quaternion)stream.ReceiveNext();
			movement = (Vector3)stream.ReceiveNext();
        }
    }

    public override void Move()
	{
		moveDir = nextPathfindingTargetPos - trans.position;
		moveDir.y = 0; //dont move up/down

		//if (moveDir.magnitude > range && normalize) //if still out of range
			moveDir.Normalize(); //TEMP, allows it to slow down when near
								 //but also gives it jankier movement

		transform.Translate(moveDir * moveSpeed * Time.deltaTime);
		//transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), lockSpeed * Time.deltaTime);
	}


	[PunRPC]
	public override void Place(int plNum)
	{
		base.Place(plNum);

		if (owner == GameManager.Faction.Earth)
		{
			targetBase = GameManager.Instance.marsBase;
			if (plNum == 1)
				Debug.LogWarning("Spawning the enemy's unit!");

		}
		else if (owner == GameManager.Faction.Mars)
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
