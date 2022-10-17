using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public GameManager.Faction owner;

    [field: SerializeField]
    public int health { get; protected set; } = 100;

    [field: SerializeField]
    public int maxHealth { get; protected set; } = 100;

    protected PhotonView view;
    protected Transform trans;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        trans = GetComponent<Transform>();
        health = maxHealth;
    }

    [PunRPC]
    public virtual void getDamage(int amount)
    {
        if (health < amount) //if we die
        {
            ParticlesManager.Instance.SpawnParticle(ParticlesManager.Particles.ExplosionBig, trans.position);

            if (GameManager.Instance.isGameOver == false)
            {
                GameManager.Instance.GetComponent<PhotonView>().RPC("GameOver", RpcTarget.All, owner);

                //view.RPC("GameManager.Instance.GameOver", RpcTarget.All, owner);
                // TODO: call GameManager.Instance.GameOver() with RPC
            }

        }
        else
        {
            health -= amount;
            //TODO: visuals
        }
    }
}
