using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //this is what the units/bases use
    public enum Faction { Earth, Mars };

    //Make this script a singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public GameObject playerPrefab;

    [Tooltip("Where to spawn the first player. Also will spawn with this rotation.")]
    public Transform player1Position;

    [Tooltip("Where to spawn the second player. Also will spawn with this rotation.")]
    public Transform player2Position;

    public bool isGameOver;

    public int targetFrameRate = 60;

    //TODO: proper naming & getters
    public PlayerBase earthBase;
    public PlayerBase marsBase;

    public Faction myFaction;

    PhotonView view;


    //private GameObject player1;
    //private GameObject player2;

    void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("More than one GameManager in scene!");
            return;
        }
        _instance = this;
        isGameOver = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (earthBase == null || marsBase == null)
            Debug.LogError("Player base not set!");

        if (PhotonNetwork.IsConnected)
        {
            view = GetComponent<PhotonView>();
            if (view.IsMine)
            {
                myFaction = WaitingRoom.player1Faction == 1 ? Faction.Earth : Faction.Mars;
            }
            else
            {
                myFaction = WaitingRoom.player2Faction == 1 ? Faction.Earth : Faction.Mars;
            }
            
            

            if (myFaction == Faction.Earth)
            {
                PhotonNetwork.Instantiate(playerPrefab.name, player1Position.position, player1Position.rotation);
            }
            else
            {
                PhotonNetwork.Instantiate(playerPrefab.name, player2Position.position, player2Position.rotation);
            }
		}
		else //if we are opening the game scene directly - ie debugging in editor - open in offline mode
		{
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
            PhotonNetwork.Instantiate(playerPrefab.name, player1Position.position, player1Position.rotation);

        }
        
        //capping to 30 fps for now so it doesnt eat my laptop battery in the editor
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;

        //myFaction = (PhotonNetwork.IsMasterClient) ? Faction.Earth : Faction.Mars;
        PathFinding.Instance.Initialize(myFaction);
        PlayerController.Instance.Initialize(myFaction);

        
    }

    [PunRPC]
    public void GameOver(Faction loser)
    {
        //TODO: call GameUIManager.Instance.GameOver(owner) with RPC
        GameUIManager.Instance.GetComponent<PhotonView>().RPC("ActivateEndScreen", RpcTarget.All, loser);
        //Debug.Log("Faction" + loser + " lost.");
        isGameOver = true;
    }

    public void ExitGames()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }
}
