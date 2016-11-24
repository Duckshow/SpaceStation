using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridAnimator : MonoBehaviour {

    [SerializeField] private float FPS = 6;

    private List<AnimatedTile> tilesToAnimate = new List<AnimatedTile>();
    class AnimatedTile{
        public Tile TileToAnimate { get; private set; }
        public CachedAssets.ShadedAsset[] BottomAnimation { get; private set; }
        public CachedAssets.ShadedAsset[] TopAnimation { get; private set; }
        public bool IsTallerThanRegular { get; private set; }
        public int CurrentFrame { get; private set; }
        public int StartFrame { get; private set; }
        public int EndFrame { get; private set; }
        public bool IsFinished { get; private set; }
        public bool PlayForward { get; private set; }
        public bool Loop { get; private set; }
        public float SecondsBeforeReverse { get; private set; }
        public float TimeFinished { get; private set; }
        public float Iterations { get; private set; }
        public bool IsPaused = false;

        public AnimatedTile(Tile _tile, CachedAssets.ShadedAsset[] _bottomAnim, CachedAssets.ShadedAsset[] _topAnim, bool _forward, bool _loop, float _secondsBeforeReverse) {
            TileToAnimate = _tile;
            BottomAnimation = _bottomAnim;
            TopAnimation = _topAnim;
            IsTallerThanRegular = IsTallerThanDefault();
            SetPlayForward(_forward);
            CurrentFrame = StartFrame;
            Loop = _loop;
            SecondsBeforeReverse = _secondsBeforeReverse;
            
        }

        bool IsTallerThanDefault() {
            if (BottomAnimation != null) {
                for (int i = 0; i < BottomAnimation.Length; i++) {
                    if (BottomAnimation[i].Diffuse.rect.height > Grid.TILE_RESOLUTION)
                        return true;
                }
            }
            if (TopAnimation != null) {
                for (int i = 0; i < TopAnimation.Length; i++) {
                    if (TopAnimation[i].Diffuse.rect.height > Grid.TILE_RESOLUTION)
                        return true;
                }
            }

            return false;
        }

        public void SetFrame(int _f, bool _silently = false) {
            if (_silently) {
                CurrentFrame = _f;
                return;
            }

            CurrentFrame = Mathf.Clamp(_f, Mathf.Min(StartFrame, EndFrame), Mathf.Max(StartFrame, EndFrame));
            IsFinished = CurrentFrame == EndFrame;
            if (IsFinished) {
                TimeFinished = Time.time;
                Iterations++;
            }
        }

        public void SetPlayForward(bool _b) {
            PlayForward = _b;
            StartFrame = PlayForward ? 0 : Mathf.Max(BottomAnimation == null ? 0 : BottomAnimation.Length, TopAnimation == null ? 0 : TopAnimation.Length) - 1;
            EndFrame = PlayForward ? Mathf.Max(BottomAnimation == null ? 0 : BottomAnimation.Length, TopAnimation == null ? 0 : TopAnimation.Length) - 1 : 0;
        }
    }


    void Start () {
        StartCoroutine(_Update());
	}
	
	IEnumerator _Update () {
        while (true) {
            for (int i = 0; i < tilesToAnimate.Count; i++) {
                if (!tilesToAnimate[i].IsPaused)
                    SwitchToNextFrame(tilesToAnimate[i]);

                if (tilesToAnimate[i].IsFinished) {
                    // loop
                    if (tilesToAnimate[i].Loop && tilesToAnimate[i].SecondsBeforeReverse < 0) {
                        tilesToAnimate[i].SetFrame(tilesToAnimate[i].StartFrame);
                        continue;
                    }

                    //pingpong
                    if ((tilesToAnimate[i].Loop || tilesToAnimate[i].Iterations < 2) && tilesToAnimate[i].SecondsBeforeReverse >= 0) {
                        tilesToAnimate[i].IsPaused = true;

                        if (Time.time - tilesToAnimate[i].TimeFinished > tilesToAnimate[i].SecondsBeforeReverse) {
                            tilesToAnimate[i].IsPaused = false;

                            tilesToAnimate[i].SetPlayForward(!tilesToAnimate[i].PlayForward);
                        }

                        continue;
                    }

                    tilesToAnimate[i].TileToAnimate.SetBuildingAllowed(true);
                    tilesToAnimate.RemoveAt(i);
                    i--;
                }
            }

            yield return new WaitForSeconds(1 / FPS);
        }
	}

    public void AnimateTile(Tile _tile, bool _forward, bool _loop, float _secondsBeforeReverse) {
        if (tilesToAnimate.FindIndex(x => x.TileToAnimate == _tile) > -1) {
            Debug.LogWarning("A " + _tile._Type_.ToString() + " tile (" + _tile.GridX + ", " + _tile.GridY + ") tried to animate while already animating, but I stopped it! Make sure it's nothing dangerous!");
            return;
        }

        if(!_loop)
            _tile.SetBuildingAllowed(false);

        tilesToAnimate.Add(new AnimatedTile(
            _tile, 
            CachedAssets.Instance.GetAnimationForTile(_tile._Type_, _tile._Orientation_, 0, _getBottomLayer: true), 
            CachedAssets.Instance.GetAnimationForTile(_tile._Type_, _tile._Orientation_, 0, _getBottomLayer: false),
            _forward,
            _loop, 
            _secondsBeforeReverse));

    }
    public void StopAnimatingTile(Tile _tile) {
        AnimatedTile _animTile = tilesToAnimate.Find(x => x.TileToAnimate == _tile);
        if (_animTile == null)
            return;

        _animTile.SetFrame(_animTile.StartFrame); // eh, not perfect since startframe can change, but it'll have to do for now
        tilesToAnimate.Remove(_animTile);

        _tile.SetBuildingAllowed(true);
    }

    private CachedAssets.ShadedAsset currentFrameBottom;
    private CachedAssets.ShadedAsset currentFrameTop;
    void SwitchToNextFrame(AnimatedTile _animTile) {

        // update above neighbour if have to, before any animation happens so we get rid of old pixels and stuff.
        if (_animTile.IsTallerThanRegular && _animTile.TileToAnimate.GridY < Grid.Instance.GridSizeY - 1)
            Grid.Instance.UpdateTile(Grid.Instance.grid[_animTile.TileToAnimate.GridX, _animTile.TileToAnimate.GridY + 1], _updateNeighbours: false, _forceUpdate: true);

        _animTile.SetFrame(_animTile.PlayForward ? _animTile.CurrentFrame + 1 : _animTile.CurrentFrame - 1);
        currentFrameBottom = (_animTile.BottomAnimation != null && _animTile.BottomAnimation.Length > _animTile.CurrentFrame) ? _animTile.BottomAnimation[_animTile.CurrentFrame] : null;
        currentFrameTop = (_animTile.TopAnimation != null && _animTile.TopAnimation.Length > _animTile.CurrentFrame) ? _animTile.TopAnimation[_animTile.CurrentFrame] : null;

        Grid.Instance.ChangeSingleTileGraphics(_animTile.TileToAnimate, currentFrameBottom, currentFrameTop);
    }
}
