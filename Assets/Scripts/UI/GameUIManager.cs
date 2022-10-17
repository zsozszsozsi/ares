using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Photon.Pun;
using UnityEngine.EventSystems;

public class GameUIManager : MonoBehaviour

{
    // (Start is called before the first frame update <- do not disturb his vibe)

    public static GameUIManager Instance;
    UIDocument doc;
    VisualElement root;
    PhotonView photonView;

    // In Game UI
    enum SideMenu {None, UnitShop, UpgradeShop, TowerShop }
    SideMenu currentSideMenu;

    Button upgradeShopBtn;

    Button unitShopBtn;
    Button unit1BuyBtn;
    Button unit2BuyBtn;
    Button unit3BuyBtn;
    Button leftLaneBtn;
    Button rightLaneBtn;
    Button midLaneBtn;

    Label laneSelectLabel;

    Button tower1BuyBtn;
    Button tower2BuyBtn;
    Button tower3BuyBtn;
    Button tower4BuyBtn;
    Button towerShopBtn;

    Label moneyLabel;

    VisualElement unitShop;
    VisualElement upgradeShop;
    VisualElement towerShop;
    VisualElement inGameUI;
    VisualElement turretUpgradeUI;

    //Game Over UI
    VisualElement endGameUI;
    Button exitBtn;
    Button initRematchBtn;
    Label winnerLabel;

    //Pause Menu UI
    VisualElement pauseMenuUI;
    Button settingsBtn;
    Button surrenderBtn;

    //Upgrade UI
    Turret selectedTurret;

    Button dmgUpgrade;
    Button fireRateUpgrade;
    Button rangeUpgrade;
    Button sellBtn;

    Label dmgLabel;
    Label fireRateLabel;
    Label rangeLabel;
    Label sellLabel;

    //Alert system
    VisualElement bottomLeft;

    //Rematch system
    VisualElement rematchMenu;
    Button rematchAcceptBtn;
    Button rematchDeclineBtn;

