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

        Waypoint[] waypoints = new Waypoint[0];
        bool pathSuccess = false;

        Tile startNode = grid.GetTileFromWorldPoint(_startPos);
        Tile targetNode = grid.GetTileFromWorldPoint(_targetPos);

        if (grid.DisplayWaypoints) {
            DrawDebugMarker(startNode.WorldPosition, Color.blue);
            DrawDebugMarker(targetNode.WorldPosition, Color.blue);
        }

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
                    if (neighbour.IsOccupied && neighbour != targetNode)
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

    Waypoint[] RetracePath(Tile startNode, Tile endNode) {

        List<Tile> path = new List<Tile>();
        Tile currentNode = endNode;
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.ParentTile;
        }
        path.Add(startNode); // added 11/18/2016 (not sure if dangerous, but might prevent problems e.g. if the path starts in front of a door?)


        Waypoint[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        if (grid.DisplayPaths || grid.DisplayWaypoints) {
            for (int i = 1; i < waypoints.Length; i++) {
                if(grid.DisplayPaths)
                    UnityEngine.Debug.DrawLine(waypoints[i - 1].Position, waypoints[i].Position, Color.yellow, 30);
                if (grid.DisplayWaypoints) {
                    DrawDebugMarker(waypoints[i - 1].Position, Color.red);

                    if(i == waypoints.Length - 1)
                        DrawDebugMarker(waypoints[i].Position, Color.red);
                }
            }
        }

        return waypoints;
    }
    void DrawDebugMarker(Vector3 _pos, Color _color) {
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0, 0.1f, 0), _pos + new Vector3(0.1f, 0, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0, 0), _pos + new Vector3(0, -0.1f, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0, -0.1f, 0), _pos + new Vector3(-0.1f, 0, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(-0.1f, 0, 0), _pos + new Vector3(0, 0.1f, 0), _color, 30);
    }
    Waypoint[] SimplifyPath(List<Tile> path) {
        List<Waypoint> waypoints = new List<Waypoint>();
        Vector2 directionFromLast = Vector2.zero;
        Vector2 directionToNext = Vector2.zero;

        for (int i = 0; i < path.Count; i++) {
            DrawDebugMarker(path[i].WorldPosition, Color.green);

            //if (path[i]._Type_ == Tile.TileType.Door)
            //    continue;

            if (i < path.Count - 1) {
                directionFromLast = directionToNext;
                directionToNext = new Vector2(path[i].DefaultPositionWorld.x - path[i + 1].DefaultPositionWorld.x, path[i].DefaultPositionWorld.y - path[i + 1].DefaultPositionWorld.y).normalized;

                if (path[i + 1]._Type_ == Tile.TileType.Door) {
                    waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, 0)); // behind door
                    continue;
                }
            }

            if (i > 0) {
                if (path[i - 1]._Type_ == Tile.TileType.Door) {
                    waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, 2)); // ahead of door
                    continue;
                }
            }

            // if the direction is changing (or if at start/end/door), add waypoint
            if (directionToNext != directionFromLast || i == 0 || i == path.Count - 1 || path[i]._Type_ == Tile.TileType.Door) {
                waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, 0));
                continue;
            }
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
public class Waypoint {
    public Vector3 Position;
    public float PassTime;
    public Waypoint(Vector3 _pos, float _passTime) {
        Position = _pos;
        PassTime = _passTime;
    }
}
