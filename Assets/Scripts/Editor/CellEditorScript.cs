using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Cell))]
[CanEditMultipleObjects]
public class CellEitorScript : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector(); //draw the normal inspector for the component

		Cell myScript = (Cell)target;

		if (GUILayout.Button("Set to free"))
		{
			myScript.ChangeState(Cell.CellState.Free);
			GameObject.DestroyImmediate(myScript.myBlockedPrefabGO);
		}

		if (GUILayout.Button("Set to blocked"))
		{
			myScript.ChangeState(Cell.CellState.Blocked);
			if(myScript.myBlockedPrefabGO != null) //reset if already existing
			{
				GameObject.DestroyImmediate(myScript.myBlockedPrefabGO);
			}
			if(myScript.blockedPrefab != null)
			{
				Debug.Log("Spawning blocked prefab");
				GameObject newGO = (GameObject)PrefabUtility.InstantiatePrefab(myScript.blockedPrefab);
				newGO.transform.position = myScript.transform.position + myScript.turretSocketPos;
				newGO.transform.SetParent(myScript.transform, true);
				myScript.myBlockedPrefabGO = newGO;
			}
		}

		if (GUILayout.Button("Set as Earth starter cell"))
		{
			GameObject.FindGameObjectWithTag("GameController").GetComponent<PathFinding>().earthStartingCells.Add(myScript);
		}

		if (GUILayout.Button("Set as Mars starter cell"))
		{
			GameObject.FindGameObjectWithTag("GameController").GetComponent<PathFinding>().marsStartingCells.Add(myScript);
		}
	}
}
