using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class WaitingRoom : MonoBehaviourPunCallbacks
{
    public Text roomName;


    public bool player1Joined = false;
    public bool player2Joined = false;

    public bool isHost = false;

    public Button startGameBtn;

    public static int player1Faction = -1;
    public static int player2Faction = -1;

    public Button faction1Selector;
    public Button faction2Selector;

    public Image f1Img;
    public Image f2Img;

    private Color bg;



    PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        bg.r = 254;
        bg.g = 250;
        bg.b = 224;
        bg.a = 1;
        view = GetComponent<PhotonView>();
        f1Img = faction1Selector.GetComponent<Image>();
        f2Img = faction2Selector.GetComponent<Image>();

        roomName.text = "Room name: " + PhotonNetwork.CurrentRoom.Name;

        if (PhotonNetwork.IsMasterClient)
        {
            view.RPC("loadPlayer", RpcTarget.All, 1);

            isHost = true;
        }
        else
        {
            view.RPC("loadPlayer", RpcTarget.All, 2);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isHost)
            startGameBtn.interactable = true;
            //startGameBtn.interactable = (player1Faction != -1) && (player2Faction != -1);
        else
            startGameBtn.gameObject.SetActive(false);
    }

    public void Faction1Selected()
    {
        if(view.IsMine)
        {
            view.RPC("FactionSelectRPC", RpcTarget.OthersBuffered, 1, 1);
            player1Faction = 1;
            f1Img.color = Color.cyan;
            f2Img.color = bg;
        }
        else
        {
            view.RPC("FactionSelectRPC", RpcTarget.OthersBuffered, 1, 2);
            player2Faction = 1;
            f1Img.color = Color.cyan;
            f2Img.color = bg;
        }
    }

    public void Faction2Selected()
    {
        if (view.IsMine)
        {
            view.RPC("FactionSelectRPC", RpcTarget.OthersBuffered, 2, 1);
            f2Img.color = Color.red;
            f1Img.color = bg;
            player1Faction = 2;
        }
        else
        {
            view.RPC("FactionSelectRPC", RpcTarget.OthersBuffered, 2, 2);
            f2Img.color = Color.red;
            f1Img.color = bg;
            player2Faction = 2;
        }
    }

    [PunRPC]
    public void FactionSelectRPC(int faction, int player)
    {
        if(faction == 1)
        {
            faction1Selector.interactable = false;
            faction2Selector.interactable = true;

        }
        else
        {
            faction2Selector.interactable = false;
            faction1Selector.interactable = true;

        }

        if(player == 1)
        {
            player1Faction = faction;
        }
        else
        {
            player2Faction = faction;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Game_new");
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
    }

    public void ReadyPlayer(int player)
    {
        view.RPC("ReadyPlayerRPC", RpcTarget.All, player);
    }


    [PunRPC]
    public void loadPlayer(int player)
    {
        if(player == 1)
        {
            player1Joined = true;
        }
        else
        {
            player2Joined = true;
            
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(isHost)//client lépett ki || mivel ha te vagy a host akkor a kliens lépett ki, ha a host lép ki akkor átadja az isHost változot
        {
            view.RPC("resetPlayerUI", RpcTarget.All, 1);
        }
        else//master lépett ki
        {
            view.RPC("resetPlayerUI", RpcTarget.All, 0);
            isHost = true;
        }
    }

}
