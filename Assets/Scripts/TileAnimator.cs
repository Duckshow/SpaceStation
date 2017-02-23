using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileAnimator {

    public const float FPS = 4f;
    private float currentFPS;

    public class TileAnimation {
        public CachedAssets.DoubleInt[] Bottom;
        public CachedAssets.DoubleInt[] Top;
        //public TileAnimation(CachedAssets.DoubleInt[] _bottomFrames, CachedAssets.DoubleInt[] _topFrames) {
        //    Bottom = _bottomFrames;
        //    Top = _topFrames;
        //}
        private static List<CachedAssets.DoubleInt> frameList = new List<CachedAssets.DoubleInt>();
        public TileAnimation(int bottomPosY, int topPosY, int amountOfFrames, int bottomForceFrameX = -1, int topForceFrameX = -1) {

            frameList.Clear();
            for (int i = 0; i < amountOfFrames; i++)
                frameList.Add(new CachedAssets.DoubleInt(bottomForceFrameX >= 0 ? bottomForceFrameX : i, bottomPosY));
            Bottom = frameList.ToArray();

            frameList.Clear();
            for (int i = 0; i < amountOfFrames; i++)
                frameList.Add(new CachedAssets.DoubleInt(topForceFrameX >= 0 ? topForceFrameX : i, topPosY));
            Top = frameList.ToArray();
        }
        public TileAnimation Reverse() {
            frameList.Clear();
            frameList.AddRange(Bottom);
            frameList.Reverse();
            Bottom = frameList.ToArray();

            frameList.Clear();
            frameList.AddRange(Top);
            frameList.Reverse();
            Top = frameList.ToArray();

            return this;
        }
        public CachedAssets.DoubleInt GetBottomFirstFrame() {
            return Bottom == null ? null : Bottom[0];
        }
        public CachedAssets.DoubleInt GetTopFirstFrame() {
            return Top == null ? null : Top[0];
        }
    }
    public TileAnimation Animation;
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
    public TileAnimation GetAirlockAnimation(AnimationContextEnum _context, Tile.TileOrientation _direction) {
        if (_context != AnimationContextEnum.Wait) {
            if ((owner._IsHorizontal_ && _direction != Tile.TileOrientation.Bottom && _direction != Tile.TileOrientation.Top) ||
                (owner._IsVertical_ && _direction != Tile.TileOrientation.Left && _direction != Tile.TileOrientation.Right))
                Debug.LogError(_direction.ToString() + " is an invalid direction to go towards! D:");
        }

        switch (_context) {
            case AnimationContextEnum.Open:
                if (owner._IsHorizontal_)
                    return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop : CachedAssets.WallSet.anim_AirlockHorizontal_OpenBottom);
                else
                    return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_OpenRight : CachedAssets.WallSet.anim_AirlockVertical_OpenLeft);
            case AnimationContextEnum.Close:
                if (owner._IsHorizontal_)
                    return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_CloseTop : CachedAssets.WallSet.anim_AirlockHorizontal_CloseBottom);
                else
                    return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_CloseRight : CachedAssets.WallSet.anim_AirlockVertical_CloseLeft);
            case AnimationContextEnum.Wait:
                if (owner._IsHorizontal_)
                    return (CachedAssets.WallSet.anim_AirlockHorizontal_Wait);
                else
                    return (CachedAssets.WallSet.anim_AirlockVertical_Wait);
            default:
                throw new System.NotImplementedException(_context.ToString() + " hasn't been properly implemented yet!");
        }
    }

    public void AnimateSequence(TileAnimation[] _sequence) {
        Grid.Instance.StartCoroutine(_AnimateSequence(_sequence));
    }
    IEnumerator _AnimateSequence(TileAnimation[] _sequence) {
        for (int i = 0; i < _sequence.Length; i++) {
            Animate(_sequence[i], true, false);
            yield return new WaitForSeconds(GetProperWaitTimeForAnim(_sequence[i]));
        }
    }
    public void Animate(TileAnimation _animation, bool _forward, bool _loop, float _fps = 0) {
        if (!IsFinished)
            Debug.LogWarning("Animator wasn't finished but launched new animation anyway! Not sure if dangerous!");
        if (!_loop)
            owner.SetBuildingAllowed(false);

        currentFPS = _fps > 0 ? _fps : FPS;

        Animation = _animation;
        SetPlayForward(_forward);
        CurrentFrame = StartFrame;
        Loop = _loop;
        IsPaused = false;

        IsFinished = false;
        timeSinceLastFrame = Time.time;
        //animationRoutine = _Animate();
        //Grid.Instance.StartCoroutine(animationRoutine);
    }
    public float GetProperWaitTimeForAnim(TileAnimation _anim) {
        return (Mathf.Max(_anim.Bottom.Length, _anim.Top.Length) + 1) / currentFPS;
    }

    public void SetPlayForward(bool _b) {
        PlayForward = _b;
        StartFrame = PlayForward ? -1 : Mathf.Max(Animation.Bottom == null ? 0 : Animation.Bottom.Length, Animation.Top == null ? 0 : Animation.Top.Length);
        EndFrame = PlayForward ? Mathf.Max(Animation.Bottom == null ? 0 : Animation.Bottom.Length, Animation.Top == null ? 0 : Animation.Top.Length) - 1 : 0;
    }

    public void StopAnimating() {
        //if (animationRoutine == null)
        //    return;

        //Grid.Instance.StopCoroutine(animationRoutine);
        //animationRoutine = null;
        IsFinished = true;
        owner.SetBuildingAllowed(true);
    }

    //IEnumerator _Animate() {
    //    while (!IsFinished) {
    //        if (!IsPaused)
    //            SwitchToNextFrame();

    //        yield return new WaitForSeconds(1 / currentFPS);

    //        if (IsFinished) {
    //            // loop
    //            if (Loop) {
    //                CurrentFrame = StartFrame;
    //                IsFinished = false;
    //                continue;
    //            }

    //            owner.SetBuildingAllowed(true);
    //        }
    //    }
    //}

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
    private float timeSinceLastFrame;
    void SwitchToNextFrame() {

        timeSinceLastFrame = Time.time;

        // set new frame and get graphics
        CurrentFrame = Mathf.Clamp(PlayForward ? CurrentFrame + 1 : CurrentFrame - 1, Mathf.Min(StartFrame, EndFrame), Mathf.Max(StartFrame, EndFrame));

        // apply new frame
        currentFrameBottom = (Animation.Bottom != null && Animation.Bottom.Length > CurrentFrame) ? Animation.Bottom[CurrentFrame] : null;
        currentFrameTop = (Animation.Top != null && Animation.Top.Length > CurrentFrame) ? Animation.Top[CurrentFrame] : null;
        owner.ChangeGraphics(currentFrameBottom, currentFrameTop);

        IsFinished = CurrentFrame == EndFrame;
        if (IsFinished) {
            TimeFinished = Time.time;
            Iterations++;
        }
    }
}
