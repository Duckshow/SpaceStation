using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileAnimator {

    public const float FPS = 4f;
    private float currentFPS;

    public class TileAnimation {
        private Vector2i[] Frames;
        public int FrameCount { get { return Frames.Length; } }
        public Vector2i First { get { return Frames[0]; } }

		private static List<Vector2i> frameList = new List<Vector2i>();
        public TileAnimation(int texturePosY, int amountOfFrames, Vector2i? forceFirstFrame = null) {
            frameList.Clear();
            for (int i = 0; i < amountOfFrames; i++) {
                if(i == 0 && forceFirstFrame.HasValue)
                    frameList.Add(forceFirstFrame.Value);
                else
                    frameList.Add(new Vector2i(i, texturePosY));
            }
            Frames = frameList.ToArray();
        }
        public Vector2i[] Forward() {
            frameList.Clear();
            frameList.AddRange(Frames);
            return frameList.ToArray();
        }
        public Vector2i[] Reverse() {
            frameList.Clear();
            frameList.AddRange(Frames);
            frameList.Reverse();
            return frameList.ToArray();
        }
    }
    public Vector2i[] AnimationBottom;
    public Vector2i[] AnimationTop;
    public int CurrentFrame { get; private set; }
    public int StartFrame { get; private set; }
    public int EndFrame { get; private set; }
    public bool IsFinished { get; private set; }
    //public bool PlayForward { get; private set; }
    public bool Loop { get; private set; }
    public float TimeFinished { get; private set; }
    public float Iterations { get; private set; }
    public bool IsPaused = false;

    private Tile owner;
    //private IEnumerator animationRoutine;


    public TileAnimator(Tile _owner) {
        owner = _owner;
        IsFinished = true;
    }

    public enum AnimationContextEnum { Open, Wait };
    public enum AnimationPartEnum { Bottom, Top };
    public TileAnimation GetDoorAnimation(AnimationContextEnum _context) {
        switch (_context) {
            case AnimationContextEnum.Open:
                return (owner._IsHorizontal_ ? CachedAssets.WallSet.anim_DoorHorizontal_Open : CachedAssets.WallSet.anim_DoorVertical_Open);
            case AnimationContextEnum.Wait:
                // not used for doors
                return null;
            default:
                throw new System.NotImplementedException(_context.ToString() + " hasn't been properly implemented yet!");
        }
    }
    public TileAnimation GetAirlockAnimation(AnimationPartEnum _part, AnimationContextEnum _context, Tile.TileOrientation _direction) {
        if (_context != AnimationContextEnum.Wait) {
            if ((owner._IsHorizontal_ && _direction != Tile.TileOrientation.Bottom && _direction != Tile.TileOrientation.Top) ||
                (owner._IsVertical_ && _direction != Tile.TileOrientation.Left && _direction != Tile.TileOrientation.Right))
                Debug.LogError(_direction.ToString() + " is an invalid direction to go towards! D:");
        }

        switch (_context) {
            case AnimationContextEnum.Open:
                if (owner._IsHorizontal_) {
                    if (_part == AnimationPartEnum.Top)
                        return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_Open_T_Top : CachedAssets.WallSet.anim_AirlockHorizontal_Open_B_Top);
                    else
                        return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_Open_T_Bottom : CachedAssets.WallSet.anim_AirlockHorizontal_Open_B_Bottom);
                }
                else {
                    if (_part == AnimationPartEnum.Top)
                        return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_Open_R_Top : CachedAssets.WallSet.anim_AirlockVertical_Open_L_Top);
                    else
                        return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_Open_R_Bottom : CachedAssets.WallSet.anim_AirlockVertical_Open_L_Bottom);
                }
            case AnimationContextEnum.Wait:
                if (owner._IsHorizontal_) {
                    if (_part == AnimationPartEnum.Top)
                        return (CachedAssets.WallSet.anim_AirlockHorizontal_Wait_Top);
                    else
                        return (CachedAssets.WallSet.anim_AirlockHorizontal_Wait_Bottom);
                }
                else {
                    if (_part == AnimationPartEnum.Top)
                        return (CachedAssets.WallSet.anim_AirlockVertical_Wait_Top);
                    else
                        return (CachedAssets.WallSet.anim_AirlockVertical_Wait_Bottom);
                }
            default:
                throw new System.NotImplementedException(_context.ToString() + " hasn't been properly implemented yet!");
        }
    }

    public void AnimateSequence(Vector2i[][] _sequenceBottom, Vector2i[][] _sequenceTop) {
        Grid.Instance.StartCoroutine(_AnimateSequence(_sequenceBottom, _sequenceTop));
    }
    IEnumerator _AnimateSequence(Vector2i[][] _sequenceBottom, Vector2i[][] _sequenceTop) {
        if (_sequenceBottom.Length != _sequenceTop.Length)
            throw new System.Exception("AnimationSequences must be of equal length in all parts of a tile!");

        for (int i = 0; i < _sequenceBottom.Length; i++) {
            Animate(_sequenceBottom[i], _sequenceTop[i], false);
            yield return new WaitForSeconds(GetProperWaitTimeForTileAnim(_sequenceBottom[i], _sequenceTop[i]));
        }
    }
    public void Animate(Vector2i[] _animationBottom, Vector2i[] _animationTop, bool _loop, float _fps = 0) {
        if (!IsFinished)
            Debug.LogWarning("Animator wasn't finished but launched new animation anyway! Not sure if dangerous!");
        if (!_loop)
            owner.SetBuildingAllowed(false);

        currentFPS = _fps > 0 ? _fps : FPS;

        AnimationBottom = _animationBottom;
        AnimationTop = _animationTop;
        //SetPlayForward(_forward);
        StartFrame = -1;
        EndFrame = Mathf.Max(
            AnimationBottom == null ? 0 : AnimationBottom.Length
            , AnimationTop == null ? 0 : AnimationTop.Length) - 1;
        CurrentFrame = StartFrame;
        Loop = _loop;
        IsPaused = false;

        IsFinished = false;
        Grid.LateUpdateAnimators.Add(this);
    }
    public float GetProperWaitTimeForTileAnim(Vector2i[] _animBottom, Vector2i[] _animTop) {
        return (Mathf.Max(_animBottom.Length, _animTop.Length) + 1) / currentFPS;
    }

    // public void SetPlayForward(bool _b) {
    //     PlayForward = _b;
    //     StartFrame = PlayForward ? -1 : Mathf.Max(AnimationBottom == null ? 0 : AnimationBottom.FrameCount, AnimationTop == null ? 0 : AnimationTop.FrameCount);
    //     EndFrame = PlayForward ? Mathf.Max(AnimationBottom == null ? 0 : AnimationBottom.FrameCount, AnimationTop == null ? 0 : AnimationTop.FrameCount) - 1 : 0;
    // }

    public void StopAnimating() {
        IsFinished = true;
        Grid.LateUpdateAnimators.Remove(this);
        owner.SetBuildingAllowed(true);
    }

	float timeAtSwitchFrame = -1000;
    public void LateUpdate() {
        if (IsFinished)
            return;
        if (IsPaused)
            return;

        if (Time.time - timeAtSwitchFrame > (1 / currentFPS)) {
            SwitchToNextFrame();
            timeAtSwitchFrame = Time.time;
        }

        if (IsFinished) {
            // loop
            if (Loop) {
                CurrentFrame = StartFrame;
                IsFinished = false;
                return;
            }

            Grid.LateUpdateAnimators.Remove(this);
            owner.SetBuildingAllowed(true);
        }
    }

    private Vector2i currentFrameBottom;
    private Vector2i currentFrameTop;
    void SwitchToNextFrame() {

        // set new frame and get graphics
        CurrentFrame = Mathf.Clamp(CurrentFrame + 1, StartFrame, EndFrame);
        //CurrentFrame = Mathf.Clamp(PlayForward ? CurrentFrame + 1 : CurrentFrame - 1, Mathf.Min(StartFrame, EndFrame), Mathf.Max(StartFrame, EndFrame));

        // apply new frame
        currentFrameBottom  = HasMoreFrames(AnimationBottom, CurrentFrame)  ? AnimationBottom[CurrentFrame] : CachedAssets.WallSet.Null;
        currentFrameTop     = HasMoreFrames(AnimationTop, CurrentFrame)     ? AnimationTop[CurrentFrame]    : CachedAssets.WallSet.Null;
        owner.ChangeWallGraphics(currentFrameBottom, currentFrameTop, false);

        IsFinished = CurrentFrame == EndFrame;
        if (IsFinished) {
            TimeFinished = Time.time;
            Grid.LateUpdateAnimators.Remove(this);
            Iterations++;
        }
    }
    bool HasMoreFrames(Vector2i[] _anim, int _currentFrame){
        return _anim != null && _anim.Length > _currentFrame;
    }
}
