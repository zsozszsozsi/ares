using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;

public class Turret : Placeable, IPunObservable
{
	public float rotateSpeed;
	public float lockSpeed;
	public float rotationOffset;
	public float range;
	public float fireRate;
	public float cooldown;

	public int dmgLevel = 1;
	public int fireRateLevel = 1;
	public int rangeLevel = 1;

	public bool isTargetLost = false;

	protected float last_y = 0;
	protected string enemyName = ""; //set in start()

	public bool drawDebugVisuals = false;

	public GameObject spawnedBullet;

	public GameObject bullet;
	public GameObject target;

	public ParticlesManager.Particles muzzleFlashParticle = ParticlesManager.Particles.Muzzleflash;

	public Transform partToRotate;
	public Transform shootingPoint;

	public Quaternion networkRotation;

	public enum UpgradeType { RANGE, FIRERATE, DAMAGE };

	protected override void Start()
	{
		base.Start();
		InvokeRepeating("FindClosestEnemy", 0f, .5f); // every 0.5 sec call FindClosestEnemy()
		enemyName = ownerNum == 0 ? "MarsEnemy" : "EarthEnemy";
	}

	protected virtual void Update()
	{
		//only rotate if my own - the PhotonTransform will send the result to the others
		if (view.IsMine)
		{
			Timer();

			if (target != null)
			{
				isTargetLost = LockShootTarget(target);
                
			}
			else
			{
				IdleRotation();
			}
		}

        if (!view.IsMine)
        {
			partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, networkRotation, Time.deltaTime * rotateSpeed);
			return;
		}
	}

	/// <summary>
	/// Called in Start() by InvokeRepeating()
	/// </summary>
	public virtual void FindClosestEnemy()
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
		
		if (shortestDistance <= range)
		{
			Vector3 end = Vector3.Normalize(closestEnemy.transform.position - transform.position);

			Ray ray = new Ray(transform.position, end);
			Debug.DrawRay(transform.position, end);

			RaycastHit hit;

			if (!Physics.Raycast(ray))
				target = closestEnemy;
            else
            {
				if(Physics.Raycast(ray, out hit))
                {
					if(hit.transform.gameObject.tag != "BlockedTower")
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

	/// <summary>
	/// Initialize the bullet after instantiate
	/// </summary>
	/// <param name="bulletViewId">the spawned bullet's photon view id</param>
	[PunRPC]
	public void InitializeBullet(int bulletViewId)
	{
		spawnedBullet = PhotonView.Find(bulletViewId).gameObject;
		
		if(target == null)
        {
			Destroy(spawnedBullet);
			return;
			// ha inicializ�l�s kozben meghalt az enemy vagy valami?
        }

		spawnedBullet.GetComponent<Projectile>().target = target;
		spawnedBullet.GetComponent<Projectile>().targetStartPos = target.transform.position;
		spawnedBullet.GetComponent<Projectile>().turret = gameObject;
		spawnedBullet.GetComponent<Projectile>().laserStartPos = shootingPoint;

		spawnedBullet.GetComponent<Projectile>().isInitialized = true;
	}

	/// <summary>
	/// Spawning the bullet, then initialize it's variables
	/// </summary>
	protected void SpawnBullet()
	{
		spawnedBullet = PhotonNetwork.Instantiate(bullet.name, shootingPoint.position, Quaternion.identity);
		view.RPC("InitializeBullet", RpcTarget.All, spawnedBullet.GetPhotonView().ViewID);
		ParticlesManager.Instance.SpawnParticle(muzzleFlashParticle, shootingPoint.position);
	}

	/// <summary>
	/// Mouse input handling
	/// </summary>
	private void OnMouseDown()
	{
		if (EventSystem.current.IsPointerOverGameObject())
		{
			Debug.LogWarning("Pressing cell through a UI element");
			return;
		}

		//Toggels the Turret Upgrade UI for this Turret
		if (view.IsMine)
			GameUIManager.Instance.ToggleTurretUpgradeUI(this);
	}

	/// <summary>
	/// only for debug 
	/// </summary>
	void OnDrawGizmos()
	{
		if (drawDebugVisuals)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, range);
		}
	}
	/// <summary>
	/// Upgrades the turret's attributes
	/// </summary>
	[PunRPC]
    public virtual void Upgrade(UpgradeType type) { }

	/// <summary>
	/// Timer for handling firerate
	/// </summary>
	protected virtual void Timer() { cooldown -= Time.deltaTime; }

	/// <summary>
	/// When target == null, then just rotating around
	/// </summary>
	protected virtual void IdleRotation() { }

	/// <summary>
	/// Locking on the target, and shooting
	/// </summary>
	/// <param name="target">target of the turret</param>
	/// <returns></returns>
	protected virtual bool LockShootTarget(GameObject target) { return false; }

	/// <summary>
	/// Call the SpawnBullet(), and shoot the target
	/// </summary>
	/// <param name="target">target of the turret</param>
	protected virtual void Shoot(GameObject target) {}
	
	/// <summary>
	/// Networking:
	/// Sendind and reciving rotation
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="info"></param>
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
		if (stream.IsWriting)
		{
			stream.SendNext(partToRotate.rotation);
		}
		else
		{
			networkRotation = (Quaternion)stream.ReceiveNext();
		}
	}

	public bool isBlocked()
	{
		Ray ray = new Ray(trans.position, trans.forward);
		RaycastHit hitData;
		return Physics.Raycast(ray, out hitData);
	}
}
