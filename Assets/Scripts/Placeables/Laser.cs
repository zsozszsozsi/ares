using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Laser : Projectile
{
    public float damagePerSec = 5f;
    public float cooldown = 0.1f; // for damage

    public LineRenderer lineRenderer;

    /// <summary>
    /// Handling damage done, lineRenderer and when target is lost destroy the laser
    /// </summary>
    void Update()
    {
        if (isInitialized)
        {
            if (turret != null)
            {
                target = turret.GetComponent<Turret>().target;

                if (target != null)
                {
                    cooldown -= Time.deltaTime;

                    laserStartPos = turret.GetComponent<Turret>().shootingPoint;
                    lineRenderer.SetPosition(0, laserStartPos.position);
                    lineRenderer.SetPosition(1, target.transform.position);
                    ParticlesManager.Instance.SpawnParticle(hitParticle, target.transform.position);

                    if (view.IsMine)
                    {
                        if (cooldown <= 0f)
                            Damage();
                    }
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                Destroy (gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// Damage enemy
    /// </summary>
    public void Damage()
    {
        cooldown = damagePerSec;

        if (target == null || target.GetPhotonView() == null)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        target.GetPhotonView().RPC("getDamage", RpcTarget.All, damage);
    }
}