    void Start()
    {
        Instance = this;

        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;
        inGameUI = root.Q<VisualElement>("InGameUI");
        currentSideMenu = SideMenu.None;
        photonView = GetComponent<PhotonView>();

        InitializeBaseGameUI();
        InitializePauseMenuUI();
        InitializeUpgradeUI();
        InitializeEndGameUI();
        InitializeRematchUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(3) || Input.mouseScrollDelta.y != 0)
        {
            ToggleTurretUpgradeUI(null);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleTurretUpgradeUI(null);
            TogglePauseMenu();
        }
    }
    private void InitializeBaseGameUI()
    {
        //---BASE GAME UI INITIALIZATION---//

        //Unit Shop
        unitShop = root.Q<VisualElement>("UnitShop");
        unitShopBtn = root.Q<Button>("UnitShopBtn");

        unitShopBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            if (currentSideMenu == SideMenu.UnitShop)
            {
                CloseCurretMenu();
            }
            else
            {
                CloseCurretMenu();

                unitShop.style.display = DisplayStyle.Flex;
                currentSideMenu = SideMenu.UnitShop;
            }
        });

        unit1BuyBtn = unitShop.Q<Button>("Unit1BuyBtn");
        unit1BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildUnit(0);
        });

        unit2BuyBtn = unitShop.Q<Button>("Unit2BuyBtn");
        unit2BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildUnit(1);
        });

        unit3BuyBtn = unitShop.Q<Button>("Unit3BuyBtn");
        unit3BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildUnit(2);
        });

        laneSelectLabel = unitShop.Q<Label>("SelectedLaneLabel");
        laneSelectLabel.text = "Selected Lane: Left";

        leftLaneBtn = unitShop.Q<Button>("LeftLaneBtn");
        leftLaneBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.ChangeLane(0);
            laneSelectLabel.text = "Selected Lane: Left";
        });

        midLaneBtn = unitShop.Q<Button>("MidLaneBtn");
        midLaneBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.ChangeLane(1);
            laneSelectLabel.text = "Selected Lane: Middle";
        });

        rightLaneBtn = unitShop.Q<Button>("RightLaneBtn");
        rightLaneBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.ChangeLane(2);
            laneSelectLabel.text = "Selected Lane: Right";
        });




        //Tower Shop
        towerShop = root.Q<VisualElement>("TowerShop");
        towerShopBtn = root.Q<Button>("TowerShopBtn");

        towerShopBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            if (currentSideMenu == SideMenu.TowerShop)
            {
                CloseCurretMenu();
            }
            else
            {
                CloseCurretMenu();

                towerShop.style.display = DisplayStyle.Flex;
                currentSideMenu = SideMenu.TowerShop;
            }
            //
        });

        tower1BuyBtn = towerShop.Q<Button>("Tower1BuyBtn");
        tower1BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildTower(0);
        });

        tower2BuyBtn = towerShop.Q<Button>("Tower2BuyBtn");
        tower2BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildTower(1);
        });

        tower3BuyBtn = towerShop.Q<Button>("Tower3BuyBtn");
        tower3BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildTower(2);
        });

        tower4BuyBtn = towerShop.Q<Button>("Tower4BuyBtn");
        tower4BuyBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            PlayerController.Instance.BuildTower(3);
        });

        //Resources
        moneyLabel = root.Q<Label>("MoneyLabel");
        moneyLabel.text = "Kredits: 50";

        //Alerts
        bottomLeft = root.Q<VisualElement>("BottomLeft");
    }

    private void InitializeUpgradeUI()
    {
        //---UPGRADE UI INITIALIZATION---//

        turretUpgradeUI = root.Q<VisualElement>("TurretUpgradeUI");
        turretUpgradeUI.style.display = DisplayStyle.None;
        turretUpgradeUI.style.width = Length.Percent(25);
        turretUpgradeUI.style.height = Length.Percent(25);

        dmgUpgrade = turretUpgradeUI.Q<Button>("DMG_U_BTN");
        fireRateUpgrade = turretUpgradeUI.Q<Button>("FR_U_BTN");
        rangeUpgrade = turretUpgradeUI.Q<Button>("R_U_BTN");

        dmgLabel = turretUpgradeUI.Q<Label>("DMG_Label");
        fireRateLabel = turretUpgradeUI.Q<Label>("FR_Label");
        rangeLabel = turretUpgradeUI.Q<Label>("R_Label");

        //TODO(later): make this not retarded code repetition
        dmgUpgrade.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            if(PlayerController.Instance.DeductMoney(25))
            {
                selectedTurret.gameObject.GetPhotonView().RPC("Upgrade", RpcTarget.All, Turret.UpgradeType.DAMAGE);
                selectedTurret.dmgLevel++;
                dmgLabel.text = "Damage: lvl " + selectedTurret.dmgLevel;
                if (selectedTurret.dmgLevel == 3)
                {
                    dmgUpgrade.text = "Max";
                    dmgUpgrade.SetEnabled(false);
                }
            }
            else
            {
                SendAlertMessageToUI("Not enough kredits to purchase upgrade!");
            }
        });

        fireRateUpgrade.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            if (PlayerController.Instance.DeductMoney(25))
            {
                selectedTurret.gameObject.GetPhotonView().RPC("Upgrade", RpcTarget.All, Turret.UpgradeType.FIRERATE);
                selectedTurret.fireRateLevel++;
                fireRateLabel.text = "Fire Rate: lvl " + selectedTurret.fireRateLevel;
                if (selectedTurret.fireRateLevel == 3)
                {
                    fireRateUpgrade.text = "Max";
                    fireRateUpgrade.SetEnabled(false);
                }
            }
            else
            {
                SendAlertMessageToUI("Not enough kredits to purchase upgrade!");
            }
        });

        rangeUpgrade.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            if (PlayerController.Instance.DeductMoney(25))
            {
                selectedTurret.gameObject.GetPhotonView().RPC("Upgrade", RpcTarget.All, Turret.UpgradeType.RANGE);
                selectedTurret.rangeLevel++;
                rangeLabel.text = "Range: lvl " + selectedTurret.rangeLevel;
                if (selectedTurret.rangeLevel == 3)
                {
                    rangeUpgrade.text = "Max";
                    rangeUpgrade.SetEnabled(false);
                }
            }
            else
            {
                SendAlertMessageToUI("Not enough kredits to purchase upgrade!");
            }
            
        });

        sellBtn = turretUpgradeUI.Q<Button>("SellBtn");
        sellLabel = turretUpgradeUI.Q<Label>("SellText");
        sellBtn.RegisterCallback<ClickEvent>((ClickEvent evt) => {

            PlayerController.Instance.GainMoney((int)(selectedTurret.GetPrice() / 2));
            PhotonNetwork.Destroy(selectedTurret.gameObject);

            ToggleTurretUpgradeUI(null);
        });
    }

    private void InitializePauseMenuUI()
    {
        //---PAUSE MENU UI INITIALIZATION---//

        pauseMenuUI = root.Q<VisualElement>("PauseMenuUI");
        pauseMenuUI.style.display = DisplayStyle.None;
        settingsBtn = pauseMenuUI.Q<Button>("StgsBtn");
        surrenderBtn = pauseMenuUI.Q<Button>("GiveUpBtn");

        surrenderBtn.RegisterCallback<ClickEvent>((ClickEvent evt) => {
            photonView.RPC("ActivateEndScreen", RpcTarget.All, PlayerController.Instance.myFaction);
        });
    }

    private void InitializeEndGameUI()
    {
        //---GAME OVER UI INITIALIZATION---//

        winnerLabel = root.Q<Label>("Winner");
        endGameUI = root.Q<VisualElement>("EndGameUI");
        endGameUI.style.display = DisplayStyle.None;
        exitBtn = root.Q<Button>("ExitBtn");
        initRematchBtn = endGameUI.Q<Button>("AskForRematchBtn");

        exitBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            GameManager.Instance.ExitGames();
        });

        initRematchBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            photonView.RPC("RematchInitiated", RpcTarget.Others);
        });
    }

    private void InitializeRematchUI()
    {
        //---REMATCH UI INITIALIZATION---//

        rematchMenu = root.Q<VisualElement>("RematchMenuUI");
        rematchAcceptBtn = rematchMenu.Q<Button>("RematchAcceptBtn");
        rematchDeclineBtn = rematchMenu.Q<Button>("RematchDeclineBtn");

        rematchAcceptBtn.RegisterCallback<ClickEvent>((ClickEvent evt) => {

            //It must be the Master Client who handles scene switches because Photon reasons
            photonView.RPC("SendBackToLobby", RpcTarget.MasterClient);
        });
        rematchDeclineBtn.RegisterCallback<ClickEvent>((ClickEvent evt) => {
            rematchMenu.style.display = DisplayStyle.None;
            endGameUI.style.display = DisplayStyle.Flex;
        });
    }
    
    /// <summary>
    /// Activates the tower shop UI, this function is used to do this from outside the designated button
    /// </summary>
    public void ActivateTowerShop()
    {
        if (currentSideMenu != SideMenu.TowerShop)
        {
            CloseCurretMenu();

            towerShop.style.display = DisplayStyle.Flex;
            currentSideMenu = SideMenu.TowerShop;
        }
    }

    /// <summary>
    /// toggles the turret upgrade UI on the mouse position(direction depending on quadrant)
    /// initiates the UIs 'selectedTurret parameter, and updates the UI with it'
    /// </summary>
    /// <param name="_selectedTurret"> the turret we generate the UI for, if 'null' is passed, closes the UI by default</param>
    public void ToggleTurretUpgradeUI(Turret _selectedTurret)
    {
        
        //Debug.Log("Mouse X: " + mousePos.x + "\n" + "Mouse Y: " + mousePos.y + "\n" + Screen.height + " " + Screen.width);

        if ((turretUpgradeUI.style.display == DisplayStyle.None) && _selectedTurret != null)
        {
            selectedTurret = _selectedTurret;
            Vector3 mousePos = Input.mousePosition;

            turretUpgradeUI.style.display = DisplayStyle.Flex;

            if(mousePos.x / Screen.width * 100 <= 50 && mousePos.y / Screen.height * 100 > 50)
            {
                //TOP LEFT
                turretUpgradeUI.style.left = Length.Percent(mousePos.x / Screen.width * 100);
                turretUpgradeUI.style.top = Length.Percent(100 - (mousePos.y / Screen.height * 100));
            }
            else if(mousePos.x / Screen.width * 100 > 50 && mousePos.y / Screen.height * 100 > 50)
            {
                //TOP RIGHT
                turretUpgradeUI.style.left = Length.Percent((mousePos.x / Screen.width * 100) - 25);
                turretUpgradeUI.style.top = Length.Percent(100 - (mousePos.y / Screen.height * 100));
            }
            else if (mousePos.x / Screen.width * 100 <= 50 && mousePos.y / Screen.height * 100 <= 50)
            {
                //BOTTOM LEFT
                turretUpgradeUI.style.left = Length.Percent(mousePos.x / Screen.width * 100);
                turretUpgradeUI.style.top = Length.Percent(100 - (mousePos.y / Screen.height * 100) - 25);
            }
            else if (mousePos.x / Screen.width * 100 > 50 && mousePos.y / Screen.height * 100 <= 50)
            {
                //BOTTOM RIGHT
                turretUpgradeUI.style.left = Length.Percent((mousePos.x / Screen.width * 100) - 25);
                turretUpgradeUI.style.top = Length.Percent(100 - (mousePos.y / Screen.height * 100) - 25);
            }

            this.UpdateUpgradeUI(selectedTurret);
        }
        else
        {
            turretUpgradeUI.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Updates the Turret Upgrade UI with the given turret's data.
    /// </summary>
    /// <param name="selectedTurret"></param>
    private void UpdateUpgradeUI(Turret selectedTurret)
    {
        sellLabel.text = "Sell for " + (selectedTurret.GetPrice() / 2) + "kr";

        dmgLabel.text = "Damage: lvl " + selectedTurret.dmgLevel;
        fireRateLabel.text = "Fire Rate: lvl " + selectedTurret.fireRateLevel;
        rangeLabel.text = "Range: lvl " + selectedTurret.rangeLevel;

        if (selectedTurret.dmgLevel == 3)
        {
            dmgUpgrade.text = "Max";
            dmgUpgrade.SetEnabled(false);
        }
        else
        {
            dmgUpgrade.text = "Buy";
            dmgUpgrade.SetEnabled(true);
        }

        if (selectedTurret.fireRateLevel == 3)
        {
            fireRateUpgrade.text = "Max";
            fireRateUpgrade.SetEnabled(false);
        }
        else
        {
            fireRateUpgrade.text = "Buy";
            fireRateUpgrade.SetEnabled(true);
        }

        if (selectedTurret.rangeLevel == 3)
        {
            rangeUpgrade.text = "Max";
            rangeUpgrade.SetEnabled(false);
        }
        else
        {
            rangeUpgrade.text = "Buy";
            rangeUpgrade.SetEnabled(true);
        }
    }

    /// <summary>
    /// Closes the currently open Side Menu(UpgradeShop,UnitShop,TowerShop).
    /// </summary>
    private void CloseCurretMenu()
    {
        switch(currentSideMenu)
        {
            case SideMenu.None:
                break;
            case SideMenu.UpgradeShop:
                upgradeShop.style.display = DisplayStyle.None;
                currentSideMenu = SideMenu.None;
                break;
            case SideMenu.UnitShop:
                unitShop.style.display = DisplayStyle.None;
                currentSideMenu = SideMenu.None;
                break;
            case SideMenu.TowerShop:
                towerShop.style.display = DisplayStyle.None;
                currentSideMenu = SideMenu.None;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Called when the resource label needs to be updated.
    /// Passes new resources as parameter.
    /// </summary>
    /// <param name="money"> placeholder for resources </param>
    public void UpdateResourceLabel(float money)
    {
        moneyLabel.text = "Kredits: " + money.ToString();
    }

    /// <summary>
    /// Activates the End Screen for both players.
    /// </summary>
    /// <param name="loser"> the losing player's faction </param>
    [PunRPC]
    public void ActivateEndScreen(GameManager.Faction loser)
    {
        inGameUI.style.display = DisplayStyle.None;
        pauseMenuUI.style.display = DisplayStyle.None;
        endGameUI.style.display = DisplayStyle.Flex;
        if(loser == GameManager.Faction.Earth)
        {
            winnerLabel.text = "Winner: Mars";
        }
        else
        {
            winnerLabel.text = "Winner: Earth";
        }     
    }

    /// <summary>
    /// Toggles the mid-game pause menu.
    /// </summary>
    public void TogglePauseMenu()
    {
        if(pauseMenuUI.style.display == DisplayStyle.None && endGameUI.style.display == DisplayStyle.None)
        {
            pauseMenuUI.style.display = DisplayStyle.Flex;
        }
        else
        {
            pauseMenuUI.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Sends an error message to be displayed to the player through the UI, (uses a Coroutine)
    /// </summary>
    /// <param name="msg"> The message to be displayed</param>
    public void SendAlertMessageToUI(string msg)
    {
        StartCoroutine(AlertCoroutine(msg));
    }

    /// <summary>
    /// Handles SendAlertMessageToUI(), with a Coroutine displaying the message for 3 seconds;
    /// </summary>
    private IEnumerator AlertCoroutine(string msg)
    {
        Label alert = new Label();
        alert.text = msg;
        alert.style.fontSize = 32;
        alert.style.color = Color.white;
        alert.style.unityTextOutlineWidth = 1;
        alert.style.unityTextOutlineColor = Color.black;
        bottomLeft.Add(alert);

        alert.style.visibility = Visibility.Visible;

        yield return new WaitForSeconds(3);
        bottomLeft.Remove(alert);
    }

    /// <summary>
    /// Handle incoming rematch request
    /// </summary>
    [PunRPC]
    public void RematchInitiated()
    {
        endGameUI.style.display = DisplayStyle.None;
        rematchMenu.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Sends the players back to the faction select menu, through the Master Client
    /// </summary>
    [PunRPC]
    public void SendBackToLobby()
    {
        PhotonNetwork.LoadLevel("WaitingRoom");
    }
}
