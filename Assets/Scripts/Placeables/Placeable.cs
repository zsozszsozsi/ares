using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeable : MonoBehaviour
{
    public float health = 100;
    public int price = 50; //TODO getters maybe? public might be dumb, it probably is dumb

    protected int gridPosX, gridPosY;
    protected Cell myCell;
    protected int ownerNum; //player index


    protected PhotonView view;
    protected Transform trans;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        view = GetComponent<PhotonView>();
        trans = GetComponent<Transform>();
    }

    /// <summary>
    /// Place a placeable object on the grid
    /// </summary>
    /// <param name="gridX">Grid position on x-axis</param>
    /// <param name="gridY">Grid position on y-axis</param>
    /// <param name="plNum">Grid position on z-axis</param>
    [PunRPC]
	public void Place(int gridX, int gridY, int plNum)
	{
        this.gridPosX = gridX;
        this.gridPosX = gridX;
        this.ownerNum = plNum;
        this.myCell = Grid.Instance.getCellAt(gridX, gridY);
        Grid.Instance.RegisterTurret(gridX, gridY, this.gameObject);
    }

    [PunRPC]
    public void getDamage(float amount)
    {
        
        if (health <= amount) //if we die
        {
            //death/explosion particle goes here

            //PhotonNetwork.Destroy can only be called by the owner
            if (view.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            ParticlesManager.Instance.SpawnParticle(ParticlesManager.Particles.ExplosionMed, trans.position);
        }
        else
        {
            health -= amount;
            ParticlesManager.Instance.SpawnParticle(ParticlesManager.Particles.ExplosionSmall, trans.position);
        }
    }

    public int GetPrice()
    {
        return price;
    }

    /// <summary>
    /// Called on each client when the Component is destroyed.
    /// </summary>
	private void OnDestroy()
	{
        myCell.ChangeStateToOriginal();
        PathFinding.Instance.FindConnections();
	}
}
