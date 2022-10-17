using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoEBullet : Bullet
{
    public float range = 2f;

    //Quadratic Bezier Curve
    protected Vector3 pos0; // bullet spawn point
    protected Vector3 pos1; // middle up point
    protected Vector3 pos2; // target point

    private bool foundEnemy = false;

    /// <summary>
    /// Setup variables, and calculate bezier curve points
    /// </summary>
    private void SetupBerzierCurve()
    {
        //Quadratic Bezier Curve/////////////

        pos0 = transform.position;

        pos2 = targetStartPos;

        Vector3 dist = pos2 - pos0;
        Vector3 tmp = pos0 + dist / 2;
        if (dist.y < 0)
            tmp.y -= dist.y;
        else
            tmp.y += dist.y;
        pos1 = tmp;

        /////////////////////////////////////
    }

    /// <summary>
    /// Calculating points for the bezier curve from 3 points and
    /// 1 time variable
    /// </summary>
    /// <param name="t">time</param>
    /// <param name="p0">point 0</param>
    /// <param name="p1">point 1</param>
    /// <param name="p2">point 2</param>
    /// <returns></returns>
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        t = Mathf.Clamp01(t); // clamp between 0 and 1

        //B(t) = (1-t)^2*P_0 + 2*(1-t)*t*P_1 + t^2*P_2 , 0 < t < 1

        float u = (1 - t);
        float u_2 = u * u;
        float t_2 = t * t;

        Vector3 p = u_2 * p0;
        p += 2 * u * t * p1;
        p += t_2 * p2;

        return p;
    }


    float t = 0f;
    float estTime = 1f; // nem számit mivel 0-t fog osztani aztán be lesz állítva az értéke rendesen!
    float speedOffset = 8f;
    float time = 0f;

    /// <summary>
    /// Moving the bullet on the calculated bezier points 
    /// </summary>
    protected override void MoveBullet()
    {
        if (!foundEnemy)
        {
            SetupBerzierCurve();
        }

        Vector3 dir = CalculateBezierPoint(t / estTime, pos0, pos1, pos2) - transform.position;
        float dist = (targetStartPos - transform.position).magnitude;
        float distanceThisFrame = speed * Time.fixedDeltaTime;

        if(!foundEnemy)
        {
            estTime = dist; //initial distance between target and bullet start pos
            estTime /= distanceThisFrame*speedOffset;
            time = estTime;
        }
        else
        {
            estTime /= distanceThisFrame * speedOffset;

            estTime = Mathf.Clamp(estTime, time - 0.25f, time + 0.25f); 
            // max eltérés az eredetihez képest +-0.25f lehet, ha mondjuk hirtelen leesne a frame vagy megszaladna
        }

        foundEnemy = true;

        if (dist <= distanceThisFrame)
        {
            /*DamageEnemy(transform.position);
            PhotonNetwork.Destroy(gameObject);*/
            if (view.IsMine)
            {
                DamageEnemy(transform.position);
            }
            Destroy(gameObject);
        }

        transform.position = Vector3.Lerp(transform.position, CalculateBezierPoint(t / estTime, pos0, pos1, pos2), distanceThisFrame);
        t += Time.deltaTime;

        estTime = time * distanceThisFrame * speedOffset; // back to original and calculate with new time.deltaTime
    }
    protected override void DamageEnemy(Vector3 pos = default)
    {
        if (target == null || target.GetPhotonView() == null)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(target.tag);

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(pos, enemy.transform.position);

            if (dist <= range && enemy != null && enemy.GetPhotonView() != null)
            {
                enemy.GetPhotonView().RPC("getDamage", RpcTarget.All, damage);
            }
        }
    }

}
