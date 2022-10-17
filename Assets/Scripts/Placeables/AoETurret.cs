using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoETurret : Turret
{
    [PunRPC]
    public override void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.RANGE:
                this.range *= 1.2f;
                break;
            case UpgradeType.DAMAGE:
                this.GetComponent<Bullet>().damage *= 1.2f;
                break;
            case UpgradeType.FIRERATE:
                this.fireRate *= 0.8f;
                break;
            default:
                Debug.LogError("Upgrade valahogy nagyon megbaszodott??");
                break;
        }
    }


    protected override bool LockShootTarget(GameObject target)
    {
        Vector3 dist = target.transform.position - shootingPoint.position;
        Vector3 tmp = shootingPoint.position + dist / 2;
        if (dist.y < 0)
            tmp.y -= dist.y;
        else
            tmp.y += dist.y;


        Vector3 dir = partToRotate.position - tmp;
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * lockSpeed);

        if (cooldown <= 0f)
        {
            if (Quaternion.Angle(partToRotate.rotation, lookRotation) < rotationOffset)
            {
                Shoot(target);
            }
        }

        isTargetLost = true; //mivel ha innen nem ebbe az ágba megy az if akkor elvesztettuk
        last_y = partToRotate.rotation.eulerAngles.y;

        return isTargetLost;
    }

    protected override void IdleRotation()
    {
        if (isTargetLost)
        {
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
        cooldown = fireRate;
        SpawnBullet();
    }

}
