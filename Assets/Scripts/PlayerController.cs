using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The class that each individual player will use to controll the game
/// Ie. build and upgrade towers, send units, etc
/// </summary>
public class PlayerController : MonoBehaviour
{
	//Make this script a singleton
	private static PlayerController _instance;
	public static PlayerController Instance { get { return _instance; } }

	public GameManager.Faction myFaction;

	public Cell SelectedCell { get; private set; }

	//the placeable that each player can build
	public GameObject[] placeablesEarth;
	public GameObject[] placeablesMars;

	public GameObject[] unitsEarth;
	public GameObject[] unitsMars;

	public Transform unitSpawnPosEarth;
	public Transform unitSpawnPosMars;


	//contains the index of the corresponding starting cell in PathFinding.cs
	public enum Lane { Left = 0, Middle = 1, Right = 2 };
	public Lane curLane { get; private set; }

	//Resource gain variables
	public int money { get; private set; }
	[Header("Money variables")]
	public float moneyTimeInterval = 1; //time spent between money increase in seconds
	private float moneyElapsedTime = 0; //time elapsed since last money increase
	public int moneyGain = 10; // amount of money gained on increase
	public int moneyStarting = 50;


	void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("More than one PlayerController in scene!");
			return;
		}
		_instance = this;

		money = moneyStarting;
	}

	public void Initialize(GameManager.Faction faction)
	{
		myFaction = faction;
	}

    public void Update()
    {
		//Resource gain, 
		moneyElapsedTime += Time.deltaTime;
		if(moneyElapsedTime >= moneyTimeInterval)
        {
			moneyElapsedTime = 0;
			money += moneyGain;

			GameUIManager.Instance.UpdateResourceLabel(money);
		}
    }

	//called from cell's OnMouseDown()
	public void SelectCell(int x, int y)
	{
		if (SelectedCell != null)
		{
			SelectedCell.SetSelectedVisuals(false);
		}
		SelectedCell = Grid.Instance.getCellAt(x, y);
		SelectedCell.SetSelectedVisuals(true);
	}

	/// <summary>
	/// Called when the user presses on the Build Tower button.
	/// If the selected cell is empty, spawns the current tower on it.
	/// </summary>
	public void BuildTower(int turretID)
	{
		//error checking
		if(SelectedCell == null)
		{
			GameUIManager.Instance.SendAlertMessageToUI("Trying to build without cell selected!");
			//Debug.LogWarning("Trying to build without cell selected!");
			return;
		}
		if (!SelectedCell.IsFree())
		{
			GameUIManager.Instance.SendAlertMessageToUI("Trying to build on cell thats not free!");
			//Debug.LogWarning("Trying to build on cell thats not free!");
			return;
		}

		//which placeable to spawn?
		//	different based on which player
		//	could also pass an index number as parameter on button press
		GameObject prefab;
		if (myFaction == GameManager.Faction.Earth)
		{
			prefab = placeablesEarth[turretID];
		}
		else
		{
			prefab = placeablesMars[turretID];
		}

		//Placeholder money deduction xd
		if (money >= prefab.GetComponent<Placeable>().GetPrice())
		{

			//if its not a starting cell and there is still a path after placing this
			if (!PathFinding.Instance.CheckIfStartingCell(SelectedCell) 
				&& PathFinding.Instance.FindConnections(SelectedCell, false))
			{
				money -= prefab.GetComponent<Placeable>().GetPrice();
				GameUIManager.Instance.UpdateResourceLabel(money);

				GameObject spawned = PhotonNetwork.Instantiate(prefab.name,
					SelectedCell.GetSocketPos(),
					Quaternion.identity);

				spawned.GetPhotonView().RPC("Place", RpcTarget.All,
					SelectedCell.gridPosX,
					SelectedCell.gridPosY,
					(int)myFaction); //TODO: must be a prettier way
			}
			else
			{
				GameUIManager.Instance.SendAlertMessageToUI("Cant place on that cell - would block unit path!");
			}
		}
        else 
		{
			GameUIManager.Instance.SendAlertMessageToUI("Cant place tower - Not enough kredits!");
		}
	}

	/// <summary>
	/// Deducts the given amount from the Player's money
	/// </summary>
	/// <param name="amount"></param>
	public bool DeductMoney(int amount)
    {
		if(amount <= money)
        {
			money -= amount;
			GameUIManager.Instance.UpdateResourceLabel(money);
			return true;
		}
		return false; ;

	}


	public void GainMoney(int amount)
    {
		money += amount;
		GameUIManager.Instance.UpdateResourceLabel(money);
	}

	/// <summary>
	/// Called from a UI button
	/// </summary>
	/// <param name="newLane"> The new lane as int, see enum Lane definition</param>
	public void ChangeLane(int newLane)
	{
		curLane = (Lane) newLane;
		GameUIManager.Instance.SendAlertMessageToUI("Unit lane set to: " + curLane);
	}

	/// <summary>
	/// Called when the user presses on the Build Unit button.
	/// Spawns the currently selected unit on the player's base
	/// </summary>
	public void BuildUnit(int unitID)
	{

		GameObject unit;
		Transform spawnPos;
		if (myFaction == GameManager.Faction.Earth)
		{
			unit = unitsEarth[unitID];
			spawnPos = unitSpawnPosEarth;
		}
		else
		{
			unit = unitsMars[unitID];
			spawnPos = unitSpawnPosMars;
		}
		if(unit.GetComponent<Unit>().price <= money)
        {
			money -= unit.GetComponent<Unit>().price;
			//spawning
			GameObject spawned = PhotonNetwork.Instantiate(unit.name,
				spawnPos.position,
				spawnPos.rotation);

			//TODO: passing in instantiate
			spawned.GetPhotonView().RPC("Place", RpcTarget.All, (int)myFaction); //Earth faction = 0, mars = 1 
		}
		else
        {
			GameUIManager.Instance.SendAlertMessageToUI("Can't buy unit - not enough money!");
		}
	}

}
