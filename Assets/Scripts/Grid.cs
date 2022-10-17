using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for handling and storing all the cells of the map in a matrix
/// Custom editor of this is controlled by GridEditorScript, for SpawnMap button
/// </summary>
[ExecuteAlways]
public class Grid : MonoBehaviour
{
    public class Position
	{
        int x, y;

        public Position (int x, int y)
		{
            this.x = x;
            this.y = y;
        }
	}

    //Make this script a singleton
    [SerializeField] private static Grid _instance;
    [SerializeField] public static Grid Instance { 
        get {
            if (_instance == null)
            {
                _instance = (Grid)FindObjectOfType(typeof(Grid));
                if (_instance == null)
                    Debug.LogError("Grid Singleton instance not found!");
            }
            return _instance;
        } 
    }

    public GameObject cellPrefab;
    public Vector3 cellSize;

    public int numOfCellsX;
    public int numOfCellsY;

    public Vector3 cellOriginPoint;

    public float spaceBetweenCells = 0.1f;

    //public PathFinding pathFinding;

    [SerializeField]
    public Cell[] myCells;


    /// <summary>
    /// Called from an GridEditorScript button. 
    /// Spawns a completely new map, with the given numOfCellsX and numOfCellsY. 
    /// The cells are poistion off of CellOriginPoint. 
    /// Also initializes all the cells.
    /// </summary>
    public void SpawnNewMap()
	{
        cellSize = cellPrefab.GetComponent<MeshRenderer>().bounds.size;

        Vector3 btmLeft = new Vector3(cellOriginPoint.x - (numOfCellsX * cellSize.x + (numOfCellsX - 1) * spaceBetweenCells) / 2 + (cellSize.x / 2),
                                       cellOriginPoint.y,
                                       cellOriginPoint.z - (numOfCellsY * cellSize.z + (numOfCellsY - 1) * spaceBetweenCells) / 2 + (cellSize.z / 2));

        Debug.Log("Spawning new map");
        Vector3 newPos = btmLeft;

        myCells = new Cell[numOfCellsX * numOfCellsY];
        Debug.Log(myCells.Length);
        for (int i = 0; i < numOfCellsX; i++)
        {
            for (int j = 0; j < numOfCellsY; j++)
            {
                GameObject newCell = GameObject.Instantiate(cellPrefab, newPos, Quaternion.identity, transform);
                myCells[i * numOfCellsY + j] = newCell.GetComponent<Cell>();
                myCells[i * numOfCellsY + j].Initialize(i, j);

                newPos += new Vector3(cellSize.x + spaceBetweenCells, 0, 0);
            }
            newPos = new Vector3(btmLeft.x,
                                  newPos.y,
                                  newPos.z + cellSize.z + spaceBetweenCells);
        }
        Debug.Log(myCells[5 * numOfCellsY + 5]);

    }


    /// <summary>
    /// After a Placeable is placed as an RPC, this is called on each client. 
    /// Registers the cell as blocked, and recalculates the Pathfinding path-
    /// </summary>
    /// <param name="x">The X coordinate of the cell to register it on</param>
    /// <param name="y">The X coordinate of the cell to register it on</param>
    /// <param name="placeable"></param>
    public void RegisterTurret(int x, int y, GameObject placeable)
	{
        //TODO: scale/rotation inherited wrongly?
        placeable.transform.SetParent(getCellAt(x, y).transform, true);
        getCellAt(x, y).RegisterPlaceable(placeable.GetComponent<Placeable>());
        PathFinding.Instance.FindConnections(); //recalculate the pathfinding grid;
	}


    /// <summary>
    /// Returns the cell at the given coordinates according to the grid system
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>The cell at the given coordinates</returns>
    public Cell getCellAt(int x, int y)
	{
        return myCells[x * numOfCellsY + y];
	}
}
