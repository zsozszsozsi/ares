using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
	public enum CellState { Free, Blocked }

	/// <summary>
	/// 
	/// </summary>
	public enum PathState { Outside, Inside, Critical, Inaccessible }

	public CellState myState = CellState.Free;
	private CellState orginalCellState;

	[Tooltip("Which faction can place on this cell:")]
	public GameManager.Faction faction; //set in editor

	PhotonView view;

	//[HideInInspector]
	[field: SerializeField] public Placeable myPlaceable { get; private set; }
	public Vector3 turretSocketPos;


	[field: SerializeField] public int gridPosX { get; private set; }
	[field: SerializeField] public int gridPosY { get; private set; }


	// PATHFINDING ###############################X
	public int pathFindingInd { get; set; } //the index of this cell in the "nodes" list of PathFinding.cs
	
	public PathState myPathState = PathState.Outside;

	[field: SerializeField] public Cell pathFindingNextNode { get; private set; }
	public bool pathFindingDebugVisualsEnabled = true;
	public Color pathFindingDebugColor = Color.magenta;


	// MATERIALS, COLORS #########################X
	public Color defaultColor;
	public Color selectedColor;

	public Material freeMaterial;
	public Material blockedMaterial; //when you cant place on this cell
	public Material criticalMaterial;
	public Material selectedMaterial;

	public GameObject blockedPrefab;
	public GameObject myBlockedPrefabGO;
	public bool rotateBlockedPrefab = true;

	// CACHED VARIABLES ###########################X
	public Vector3 myPosition;
	private Renderer myRenderer;
	private PlayerController buildManager;

	public bool isInitialized = false;

	#region MonoBehaviour methods

	private void Start()
	{
		//move to custom initialize()...?
		myRenderer = GetComponent<Renderer>();
		buildManager = PlayerController.Instance;

		myPosition = transform.position;
		orginalCellState = myState;

		view = GetComponent<PhotonView>();
		isInitialized = true;

	}

	public void Update()
	{
		if (pathFindingDebugVisualsEnabled && pathFindingNextNode != null)
		{
			Debug.DrawLine(myPosition + 2*turretSocketPos, pathFindingNextNode.myPosition + 2*turretSocketPos, pathFindingDebugColor, Time.deltaTime);
		}
	}

	#endregion MonoBehaviour methods

	#region public methods

	public void setNextTarget(Cell next)
	{
		pathFindingNextNode = next;
	}

	/// <summary>
	/// </summary>
	/// <returns>"Is this cell free for units/turrets?"</returns>
	public bool IsFree()
	{
		return myPlaceable == null && myState == CellState.Free;
	}

	public void Initialize(int cellX, int cellY)
	{
		gridPosX = cellX;
		gridPosY = cellY;
	}

	public void RegisterPlaceable(Placeable placeable)
	{
		if (myPlaceable != null) //double checking
			Debug.LogError("Placed turret on another turret!");

		myPlaceable = placeable;
		myState = CellState.Blocked;
	}

	public void ChangeState(CellState newState)
	{
		myState = newState;
		RenderMaterial();
	}


	/// <summary>
	/// Revert revert revert revert revert revert.
	/// </summary>
	public void ChangeStateToOriginal()
	{
		myPlaceable = null;
		myState = orginalCellState;
	}

	public void ChangePathState(PathState newState)
	{
		//if (newState == PathState.Critical && myState == CellState.Free)
		//{
		//	ChangeState(CellState.Critical);
		//}
		//else if(newState != PathState.Critical && myState == CellState.Critical)
		//{
		//	ChangeState(CellState.Free);
		//}
		//we dont really care about the inside/outside 
		myPathState = newState;
		RenderMaterial();
	}

	public void SetSelectedVisuals(bool selected)
	{
		//myRenderer.material.color = selected ? selectedColor : defaultColor;
		if(selected)
		{
			myRenderer.material = selectedMaterial;
		}
		else
		{
			RenderMaterial();
		}
		//TODO: blocked state
	}

	public void RenderMaterial()
	{
		if (myRenderer == null)
			myRenderer = GetComponent<Renderer>();

		switch (myState)
		{
			case CellState.Free: //free
				myRenderer.material = (myPathState == PathState.Critical) ? criticalMaterial : freeMaterial;
				break;
			case CellState.Blocked: //blocked
				//if its only blocked cos of placement, it was originally free
				myRenderer.material = myPlaceable == null ? blockedMaterial : freeMaterial;
				break;
			//case CellState.Critical: //when its critical to the path to the target
			//	myRenderer.material = criticalMaterial;
			//	break;
			default:
				Debug.LogError("Unexpected cell state during rendering!");
				break;
		}
		if(myPathState == PathState.Inaccessible)
		{
			myRenderer.material.color = Color.red;
		}
	}

	/// <summary>
	/// Where a new placeable should be placed
	/// </summary>
	/// <returns></returns>
	public Vector3 GetSocketPos()
	{
		return transform.position + turretSocketPos;
	}

	#endregion public methods

	#region Mouse methods

	/// <summary>
	/// When the cell is clicked on, select it
	/// </summary>
	private void OnMouseDown()
	{
		if (EventSystem.current.IsPointerOverGameObject())
		{
			//Debug.LogWarning("Pressing cell through a UI element");
			return;
		}

		if (isInitialized && myState != CellState.Blocked)
		{
			if (PlayerController.Instance.myFaction == faction) 
			{
				PlayerController.Instance.SelectCell(gridPosX, gridPosY);
			} else
			{
				GameUIManager.Instance.SendAlertMessageToUI("Cant select the enemy's cell!");
			}
		}

		//Toggles the Turret Upgrade UI, if the cell has a Turret on it
		//Toggles the tower shop of the cell doesnt have a tower
		if(myPlaceable != null)
        {
			if(myPlaceable.GetType().IsSubclassOf(typeof(Turret)) && myPlaceable.GetComponent<PhotonView>().IsMine)
            {
				GameUIManager.Instance.ToggleTurretUpgradeUI((Turret)myPlaceable);
			}
        }
		else if(myState != CellState.Blocked)
		{ 
			GameUIManager.Instance.ActivateTowerShop();
        }
		
	}

	#endregion Mouse methods

	#region Custom Editor methods

	#endregion Custom Editor methods
}
