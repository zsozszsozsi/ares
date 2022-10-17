using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour, IPunObservable
{
    public ParticlesManager.Particles hitParticle = ParticlesManager.Particles.MetalHit;

    public GameObject target;

    public GameObject turret;

    public Vector3 targetStartPos;

    public Transform laserStartPos;

    public float speed = 5f;
    public float damage = 30f;

    public bool isInitialized = false;

    public PhotonView view;

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       //nem kell mindenki localba mozgatja a dolgot
    }

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

    }


    //public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}
