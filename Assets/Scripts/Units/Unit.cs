using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IPunObservable
{
    //TODO: how to import GameManager in c#..?
    public GameManager.Faction owner;

    [field: SerializeField]
    public float health { get; protected set; } = 100;

    [field: SerializeField]
    public float maxHealth { get; protected set; } = 100;

    [field: SerializeField]
    public int price { get; protected set; } = 50;

    public float moveSpeed = 5;

    protected PhotonView view;
    protected Transform trans;

    public string enemyName;

    public float distToTarg;

    private PathFinding pathFinder;

    public Cell nextPathfindingTargetCell;
    public Vector3 nextPathfindingTargetPos;

    [Tooltip("How much % we have to be in a cell before targeting the next cell in the path")]
    [Range(0f, 1f)]
    public float pathFindingCellRangeModifier;
    private float pathFindingCellRange;

    public ParticlesManager.Particles deathParticle = ParticlesManager.Particles.ExplosionSmall;

    /// Networking variables\\\
    public Vector3 networkPosition;
    public Quaternion networkRotation;
    public Vector3 oldPosition;
    public Vector3 movement;

    public Quaternion networkHeadRotation;
    public Quaternion networkGunRotation;
    ///---------------\\\


    // Start is called before the first frame update
    void Start()
    {
        enemyName = owner == GameManager.Faction.Earth ? "MarsTurret" : "EarthTurret"; 

        view = GetComponent<PhotonView>();
        trans = GetComponent<Transform>();

        if (pathFindingCellRangeModifier == 0)
            Debug.LogError("Pathfinding cell range modifier set to null!");

        pathFindingCellRange = Grid.Instance.cellSize.x * pathFindingCellRangeModifier;


        pathFinder = PathFinding.Instance;
        nextPathfindingTargetCell = pathFinder.getStartingTargetCell();
        nextPathfindingTargetPos = nextPathfindingTargetCell.transform.position;
        nextPathfindingTargetPos.y = trans.position.y; //ignore height

        InvokeRepeating("FindClosestTurret", 0f, .5f); //every 0.5 sec call FindClosestTurret()
    }

    /// <summary>
    /// Sending/Receiving position rotation
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(movement);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            movement = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (movement * lag);
        }
    }

    public void CheckPathfindingTarget()
    {
        //distToTarg = Vector3.Distance(trans.position, nextPathfindingTargetPos);
        Vector3 dirToTarg = trans.position - nextPathfindingTargetPos;
        dirToTarg.y = 0;
        distToTarg = dirToTarg.magnitude;

        if (distToTarg < pathFindingCellRange)
        {
            nextPathfindingTargetCell = pathFinder.getNextTarget(nextPathfindingTargetCell);
            if (nextPathfindingTargetCell == null)
            {
                Debug.LogError("Next target cell is null!");
                gameObject.SetActive(false);
            }
            nextPathfindingTargetPos = nextPathfindingTargetCell.transform.position;
            nextPathfindingTargetPos.y = trans.position.y; //ignore height
        }
    }

    public abstract void Move();

    /// <summary>
    /// Finding closest turret, only for apc unit
    /// </summary>
    public virtual void FindClosestTurret() { }


    //not even sure if we need this...?
    //TODO: replace with start...?
    [PunRPC]
    public virtual void Place(int plNum)
    {
        health = maxHealth;
    }

    // if we have seperate logic for damaging infantry groups, override this
    [PunRPC]
    public virtual void getDamage(float amount)
	{
        if (health <= amount) //if we die
		{
            ParticlesManager.Instance.SpawnParticle(deathParticle, trans.position);

            //PhotonNetwork.Destroy can only be called by the owner
            if (view.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
		}
		else
		{
            health -= amount;
            //TODO: visuals
		}
	}
}
