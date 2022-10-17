using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerManager : MonoBehaviour
{

    public Camera mainCamera;

    PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            mainCamera.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
