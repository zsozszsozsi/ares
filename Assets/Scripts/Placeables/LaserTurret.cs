using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class LaserTurret : Turret
{
    bool activeLaser = false;

    

    [PunRPC]
    public override void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.RANGE:
                this.range *= 1.2f;
                break;
            case UpgradeType.DAMAGE:
                this.GetComponent<Laser>().damagePerSec *= 1.2f;
                break;
            case UpgradeType.FIRERATE:
                this.GetComponent<Laser>().cooldown *= 0.8f;
                break;
            default:
                Debug.LogError("Upgrade valahogy nagyon megbaszodott??");
                break;
        }
    }


    protected override bool LockShootTarget(GameObject target)
    {
        Vector3 dir = partToRotate.position - target.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * lockSpeed);

        if (!activeLaser)
        {
            if (Quaternion.Angle(partToRotate.rotation, lookRotation) < rotationOffset)
            {
                Shoot(target);
            }
        }
        /*else
        {
            view.RPC("UpdateLaser", RpcTarget.All);
        }*/

        isTargetLost = true; //mivel ha innen nem ebbe az ágba megy az if akkor elvesztettuk
        last_y = partToRotate.rotation.eulerAngles.y;

        return isTargetLost;
    }

    protected override void IdleRotation()
    {
        if (isTargetLost)
        {
            activeLaser = false;

            partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, Quaternion.Euler(0f, last_y, 0f), Time.deltaTime * rotateSpeed);

            if (Quaternion.Angle(partToRotate.rotation, Quaternion.Euler(0f, last_y, 0f)) < 0.05f)
            {
                isTargetLost = false;
            }
        }
        else
        {
            last_y += rotateSpeed * Time.deltaTime;
            partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, Quaternion.Euler(0f, last_y, 0f), Time.deltaTime * rotateSpeed);
        }
    }

    protected override void Shoot(GameObject target)
    {
        activeLaser = true;
        SpawnBullet();
    }

    /*[PunRPC]
    public void UpdateLaser()
    {
        //TODO: miért dob errort ha nem nullcheckelünk?
        if(spawnedBullet)
        spawnedBullet.GetComponent<Laser>().laserStartPos = shootingPoint;
    }*/
}
