using UnityEngine;
using System.Collections;

public class Task {

    public bool ParallelToPrevious = false;
    public bool Success = false;
    public bool IsRunning = false;

    protected MetaTask meta;

    private IEnumerator cachedRoutine;


    public virtual void Start(MetaTask _meta) {
        IsRunning = true;
        meta = _meta;
    }

    public virtual void Stop() {
        IsRunning = false;
        meta.FinishedTask();
    }

    public class Move : Task {

        Vector3 target;
        float speed;
        Waypoint[] path;
        Waypoint[] pathFull;

        int targetIndex = 0;

        Waypoint nextWaypoint;
        Waypoint previousWaypoint;
        Vector3 newPosition;
        Vector3 diff;
        Tile nextWaypointTile;
        Tile prevWaypointTile;
        Tile.TileOrientation direction;
        int nextWaypointInFullPath;
        Tile.TileOrientation prevDirection;
        float distance;
        float timeAtPrevWaypoint;
        float yieldTime;


        public Move(Vector3 _target, float _speed) {
            target = _target;
            speed = _speed;
        }

        public override void Start(MetaTask _meta) {
            PathRequestManager.RequestPath(_meta.Handler.Owner.transform.position, target, OnPathFound);
            base.Start(_meta);
        }

        void OnPathFound(Waypoint[] newPath, Waypoint[] fullPath, bool pathSuccessful) {
            if (!IsRunning) // if the task was cancelled during the pathfinding
                return;

            if (pathSuccessful) {
                path = newPath;
                pathFull = fullPath;
                cachedRoutine = _PerformTask();
                meta.Handler.Owner.StartCoroutine(cachedRoutine);
            }
            else {
                Debug.LogWarning(meta.Handler.Owner.name + " couldn't find a path!");
                Stop();
            }
        }

        IEnumerator _PerformTask() {
            meta.Handler.Owner.CurrentTask = "Moving to " + target;

            nextWaypoint = path[0];
            previousWaypoint = new Waypoint(meta.Handler.Owner.transform.position);
            nextWaypointTile = Grid.Instance.GetTileFromWorldPoint(nextWaypoint.Position);
            nextWaypointInFullPath = 0;
            prevWaypointTile = Grid.Instance.GetTileFromWorldPoint(meta.Handler.Owner.transform.position);
            distance = Vector3.Distance(previousWaypoint.Position, nextWaypoint.Position);
            timeAtPrevWaypoint = Time.time;
            direction = Tile.TileOrientation.None;
            prevDirection = Tile.TileOrientation.None;
            yieldTime = 0;
            
            SendActorNewOrientation((nextWaypoint.Position - meta.Handler.Owner.transform.position).normalized);
            while (true) {
                if (Mathf.Abs(nextWaypoint.Position.x - meta.Handler.Owner.transform.position.x) < 0.01f && Mathf.Abs(nextWaypoint.Position.y - meta.Handler.Owner.transform.position.y) < 0.01f) {

                    targetIndex++;
                    if (targetIndex >= path.Length)
                        break;

                    // update variables
                    previousWaypoint = nextWaypoint;
                    prevWaypointTile = nextWaypointTile;
                    nextWaypoint = path[targetIndex];
                    nextWaypointTile = Grid.Instance.GetTileFromWorldPoint(nextWaypoint.Position);
                    distance = Vector3.Distance(previousWaypoint.Position, nextWaypoint.Position);
                    
                    for(int i = 0; i < pathFull.Length; i++){
                        if(pathFull[i].Position != nextWaypoint.Position){
                            Debug.Log(pathFull[i].Position + " / " + nextWaypoint.Position);
                            continue;
                        }
                        nextWaypointInFullPath = i;
                        Debug.Log("bing");
                        break;
                    }

                    // update orientation
                    SendActorNewOrientation((nextWaypoint.Position - previousWaypoint.Position).normalized);
                    prevDirection = direction;
                    if(targetIndex <= 1)
                        direction = GetDirectionFromVector3((nextWaypoint.Position - previousWaypoint.Position).normalized);
                    else{
                        Debug.Log(path.Length + ", " + pathFull.Length + ", " + nextWaypointInFullPath);
                        direction = GetDirectionFromVector3((nextWaypoint.Position - pathFull[nextWaypointInFullPath - 1].Position).normalized);
                    }

                    // trigger effects at current tile if applicable
                    if (prevWaypointTile.ForceActorStopWhenPassingThis) {
                        while (!prevWaypointTile.Animator.IsFinished)
                            yield return null;

                        prevWaypointTile.OnActorEnterTile(direction, out yieldTime);
                        yield return new WaitForSeconds(yieldTime);
                    }

                    // trigger effects at next tile if applicable
                    if (nextWaypointTile.ForceActorStopWhenPassingThis) {
                        while (!nextWaypointTile.Animator.IsFinished)
                            yield return null;

                        nextWaypointTile.OnActorApproachingTile(direction);
                    }

                    // force actor to lie down if the next tile is not adjacent to a wall (looks better)
					ForceActorLieDown(nextWaypointTile._WallType_ == Tile.Type.Empty && !nextWaypointTile.IsBlocked_B && !nextWaypointTile.IsBlocked_L && !nextWaypointTile.IsBlocked_R && !nextWaypointTile.IsBlocked_T);

                    // set time so movement is kept at a good pace
                    timeAtPrevWaypoint = Time.time;
                }

                // create a slowdown-effect when approaching nextwaypoint
                newPosition = Vector3.Lerp(previousWaypoint.Position, nextWaypoint.Position, Mathf.Clamp01((Time.time - timeAtPrevWaypoint) / (distance / speed)));
                diff = newPosition - meta.Handler.Owner.transform.position; 
                if (Vector3.Distance(newPosition, nextWaypoint.Position) < Grid.Instance.NodeRadius)
                    diff *= Mathf.Max(0.1f, Vector3.Distance(newPosition, nextWaypoint.Position) / Grid.Instance.NodeRadius);

                meta.Handler.Owner.transform.position += diff;
                yield return null;
            }

            Success = true;
            Stop();
        }

