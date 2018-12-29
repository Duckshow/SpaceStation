using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(NodeObject))]
public class TileObjectEditor : Editor {

	private Vector2 oldPosition;

    private NodeObject thisTO;
    public override void OnInspectorGUI() {
        thisTO = (NodeObject)target;
        DrawDefaultInspector();

		if(!Application.isPlaying)
			return;

		if (thisTO.transform.position.x != oldPosition.x || thisTO.transform.position.y != oldPosition.y)
			thisTO.SetGridPosition(GameGrid.Instance.GetNodeFromWorldPos(thisTO.transform.position), _setPosition: false);

		oldPosition = thisTO.transform.position;
    }
}
