using UnityEngine;
using System.Collections;

public class TileAnimator {

    private const int FPS = 6;

    public class TileAnimation {
        public CachedAssets.DoubleInt[] Bottom;
        public CachedAssets.DoubleInt[] Top;
        public TileAnimation(CachedAssets.DoubleInt[] _bottomFrames, CachedAssets.DoubleInt[] _topFrames) {
            Bottom = _bottomFrames;
            Top = _topFrames;
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
    private IEnumerator animationRoutine;


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
        if ((owner._IsHorizontal_ && _direction != Tile.TileOrientation.Left && _direction != Tile.TileOrientation.Right) ||
            (owner._IsVertical_ && _direction != Tile.TileOrientation.Bottom && _direction != Tile.TileOrientation.Top))
            Debug.LogError(_direction.ToString() + " is an invalid direction to come from! D:");

        switch (_context) {
            case AnimationContextEnum.Open:
                if (owner._IsHorizontal_)
                    return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_OpenBottom : CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop);
                else
                    return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_OpenLeft : CachedAssets.WallSet.anim_AirlockVertical_OpenRight);
            case AnimationContextEnum.Close:
                if (owner._IsHorizontal_)
                    return (_direction == Tile.TileOrientation.Bottom ? CachedAssets.WallSet.anim_AirlockHorizontal_CloseBottom : CachedAssets.WallSet.anim_AirlockHorizontal_CloseTop);
                else
                    return (_direction == Tile.TileOrientation.Left ? CachedAssets.WallSet.anim_AirlockVertical_CloseLeft : CachedAssets.WallSet.anim_AirlockVertical_CloseRight);
            case AnimationContextEnum.Wait:
                if (owner._IsHorizontal_)
                    return (CachedAssets.WallSet.anim_AirlockHorizontal_Wait);
                else
                    return (CachedAssets.WallSet.anim_AirlockVertical_Wait);
            default:
                throw new System.NotImplementedException(_context.ToString() + " hasn't been properly implemented yet!");
        }
    }
    private static int frameCount = 0;
    public static float GetAnimationLengthInSeconds(TileAnimation _animation) {
        frameCount = Mathf.Max(_animation.Bottom.Length, _animation.Top.Length);
        return frameCount / FPS;
    }
    public void Animate(TileAnimation _animation, bool _forward, bool _loop) {
        if(!_loop)
            owner.SetBuildingAllowed(false);

        Animation = _animation;
        SetPlayForward(_forward);
        CurrentFrame = StartFrame;
        Loop = _loop;
        IsPaused = false;

        if (IsFinished) {
            IsFinished = false;
            animationRoutine = _Animate();
            Grid.Instance.StartCoroutine(animationRoutine);
        }
    }
    public void SetPlayForward(bool _b) {
        PlayForward = _b;
        StartFrame = PlayForward ? 0 : Mathf.Max(Animation.Bottom == null ? 0 : Animation.Bottom.Length, Animation.Top == null ? 0 : Animation.Top.Length) - 1;
        EndFrame = PlayForward ? Mathf.Max(Animation.Bottom == null ? 0 : Animation.Bottom.Length, Animation.Top == null ? 0 : Animation.Top.Length) - 1 : 0;
    }
    IEnumerator _Animate() {
        while (!IsFinished) {
            if (!IsPaused)
                SwitchToNextFrame();

            if (IsFinished) {
                // loop
                if (Loop) {
                    CurrentFrame = StartFrame;
                    IsFinished = false;
                    continue;
                }

                owner.SetBuildingAllowed(true);
            }

            yield return new WaitForSeconds(1 / FPS);
        }
    }
    public void StopAnimating() {
        if (animationRoutine == null)
            return;

        Grid.Instance.StopCoroutine(animationRoutine);
        animationRoutine = null;
        IsFinished = true;
        owner.SetBuildingAllowed(true);
    }

    private CachedAssets.DoubleInt currentFrameBottom;
    private CachedAssets.DoubleInt currentFrameTop;
    void SwitchToNextFrame() {
        // set new frame and get graphics
        CurrentFrame = Mathf.Clamp(PlayForward ? CurrentFrame + 1 : CurrentFrame - 1, Mathf.Min(StartFrame, EndFrame), Mathf.Max(StartFrame, EndFrame));
        IsFinished = CurrentFrame == EndFrame;
        if (IsFinished) {
            TimeFinished = Time.time;
            Iterations++;
        }
        currentFrameBottom = (Animation.Bottom != null && Animation.Bottom.Length > CurrentFrame) ? Animation.Bottom[CurrentFrame] : null;
        currentFrameTop = (Animation.Top != null && Animation.Top.Length > CurrentFrame) ? Animation.Top[CurrentFrame] : null;

        // apply new frame
        owner.ChangeGraphics(
            (Animation.Bottom != null && Animation.Bottom.Length > CurrentFrame) ? Animation.Bottom[CurrentFrame] : null,
            (Animation.Top != null && Animation.Top.Length > CurrentFrame) ? Animation.Top[CurrentFrame] : null);
    }
}