        public override void Stop() {
            if(cachedRoutine != null) // not sure if the "if" is dangerous...
                meta.Handler.Owner.StopCoroutine(cachedRoutine);
            base.Stop();
        }

        float directionAngle;
        void SendActorNewOrientation(Vector3 _newDirection) {
            directionAngle = (Mathf.Rad2Deg * Mathf.Atan2(-_newDirection.x, -_newDirection.y)) + 180;
            
            // up
            if (directionAngle > 315 || directionAngle < 45)
                meta.Handler.Owner.Orienter.SetOrientation(ActorOrientation.OrientationEnum.Up);
            // right
            else if (directionAngle > 45 && directionAngle < 135)
                meta.Handler.Owner.Orienter.SetOrientation(ActorOrientation.OrientationEnum.Right);
            // down
            else if (directionAngle > 135 && directionAngle < 225)
                meta.Handler.Owner.Orienter.SetOrientation(ActorOrientation.OrientationEnum.Down);
            // left
            else if (directionAngle > 225 && directionAngle < 315)
                meta.Handler.Owner.Orienter.SetOrientation(ActorOrientation.OrientationEnum.Left);
        }
        Tile.TileOrientation GetDirectionFromVector3(Vector3 _newDirection) {
            directionAngle = (Mathf.Rad2Deg * Mathf.Atan2(-_newDirection.x, -_newDirection.y)) + 180;

            // up
            if (directionAngle >= 315 || directionAngle < 45)
                return Tile.TileOrientation.Top;
            // right
            else if (directionAngle >= 45 && directionAngle < 135)
                return Tile.TileOrientation.Right;
            // down
            else if (directionAngle >= 135 && directionAngle < 225)
                return Tile.TileOrientation.Bottom;
            // left
            else if (directionAngle >= 225 && directionAngle < 315)
                return Tile.TileOrientation.Left;

            Debug.Log("Direction was None but angle was " + directionAngle);
            return Tile.TileOrientation.None;
        }

        void ForceActorLieDown(bool _b) {
            meta.Handler.Owner.Orienter.ForceLieDown(_b);
        }
    }

    public class TakeResource : Task {

        ResourceContainer toContainer;
        ResourceContainer fromContainer;
        float amount;

