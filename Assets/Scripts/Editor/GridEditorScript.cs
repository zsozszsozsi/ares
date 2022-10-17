using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Grid))]
public class GridEditorScript : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector(); //draw the normal inspector for the component

		Grid myScript = (Grid)target;
		
		if(GUILayout.Button("Spawn new map"))
		{
			myScript.SpawnNewMap();
			EditorUtility.SetDirty(myScript);
		}

		if (GUILayout.Button("Reset blocked cells"))
		{
			foreach(Cell curCell in myScript.myCells)
			{
				if(curCell.myState == Cell.CellState.Blocked)
				{
					if (curCell.myBlockedPrefabGO != null) //reset if already existing
					{
						GameObject.DestroyImmediate(curCell.myBlockedPrefabGO);
					}
					if (curCell.blockedPrefab != null)
					{
						Debug.Log("Spawning blocked prefab");
						GameObject newGO = (GameObject)PrefabUtility.InstantiatePrefab(curCell.blockedPrefab);
						newGO.transform.position = curCell.transform.position + curCell.turretSocketPos;
						newGO.transform.SetParent(myScript.transform, true);
						curCell.myBlockedPrefabGO = newGO;
					}
					if (curCell.blockedPrefab != null)
					{
						curCell.blockedPrefab.transform.rotation = curCell.rotateBlockedPrefab
																	? Quaternion.Euler(0, Random.Range(0, 360), 0)
																	: Quaternion.identity;
					}
					curCell.RenderMaterial();
				}
			}
		}
	}
}
