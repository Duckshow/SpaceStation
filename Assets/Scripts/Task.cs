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
        int targetIndex = 0;


        public Move(Vector3 _target, float _speed) {
            target = _target;
            speed = _speed;
        }

        public override void Start(MetaTask _meta) {
            PathRequestManager.RequestPath(_meta.Handler.Owner.transform.position, target, OnPathFound);
            base.Start(_meta);
        }

        void OnPathFound(Waypoint[] newPath, bool pathSuccessful) {
            if (!IsRunning) // if the task was cancelled during the pathfinding
                return;

            if (pathSuccessful) {
                path = newPath;
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

            Waypoint _nextWaypoint = path[0];
            Waypoint _previousWaypoint = new Waypoint(meta.Handler.Owner.transform.position, 0);
            Vector3 _newPosition;
            Vector3 _diff;
            Tile _nextWaypointTile = Grid.Instance.GetTileFromWorldPoint(_nextWaypoint.Position);
            Tile _prevWaypointTile = Grid.Instance.GetTileFromWorldPoint(meta.Handler.Owner.transform.position);
            float _distance = Vector3.Distance(_previousWaypoint.Position, _nextWaypoint.Position);
            float _timeAtPrevWaypoint = Time.time;
            Tile.TileOrientation _direction = Tile.TileOrientation.None;

            SendActorNewOrientation((_nextWaypoint.Position - meta.Handler.Owner.transform.position).normalized);
            while (true) {
                if (Mathf.Abs(_nextWaypoint.Position.x - meta.Handler.Owner.transform.position.x) < 0.01f && Mathf.Abs(_nextWaypoint.Position.y - meta.Handler.Owner.transform.position.y) < 0.01f) {

                    targetIndex++;
                    if (targetIndex >= path.Length)
                        break;

                    _previousWaypoint = _nextWaypoint;
                    _nextWaypoint = path[targetIndex];
                    _nextWaypointTile = Grid.Instance.GetTileFromWorldPoint(_nextWaypoint.Position);

                    _distance = Vector3.Distance(_previousWaypoint.Position, _nextWaypoint.Position);
                    
                    // update orientation
                    SendActorNewOrientation((_nextWaypoint.Position - _previousWaypoint.Position).normalized);
                    _direction = GetDirectionFromVector3((_nextWaypoint.Position - _previousWaypoint.Position).normalized);

                    // trigger next waypoint's entry animation
                    if (_nextWaypointTile._Type_ == Tile.TileType.Door)
                        _nextWaypointTile.Animator.Animate(_nextWaypointTile.Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Open), _forward: true, _loop: false);
                    else if (_nextWaypointTile._Type_ == Tile.TileType.Airlock)
                        _nextWaypointTile.Animator.Animate(_nextWaypointTile.Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Open, _direction), _forward: true, _loop: false);


                    if (_prevWaypointTile._Type_ == Tile.TileType.Airlock) {
                        TileAnimator.TileAnimation _anim = _prevWaypointTile.Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, _direction);
                        _prevWaypointTile.Animator.Animate(_anim, _forward: true, _loop: false);
                        yield return new WaitForSeconds(TileAnimator.GetAnimationLengthInSeconds(_anim));
                    }

                     // TODO:  Open on approach, close when arrived, wait, open, then close when the actor has left.


                    // wait here if there's a waittime
                    if (_previousWaypoint.WaitTime > 0) {
                        // trigger prevWaypoints waiting animation
                        if (_prevWaypointTile._Type_ == Tile.TileType.Door)
                            _prevWaypointTile.Animator.Animate(_prevWaypointTile.Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Wait), _forward: true, _loop: false);
                        else if (_prevWaypointTile._Type_ == Tile.TileType.Airlock)
                            _prevWaypointTile.Animator.Animate(_prevWaypointTile.Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Wait, Tile.TileOrientation.None), _forward: true, _loop: false);

                        yield return new WaitForSeconds(_previousWaypoint.WaitTime);
                    }


                    // trigger prevWaypoints exit animation
                    if (_prevWaypointTile._Type_ == Tile.TileType.Door)
                        _prevWaypointTile.Animator.Animate(_prevWaypointTile.Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Close), _forward: true, _loop: false);
                    else if (_prevWaypointTile._Type_ == Tile.TileType.Airlock)
                        _prevWaypointTile.Animator.Animate(_prevWaypointTile.Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, GetReverseDirection(_direction)), _forward: true, _loop: false);

                    // force actor to lie down if the next tile is not adjacent to a wall (looks better)
                    ForceActorLieDown(_nextWaypointTile._Type_ == Tile.TileType.Empty && !_nextWaypointTile.HasConnectable_B && !_nextWaypointTile.HasConnectable_L && !_nextWaypointTile.HasConnectable_R && !_nextWaypointTile.HasConnectable_T);

                    // set time so movement is kept at a good pace
                    _timeAtPrevWaypoint = Time.time;
                }

                // create a slowdown-effect when approaching nextwaypoint
                _newPosition = Vector3.Lerp(_previousWaypoint.Position, _nextWaypoint.Position, Mathf.Clamp01((Time.time - _timeAtPrevWaypoint) / (_distance / speed)));
                _diff = _newPosition - meta.Handler.Owner.transform.position; 
                if (Vector3.Distance(_newPosition, _nextWaypoint.Position) < Grid.Instance.NodeRadius)
                    _diff *= Mathf.Max(0.1f, Vector3.Distance(_newPosition, _nextWaypoint.Position) / Grid.Instance.NodeRadius);

                meta.Handler.Owner.transform.position += _diff;
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
            if (directionAngle > 315 || directionAngle < 45)
                return Tile.TileOrientation.Top;
            // right
            else if (directionAngle > 45 && directionAngle < 135)
                return Tile.TileOrientation.Right;
            // down
            else if (directionAngle > 135 && directionAngle < 225)
                return Tile.TileOrientation.Bottom;
            // left
            else if (directionAngle > 225 && directionAngle < 315)
                return Tile.TileOrientation.Left;

            return Tile.TileOrientation.None;
        }
        Tile.TileOrientation GetReverseDirection(Tile.TileOrientation _direction) {
            switch (_direction) {
                case Tile.TileOrientation.Bottom:
                    return Tile.TileOrientation.Top;
                case Tile.TileOrientation.Left:
                    return Tile.TileOrientation.Right;
                case Tile.TileOrientation.Top:
                    return Tile.TileOrientation.Bottom;
                case Tile.TileOrientation.Right:
                    return Tile.TileOrientation.Left;
            }
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