        public TakeResource(TaskHandler _handler, ResourceContainer _to, ResourceContainer _from, float _amount, bool _parallelToPrevious) {
            toContainer = _to;
            fromContainer = _from;
            amount = _amount;
            ParallelToPrevious = _parallelToPrevious;

            _handler.ResourcesPendingFetch.Add(_from.Type);
        }
        public TakeResource(TaskHandler _handler, ResourceContainer _to, ResourceContainer _from, bool _parallelToPrevious) { 
            toContainer = _to;
            fromContainer = _from;
            amount = 100000; // arbitrary
            ParallelToPrevious = _parallelToPrevious;

            _handler.ResourcesPendingFetch.Add(_from.Type);
        }

        public override void Start(MetaTask _meta) {
            base.Start(_meta);

            fromContainer.AmountUsingThis++;
            cachedRoutine = _PerformTask();
            if (toContainer.Type != fromContainer.Type) {
                Debug.LogWarning(toContainer.name + " and " + fromContainer.name + " differed in resource type, but tried to make a transfer anyway! This shouldn't happen!");
                Stop();
                return;
            }
            meta.Handler.Owner.StartCoroutine(cachedRoutine);
        }

        IEnumerator _PerformTask() {
            meta.Handler.Owner.CurrentTask = "Fetching " + fromContainer.Type;

            float cap = Mathf.Min(toContainer.Max, toContainer._Current_ + amount);
            while (toContainer._Current_ < cap) {
                yield return new WaitForSeconds(1);

                // break if fail, or if it just dispenses caches (cheap trick, but should work)
                if (!fromContainer.TryTakeResource(toContainer) || fromContainer.DispensesHeldCaches)
                    break;
            }

            Success = true;
            Stop();
        }

        public override void Stop() {
            fromContainer.AmountUsingThis--;
            meta.Handler.Owner.StopCoroutine(cachedRoutine);
            meta.Handler.ResourcesPendingFetch.Remove(toContainer.Type);
            base.Stop();
        }
    }

    public class ConsumeHeldResource : Task {

        ResourceContainer toContainer;
        float heldResourceAmount;

        public ConsumeHeldResource(TaskHandler _handler, ResourceContainer _to, bool _parallelToPrevious) {
            toContainer = _to;
            ParallelToPrevious = _parallelToPrevious;

            switch (_to.Type) {
                case ResourceManager.ResourceType.Water:
                    heldResourceAmount = _to.OwnerPawn.HeldWater;
                    break;
                case ResourceManager.ResourceType.Food:
                    heldResourceAmount = _to.OwnerPawn.HeldFood;
                    break;
            }

            _handler.ResourcesPendingFetch.Add(_to.Type);
        }

        public override void Start(MetaTask _meta) {
            base.Start(_meta);

            cachedRoutine = _PerformTask();
            if (heldResourceAmount == 0) {
                Debug.LogWarning(toContainer.OwnerPawn.name + " tried to consume its Held" + toContainer.Type.ToString() + " but it was empty! This shouldn't happen!");
                Stop();
                return;
            }
            meta.Handler.Owner.StartCoroutine(cachedRoutine);
        }

        IEnumerator _PerformTask() {
            meta.Handler.Owner.CurrentTask = "Consuming Held" + toContainer.Type;

            while (toContainer._Current_ < toContainer.Max) {
                yield return new WaitForSeconds(1);

                float _received = Mathf.Min(Actor.RATE_CONSUME, heldResourceAmount); 
                heldResourceAmount -= _received;
                toContainer.SetResourceCurrent(toContainer._Current_ + _received);

                switch (toContainer.Type) {
                    case ResourceManager.ResourceType.Water:
                        toContainer.OwnerPawn.HeldWater = heldResourceAmount;
                        break;
                    case ResourceManager.ResourceType.Food:
                        toContainer.OwnerPawn.HeldFood = heldResourceAmount;
                        break;
                }

                if (heldResourceAmount == 0)
                    break;
            }

            Success = true;
            Stop();
        }

        public override void Stop() {
            meta.Handler.Owner.StopCoroutine(cachedRoutine);
            meta.Handler.ResourcesPendingFetch.Remove(toContainer.Type);
            base.Stop();
        }
    }
}