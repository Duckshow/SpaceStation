﻿using UnityEngine;

public class TileObject : MonoBehaviour {

    public Tile MyTile;
    public UVController[] MyUVControllers;
    public TileObject Parent { get; private set; }


	void Start () {
        if (Grid.Instance == null)
            return;

        //myUVControllers = GetComponentsInChildren<UVController>();
        SetGridPosition(Grid.Instance.GetClosestFreeNode(transform.position));
	}

    public void SetGridPosition(Tile _tile) {
        if (Parent != null)
            return;

        if (MyTile != null) {
            MyTile.IsOccupiedByObject = false; // TODO: not really futureproof, is it?
            MyTile.OccupyingInspectable = null;
        }

        MyTile = _tile;
        transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        if (isActive) {
            MyTile.IsOccupiedByObject = true;
            MyTile.OccupyingInspectable = GetComponent<CanInspect>();
        }

        Sort();
    }

    public void SetParent(TileObject _parent) {
        Parent = _parent;
        transform.parent = _parent != null ? _parent.transform : null;
    }

    private bool isActive = true;
    public void Activate() {
        if (isActive) //dijnqidnqd // continue here. Try placing a large group of Components together in a cluster and switch between them. Something's making them lock to their tiles and act reeeeaaally weirdly :(
            return;
        if (MyTile == null)
            return;

        isActive = true;
        MyTile = Grid.Instance.GetClosestFreeNode(MyTile);
        if (MyTile == null)
            throw new System.Exception(name + " couldn't find a free tile!");

        MyTile.IsOccupiedByObject = true;
        MyTile.OccupyingInspectable = GetComponent<CanInspect>();
        transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        Sort();
    }
    public void DeActivate() {
        if (!isActive)
            return;
        if (MyTile == null)
            return;

        isActive = false;
        MyTile.IsOccupiedByObject = false;
        MyTile.OccupyingInspectable = null;
    }

    public void Sort() {
        for (int i = 0; i < MyUVControllers.Length; i++)
            MyUVControllers[i].Sort(MyTile.GridY);
    }
}
