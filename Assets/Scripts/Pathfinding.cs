using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

    PathRequestManager requestManager;
    Grid grid;


    void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 _startPos, Vector3 _targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Tile startNode = grid.GetTileFromWorldPoint(_startPos);
        Tile targetNode = grid.GetTileFromWorldPoint(_targetPos);

        if (startNode == targetNode) {
            UnityEngine.Debug.Log("Something tried to walk to where it already was! Skip!");
            requestManager.FinishedProcessingPath(waypoints, false);
            yield break;
        }

        if (startNode.Walkable && targetNode.Walkable) {
            Heap<Tile> openSet = new Heap<Tile>(grid.MaxSize);
            HashSet<Tile> closedSet = new HashSet<Tile>();

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Tile currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Tile neighbour in grid.GetNeighbours(currentNode.GridX, currentNode.GridY)) {
                    if (!neighbour.Walkable)
                        continue;
                    if (neighbour.IsOccupied && neighbour != targetNode) // todo: maybe fix the latter part?
                        continue;
                    if (closedSet.Contains(neighbour))
                        continue;
                    if (Grid.Instance.IsNeighbourBlockedDiagonally(currentNode, neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour)) {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.ParentTile = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        yield return null;
        if (pathSuccess)
            waypoints = RetracePath(startNode, targetNode);
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Tile startNode, Tile endNode) {
        List<Tile> path = new List<Tile>();
        Tile currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.ParentTile;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        if (grid.DisplayPaths) {
            for (int i = 1; i < waypoints.Length; i++)
                UnityEngine.Debug.DrawLine(waypoints[i - 1], waypoints[i], Color.yellow, 30);
        }

        return waypoints;
    }

    Vector3[] SimplifyPath(List<Tile> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i - 1].DefaultPositionWorld.x - path[i].DefaultPositionWorld.x, path[i - 1].DefaultPositionWorld.y - path[i].DefaultPositionWorld.y);
            if (directionNew != directionOld)
                waypoints.Add(path[i - 1].CharacterPositionWorld);
            
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Tile nodeA, Tile nodeB) {
        int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        else
            return 14 * distX + 10 * (distY - distX);
    }
}
