using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    public TMP_InputField hostName;
    public TMP_InputField roomName;


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void Host()
    {
        PhotonNetwork.CreateRoom(hostName.text);
        
    }

    public void Join()
    {
        PhotonNetwork.JoinRoom(roomName.text);
    }

    public void LeaveMultiplayer()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("MainMenu");
    }

    public override void OnJoinedRoom()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers = 2;

            PhotonNetwork.LoadLevel("WaitingRoom");
        }

    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // valami popupba ki kell irni hogy mi volt a hiba.

        Debug.Log(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // valami popupba ki kell irni hogy mi volt a hiba.

        Debug.Log(message);
    }

}
