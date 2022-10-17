using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Még progtech projektkor csináltam, Algo 2-s Floyd-Warshall jegyzet alapján
/// Unoptimized for a changing level
/// </summary>
public class PathFinding : MonoBehaviour
{
    [System.Serializable]
    public class Node
	{
        public Cell cell;
        public Node nextNode;
        public int x, y;

        public Node(Cell cell) { 
            this.cell = cell;
            x = cell.gridPosX;
            y = cell.gridPosY;
        }
	}

    //Make this script a singleton
    private static PathFinding _instance;
    public static PathFinding Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (PathFinding)FindObjectOfType(typeof(PathFinding));
                if (_instance == null)
                    Debug.LogError("Grid Singleton instance not found!");
            }
            return _instance;
        }
    }

    [Tooltip("Left: 0, Middle: 1, Right: 2")]
    public List<Cell> earthStartingCells;
    public Cell earthTargetCell;

    [Tooltip("Left: 0, Middle: 1, Right: 2")]
    public List<Cell> marsStartingCells;
    public Cell marsTargetCell;

    private List<Cell> startingCells;
    private Cell targetCell;
    private Cell enemysTargetCell; // basically our base's cell

    private Grid grid;
    [HideInInspector] public List<Node> nodes; //the possible nodes
    private List<Node> newNodes; //used to temporarily store the new possible nodes 

    private int[,] dists; //the distances between the nodes
    private int[,] pis;   //the parent indexes of the nodes in the paths
    private int targetInd; //the index of the targetCell in the Nodes List


    /// <summary>
    /// Called from the GameManager Start() function
    /// </summary>
    /// <param name="faction">THe clients faction</param>
    public void Initialize(GameManager.Faction faction)
	{
        grid = Grid.Instance;

        if (marsTargetCell == null || earthTargetCell == null)
            Debug.LogError("Target cell not set for PathFinding!");

        if (marsStartingCells == null || marsStartingCells.Count != 3
            || earthStartingCells == null || marsStartingCells.Count != 3)
            Debug.LogError("Starting cells not set properly for PathFinding!");

        startingCells = faction == GameManager.Faction.Earth ? earthStartingCells : marsStartingCells;
        targetCell = faction == GameManager.Faction.Earth ? earthTargetCell : marsTargetCell;
        enemysTargetCell = faction != GameManager.Faction.Earth ? earthTargetCell : marsTargetCell;

        FindConnections();

	}



    /// <summary>
    /// Completely recalculates all the possible paths in the grid
    /// </summary>
    /// <param name="exceptionCell">If one of the cells should be handled as blocked, pass it here</param>
    /// <param name="permanentChanges">If we are only interested in if there is still a valid path, set this to False</param>
    /// <returns></returns>
    public bool FindConnections(Cell exceptionCell = null, bool permanentChanges = true)
	{
        //Debug.Log("Refinding node connections,:");
        //if (exceptionCell != null)
            //Debug.Log("(in testing mode)");

        newNodes = new List<Node>();
        for (int row = 0; row < grid.numOfCellsY; row++)
        {
            for (int col = 0; col < grid.numOfCellsX; col++)
            {
                if (IsEmpty(col, row))
                {
                    Cell curCell = grid.getCellAt(col, row);
                    if (exceptionCell == null || exceptionCell != curCell)
					{
                        newNodes.Add(new Node(curCell));
                        curCell.pathFindingInd = newNodes.Count - 1;
                        if (curCell == targetCell)
                        {
                            targetInd = curCell.pathFindingInd;
                            //Debug.Log("Found target cell at: " + targetInd);
                        }
					}
                }
            }
        }

        dists = new int[newNodes.Count, newNodes.Count];
        pis = new int[newNodes.Count, newNodes.Count];

        //setting up the direct connections
        for (int curInd = 0; curInd < newNodes.Count; curInd++)
        {
            //set the default values (infinite distance)
            for (int j = 0; j < newNodes.Count; j++)
            {
                dists[curInd, j] = newNodes.Count * 2;
            }

            int curX = newNodes[curInd].x;
            int curY = newNodes[curInd].y;

            //surrounding cells: right , left    , up      , down
            int[] toTestXs = { curX + 1, curX - 1, curX,     curX };
            int[] toTestYs = { curY,     curY,     curY + 1, curY - 1 };

            //setting starting distances for free surrounding cells
            for (int i = 0; i < 4; i++)
            {
                if(IsEmpty(toTestXs[i], toTestYs[i]))
                {
                    if(exceptionCell == null || 
                        !(exceptionCell.gridPosX == toTestXs[i] && exceptionCell.gridPosY == toTestYs[i]) )
					{
                        int neighbourInd = newNodes.FindIndex((cur) => cur.x == toTestXs[i] && cur.y == toTestYs[i]);

                        if (neighbourInd >= 0)
					    {
                            //if(isOpen(curX, curY, toTestXs[i], toTestYs[i])){
                            dists[curInd, neighbourInd] = 1;
                            pis[curInd, neighbourInd ] = curInd;
						    //} else{
						    //    d[curInd][targetInd] = infiniteValue; //theoratically infinite
						    //    p[curInd][targetInd] = -1;
						    //}
					    }
					    else
					    {
                            Debug.LogError("couldnt find neigbour in dists/pis index of :" + neighbourInd);
					    }
					} 
                }
            }

            //diagonal elements (path leading to itself)
            dists[curInd, curInd] = 0;
            pis[curInd, curInd] = 0; //should be 0 originally but thats bs...?
        }

        //main part of the floyd-warshall algorithm
        for (int k = 0; k < newNodes.Count; k++)
        {
            for (int i = 0; i < newNodes.Count; i++)
            {
                for (int j = 0; j < newNodes.Count; j++)
                {
                    if (dists[i, j] > dists[i, k] + dists[k, j])
                    { //we found a better path thru k
                        dists[i, j] = dists[i, k] + dists[k, j];
                        pis[i, j] = pis[k, j];
                    }
                }
            }
        }


        if (CalculateNextNodes(permanentChanges) && PaintNodes(permanentChanges))
        {
            if(permanentChanges)
                nodes = newNodes; //TODO: do i need to free nodes first?
            
            return true;
        }
        else
            return false;

    }

    /// <summary>
    /// Sets the "nextNode" attributes of all the nodes
    /// </summary>
    private bool CalculateNextNodes(bool permanentChanges = true)
	{
        //Debug.Log("Calculating next nodes, number of nodes: " + newNodes.Count);
        for (int fromInd = 0; fromInd < newNodes.Count; fromInd++)
        {
            //TODO: handle if = myBaseTarget

            //no route to it
            if (dists[fromInd, targetInd] == int.MaxValue) {
                newNodes[fromInd].nextNode = null;
                Debug.LogWarning("Target has infinite distance");
                continue;
            }

            //already at destination
            if (fromInd == targetInd) {
                //Debug.Log("Found target once at : " + fromInd);
                newNodes[fromInd].nextNode = newNodes[fromInd];
                if (permanentChanges)
                {
                    //this is where we will store the next target for this cell for quicker access
                    newNodes[fromInd].cell.setNextTarget(newNodes[fromInd].cell);
                }
                continue;
            }

            int nextParent = pis[fromInd, targetInd];
            int antiInfLoopCounter = 0;
            while (pis[fromInd, nextParent] != fromInd)
            {
                nextParent = pis[fromInd, nextParent];

                //TODO: properly detect inaccesibility/circles in graph
                if(dists[fromInd, targetInd] == 1)
				{
                    nextParent = targetInd;
                    break;
				}

                antiInfLoopCounter++;
                if (antiInfLoopCounter > newNodes.Count * 2) //TODO: optimize
                {
                    //Debug.LogWarning("Entered infinite loop while calculatig paths for node index: " + fromInd
                        //+ "\nNext node set as null");
                    break;
                }

            }


            if (newNodes[nextParent].cell == null || antiInfLoopCounter > newNodes.Count * 2)
            {
                //TODO: set cell.SetNextTarger to null
                //Debug.LogWarning("NextNode set as null");

                newNodes[fromInd].nextNode = null; //TODO: think this thru
                if(permanentChanges)
                    newNodes[fromInd].cell.setNextTarget(null);
            }
            //         else if(antiInfLoopCounter > newNodes.Count * 2)
            //{
            //             //skip straight to the target
            //             newNodes[fromInd].nextNode = targetCell;
            //}
            else
			{
                newNodes[fromInd].nextNode = newNodes[nextParent];

                if (permanentChanges)
				{
                    //this is where we will store the next target for this cell for quicker access
                    newNodes[fromInd].cell.setNextTarget( newNodes[nextParent].cell );
				}
            }
		}

        return true;
	}

    /// <summary>
    /// Goes thru all the paths between the 2 bases, and sets the PathState property of the cells
    /// </summary>
    /// <param name="permanentChanges"></param>
    /// <returns>Whether or not there is still a path between the two bases</returns>
    public bool PaintNodes(bool permanentChanges = true)
	{
        //Debug.Log("Painting nodes, number of starting cells: " + startingCells.Count);

        List<int> pathIndexes = new List<int>();
        //TODO: repaint everey non-critical after its critical
        foreach (Cell starter in startingCells)
		{
            Node nextNode = newNodes.Find( (curnode) => curnode.cell == starter);
            
            int safetyCounter = 0; //to avoid circles in the graph
            while(safetyCounter < newNodes.Count && nextNode != null && nextNode.cell != targetCell)
			{
            
                pathIndexes.Add(nextNode.cell.pathFindingInd);

                if(permanentChanges)
                    nextNode.cell.ChangePathState(Cell.PathState.Critical);

                nextNode = nextNode.nextNode;
                safetyCounter++;
			}

            if(nextNode == null || nextNode.cell != targetCell)
			{
                if(permanentChanges)
                    Debug.LogError("Pathfinder node painter couldnt reach target! loop count: " + safetyCounter);
                else
                    Debug.LogWarning("Pathfinder node painter couldnt reach target! loop count: " + safetyCounter);

                return false;
			}
		}

        if(permanentChanges)
		{
		    for (int i = 0; i < newNodes.Count; i++)
		    {
			    if (pathIndexes.Contains(i) == false)
			    {
                    if(newNodes[i].nextNode != null)
				    {
                        newNodes[i].cell.ChangePathState(Cell.PathState.Outside);
				    }
				    else
				    {
                        newNodes[i].cell.ChangePathState(Cell.PathState.Inaccessible);

                    }
                }
		    }
		}

        return true;
	}

    /// <summary>
    /// Randomly chooses a starting cell from the avaliable list
    /// </summary>
    /// <returns></returns>
    public Cell getStartingTargetCell()
	{
        int startingIndex = (int) PlayerController.Instance.curLane;
		//int randInd = Random.Range(0, startingCells.Count);
        return startingCells[startingIndex];
	}

    /// <summary>
    /// Returns if toCheck is a starter cell
    /// </summary>
    public bool CheckIfStartingCell(Cell toCheck)
	{
		return earthStartingCells.Contains(toCheck) || marsStartingCells.Contains(toCheck);
	}

	/// <summary>
	/// Gives back the next target, if we are already at the "from" cell
	/// </summary>
	/// <param name="from">The cell we are on</param>
	public Cell getNextTarget(Cell from)
	{
        //Cell next = nodes.Find((cur) => from == cur.cell).nextNode;
        Cell next = from.pathFindingNextNode;

        return next;

	}

    /// <summary>
    /// Checks if the cell can be used for the path
    /// </summary>
    /// <param name="x"> X coordinate of the cell</param>
    /// <param name="y"> Y coordinate of the cell</param>
    private bool IsEmpty(int x, int y)
	{
        if (x < 0 || x >= grid.numOfCellsX || y < 0 || y >= grid.numOfCellsY)
            return false;


        Cell curCell = grid.getCellAt(x, y);


        if (curCell == enemysTargetCell)
            return false;

        if ( curCell.IsFree() || curCell == targetCell || startingCells.Contains(curCell) )
            return true;

        return false;

    }
}
 