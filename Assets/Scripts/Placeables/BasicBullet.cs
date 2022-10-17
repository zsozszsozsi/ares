using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BasicBullet : Bullet
{
    /// <summary>
    /// Moving the bullet to the enemy
    /// </summary>
    protected override void MoveBullet()
    {
        Vector3 dir = target.transform.position - transform.position;
        float dist = dir.magnitude;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dist <= distanceThisFrame)
        {
            if (view.IsMine) 
            {
                DamageEnemy();
            }
            ParticlesManager.Instance.SpawnParticle(hitParticle, transform.position);
            Destroy(gameObject);
            /*DamageEnemy();
            PhotonNetwork.Destroy(gameObject);*/
        }

        transform.Translate(Vector3.Normalize(dir) * distanceThisFrame, Space.World);
        transform.LookAt(target.transform);
    }

    protected override void DamageEnemy(Vector3 pos = default)
    {
        if (target == null || target.GetPhotonView() == null)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }
        target.GetPhotonView().RPC("getDamage", RpcTarget.All, damage);
    }
}
