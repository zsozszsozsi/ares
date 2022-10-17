using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : Projectile
{

    // Update is called once per frame
    void Update()
    {
        if (isInitialized)
        {
            /*if (view.IsMine)
            {
                if (target == null)
                {
                    PhotonNetwork.Destroy(gameObject);
                    return;
                }
                oldPos = transform.position;
                MoveBullet();
                movement = transform.position - oldPos;
            }

            if (!view.IsMine)
            {
                transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * speed);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 100);
                return;
            }*/

            if(turret == null || target == null)
            {
                Destroy(gameObject);
                return;
            }
            MoveBullet();
        }
        
    }

    /// <summary>
    /// Moving the bullet
    /// </summary>
    protected virtual void MoveBullet() { }

    /// <summary>
    /// Damage enemies in the impact zone
    /// </summary>
    /// <param name="pos">impact position, reqiured for aoe turret only</param>
    protected virtual void DamageEnemy(Vector3 pos = default) { }

    /*public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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
    }*/
}
