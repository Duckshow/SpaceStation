using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileAnimator {

    public const float FPS = 4f;
    private float currentFPS;

    public class TileAnimation {
        public CachedAssets.DoubleInt[] Frames;

		private static List<CachedAssets.DoubleInt> frameList = new List<CachedAssets.DoubleInt>();
        public TileAnimation(int texturePosY, int amountOfFrames, CachedAssets.DoubleInt forceFirstFrame = null) {
            frameList.Clear();
            for (int i = 0; i < amountOfFrames; i++) {
                if(i == 0 && forceFirstFrame != null)
                    frameList.Add(forceFirstFrame);
                else
                    frameList.Add(new CachedAssets.DoubleInt(i, texturePosY));
            }
            Frames = frameList.ToArray();
        }
        public TileAnimation Reverse() {
            frameList.Clear();
            frameList.AddRange(Frames);
            frameList.Reverse();
            Frames = frameList.ToArray();
            return this;
        }
    }
    public TileAnimation AnimationBottom;
    public TileAnimation AnimationTop;
    public int CurrentFrame { get; private set; }
    public int StartFrame { get; private set; }
    public int EndFrame { get; private set; }
    public bool IsFinished { get; private set; }
    public bool PlayForward { get; private set; }
    public bool Loop { get; private set; }
    public float TimeFinished { get; private set; }
    public float Iterations { get; private set; }
    public bool IsPaused = false;

    private Tile owner;
    //private IEnumerator animationRoutine;


    public TileAnimator(Tile _owner) {
        owner = _owner;
        Grid.LateUpdateAnimators.Add(this);
        IsFinished = true;
    }

    public enum AnimationContextEnum { Open, Close, Wait };
    public enum AnimationPartEnum { Bottom, Top };
    public TileAnimation GetDoorAnimation(AnimationContextEnum _context) {
        switch (_context) {
            case AnimationContextEnum.Open:
                return (owner._IsHorizontal_ ? CachedAssets.WallSet.anim_DoorHorizontal_Open : CachedAssets.WallSet.anim_DoorVertical_Open);
            case AnimationContextEnum.Close:
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
            case AnimationContextEnum.Close:
                if (owner._IsHorizontal_) {
                    if (_part == AnimationPartEnum.Top)
                        return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_Close_T_Top : CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Top);
                    else
                        return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_Close_T_Bottom : CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Bottom);
                }
                else {
                    if (_part == AnimationPartEnum.Top)
                        return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_Close_R_Top : CachedAssets.WallSet.anim_AirlockVertical_Close_L_Top);
                    else
                        return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_Close_R_Bottom : CachedAssets.WallSet.anim_AirlockVertical_Close_L_Bottom);
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

    public void AnimateSequence(TileAnimation[] _sequenceBottom, TileAnimation[] _sequenceTop) {
        Grid.Instance.StartCoroutine(_AnimateSequence(_sequenceBottom, _sequenceTop));
    }
    IEnumerator _AnimateSequence(TileAnimation[] _sequenceBottom, TileAnimation[] _sequenceTop) {
        if (_sequenceBottom.Length != _sequenceTop.Length)
            throw new System.Exception("AnimationSequences must be of equal length in all parts of a tile!");

        for (int i = 0; i < _sequenceBottom.Length; i++) {
            Animate(_sequenceBottom[i], _sequenceTop[i], true, false);
            yield return new WaitForSeconds(GetProperWaitTimeForTileAnim(_sequenceBottom[i], _sequenceTop[i]));
        }
    }
    public void Animate(TileAnimation _animationBottom, TileAnimation _animationTop, bool _forward, bool _loop, float _fps = 0) {
        if (!IsFinished)
            Debug.LogWarning("Animator wasn't finished but launched new animation anyway! Not sure if dangerous!");
        if (!_loop)
            owner.SetBuildingAllowed(false);

        currentFPS = _fps > 0 ? _fps : FPS;

        AnimationBottom = _animationBottom;
        AnimationTop = _animationTop;
        SetPlayForward(_forward);
        CurrentFrame = StartFrame;
        Loop = _loop;
        IsPaused = false;

        IsFinished = false;
    }
    public float GetProperWaitTimeForTileAnim(TileAnimation _animBottom, TileAnimation _animTop) {
        return (Mathf.Max(_animBottom.Frames.Length, _animTop.Frames.Length) + 1) / currentFPS;
    }

    public void SetPlayForward(bool _b) {
        PlayForward = _b;
        StartFrame = PlayForward ? -1 : Mathf.Max(AnimationBottom == null ? 0 : AnimationBottom.Frames.Length, AnimationTop == null ? 0 : AnimationTop.Frames.Length);
        EndFrame = PlayForward ? Mathf.Max(AnimationBottom == null ? 0 : AnimationBottom.Frames.Length, AnimationTop == null ? 0 : AnimationTop.Frames.Length) - 1 : 0;
    }

    public void StopAnimating() {
        IsFinished = true;
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

            owner.SetBuildingAllowed(true);
        }
    }

    private CachedAssets.DoubleInt currentFrameBottom;
    private CachedAssets.DoubleInt currentFrameTop;
    void SwitchToNextFrame() {

        // set new frame and get graphics
        CurrentFrame = Mathf.Clamp(PlayForward ? CurrentFrame + 1 : CurrentFrame - 1, Mathf.Min(StartFrame, EndFrame), Mathf.Max(StartFrame, EndFrame));

        // apply new frame
        currentFrameBottom = (AnimationBottom != null && AnimationBottom.Frames.Length > CurrentFrame) ? AnimationBottom.Frames[CurrentFrame] : null;
        currentFrameTop = (AnimationTop != null && AnimationTop.Frames.Length > CurrentFrame) ? AnimationTop.Frames[CurrentFrame] : null;
        owner.ChangeWallGraphics(currentFrameBottom, currentFrameTop, false);

        IsFinished = CurrentFrame == EndFrame;
        if (IsFinished) {
            TimeFinished = Time.time;
            Iterations++;
        }
    }
}
