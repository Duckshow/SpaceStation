using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TileObject))]
public class TileObjectEditor : Editor {

	private Vector2 oldPosition;

    private TileObject thisTO;
    public override void OnInspectorGUI() {
        thisTO = (TileObject)target;
        DrawDefaultInspector();

		if(!Application.isPlaying)
			return;

		if (thisTO.transform.position.x != oldPosition.x || thisTO.transform.position.y != oldPosition.y)
			thisTO.SetGridPosition(Grid.Instance.GetTileFromWorldPoint(thisTO.transform.position), _setPosition: false);

		oldPosition = thisTO.transform.position;
    }
}
