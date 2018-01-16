using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSorter : MonoBehaviour {

    public SortingLayerEnum SortingLayer = SortingLayerEnum.Grid;
    public enum SortingLayerEnum { 
		BelowGrid, 
		Grid, 
		AboveGrid 
	}
	//public GridLayerEnum GridSorting;
	public bool SortAboveActors = false;
	public enum GridLayerEnum { 
		Floor = 0, 
		FloorCorners = 1, 
		Bottom = 2, 
		Top = 3, 
		TopCorners = 4 
	}

    protected bool hasStarted = false;
    protected MeshRenderer myRenderer;
    public MeshRenderer Renderer { get { return myRenderer; } }


    void Start(){
        if (!hasStarted)
            Setup();
    }

    public virtual void Setup(){
        if (hasStarted && Application.isPlaying)
            return;
        hasStarted = true;

        myRenderer = GetComponent<MeshRenderer>();
		switch (SortingLayer){
			case SortingLayerEnum.AboveGrid:
                myRenderer.sortingLayerName = "AboveGrid";
                break;
			case SortingLayerEnum.Grid:
        		myRenderer.sortingLayerName = "Grid";
                break;
			case SortingLayerEnum.BelowGrid:
                myRenderer.sortingLayerName = "BelowGrid";
                break;
        }
	}

	public static int GetSortOrderFromGridY(int _gridY) { return (Grid.GridSizeY * GameManager.Instance.SortingTransformsPerPosY) - (_gridY * GameManager.Instance.SortingTransformsPerPosY); }
    public int GetSortOrder() { return regularSortOrder; }
    //public int GetSortOrder() { return (customSortOrder.HasValue ? (int)customSortOrder : regularSortOrder); }
    private int regularSortOrder = 0;
    public void Sort(int _gridY){
        if (!hasStarted)
            Setup();

		switch (SortingLayer){
            case SortingLayerEnum.AboveGrid:
                regularSortOrder = Mathf.Abs(Mathf.RoundToInt(Camera.main.transform.position.z - transform.position.z));
                break;
            case SortingLayerEnum.Grid:
				regularSortOrder = GetSortOrderFromGridY(_gridY);
				// if (SortingLayer == SortingLayerEnum.Floor)
				// 	regularSortOrder -= 2;
				// else 
				// if (GridSorting == GridLayerEnum.FloorCorners)
				// 	regularSortOrder += 1;
				// else if (GridSorting == GridLayerEnum.Bottom)
				// 	regularSortOrder += 2;
				// else if (GridSorting == GridLayerEnum.Top)
				// 	regularSortOrder += 8; // 8 is a hack to account for 5 transforms in an actor
				// else if (GridSorting == GridLayerEnum.TopCorners)
				// 	regularSortOrder += 9;

				if (SortAboveActors)
					regularSortOrder += 6; // to account for 5 transforms in an actor
				break;
            case SortingLayerEnum.BelowGrid:
                regularSortOrder = Mathf.Abs(Mathf.RoundToInt(transform.position.z));
                break;
        }


		// Note: custom sorting disabled because unused. should work, but try to avoid it because complexity

        //if (!customSortOrder.HasValue)
            myRenderer.sortingOrder = regularSortOrder;
    }
    // private int? customSortOrder = null;
    // public void SortCustom(int _customSortOrder){
    //     customSortOrder = _customSortOrder;
    //     myRenderer.sortingOrder = _customSortOrder;
    // }
    // public void RemoveCustomSort(){
    //     customSortOrder = null;
    //     myRenderer.sortingOrder = regularSortOrder;
    // }
}
