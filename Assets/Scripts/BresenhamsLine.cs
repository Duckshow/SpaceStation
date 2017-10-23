using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

public class BresenhamsLine : MonoBehaviour {
    public static Vector2 lastCastStart;
    public static Vector2 lastCastEnd;

    public class OverlapWithTiles {
        public Vector2 TilePos;
        public Vector2[] ExtraTilePositions;
        public Tile Tile;
        public Tile[] ExtraTiles;
        public OverlapWithTiles(Vector2 _tile, Vector2[] _extras = null) {
            TilePos = _tile;
            ExtraTilePositions = _extras;

            if (Grid.Instance != null) { // for debugging 
                Tile = Grid.Instance.GetTileFromWorldPoint(TilePos);

                if (ExtraTilePositions != null){   
                    ExtraTiles = new Tile[ExtraTilePositions.Length];
                    for (int i = 0; i < ExtraTilePositions.Length; i++)
                        ExtraTiles[i] = Grid.Instance.GetTileFromWorldPoint(ExtraTilePositions[i]);    
                }
            }
        }
    }
    public class OverlapSimple {
        public Vector2 Pos;
        public Vector2[] ExtraPositions;
        public OverlapSimple(Vector2 _tile, Vector2[] _extras = null) {
            Pos = _tile;
            ExtraPositions = _extras;
        }
    }

    // move-ratios (how much x will move per y and vice versa)
    private static float YperX;
    private static float XperY;

    // basically the worldpos of tiles (virtually, not literally since this doesn't rely on the actual grid)
    private static Vector2 roundedStart = new Vector2();
    private static Vector2 roundedCurrent = new Vector2();
    private static Vector2 roundedEnd = new Vector2();
    private static Vector2 roundedPrevious;

    // progress to next column in grid (0/0 is corner closest to start, 1/1 is opposite)
    private static Vector2 prog = new Vector2();
 
    // progress when cast reaches opposite side of current tile
    private static Vector2 future = new Vector2();
    
    // total progress since start
    private static Vector2 total;

    private static List<OverlapSimple> overlaps = new List<OverlapSimple>();

    static bool abort = false;
    private enum NextStepEnum { X, Y, Diag }
    public static List<OverlapSimple> GridcastSimple(Vector2 _start, Vector2 _end) {
        if(abort)
            return null;


        // direction (-1, 0, 1)
        Vector2 length = _end - _start;
        Vector2 step = new Vector2(length.x == 0 ? 0 : Mathf.Sign(length.x), length.y == 0 ? 0 : Mathf.Sign(length.y));

        // setup start
        roundedStart.x = Mathf.Floor(Mathf.Abs(_start.x)) * Mathf.Sign(_start.x); // special flooring so negative values go up (-10.5 becomes -10, not -11)
        roundedStart.y = Mathf.Floor(Mathf.Abs(_start.y)) * Mathf.Sign(_start.y);
        roundedStart.x += Mathf.Sign(_start.x) * 0.5f;
        roundedStart.y += Mathf.Sign(_start.y) * 0.5f;
        
        // setup goal
        roundedEnd.x = Mathf.Floor(Mathf.Abs(_end.x)) * Mathf.Sign(_end.x);
        roundedEnd.y = Mathf.Floor(Mathf.Abs(_end.y)) * Mathf.Sign(_end.y);

        float _diffX = Mathf.Abs(_end.x) - Mathf.Floor(_end.x);
        float _diffY = Mathf.Abs(_end.y) - Mathf.Floor(_end.y);
        if(_diffX == 0 || _diffX == 1)
            roundedEnd.x += step.x * 0.5f;
        else
            roundedEnd.x += Mathf.Sign(_end.x) * 0.5f;

        if(_diffY == 0 || _diffY == 1)
            roundedEnd.y += step.y * 0.5f;
        else
            roundedEnd.y += Mathf.Sign(_end.y) * 0.5f;

        // setup progress at start
        prog.x = Mathf.Abs(_start.x - roundedStart.x) + 0.5f;
        if(step.x != 0 && step.x != Mathf.Sign(_start.x - roundedStart.x))
            prog.x = 1 - prog.x;
        prog.y = Mathf.Abs(_start.y - roundedStart.y) + 0.5f;
        if(step.y != 0 && step.y != Mathf.Sign(_start.y - roundedStart.y))
            prog.y = 1 - prog.y;

        // setup tiles found
        overlaps.Clear();
        overlaps.Add(new OverlapSimple(roundedStart));

        // setup variables for loop
        roundedCurrent = roundedStart; // used for making sure that the last tile also finds diagonal neighbours
        roundedPrevious = roundedStart;
        future = prog;
        total = _start;

        // predict and iterate over tiles that will be hit by cast and add to list
        while (roundedCurrent.x != roundedEnd.x || roundedCurrent.y != roundedEnd.y){
            roundedPrevious = roundedCurrent;

            // update ratios
            Vector2 _distance = roundedEnd - roundedCurrent;
            XperY = Mathf.Sqrt(Mathf.Pow((_distance.x / _distance.y), 2));
            YperX = Mathf.Sqrt(Mathf.Pow((_distance.y / _distance.x), 2));

            // predict future x/y using ratio and progress
            future.x = prog.x + (XperY * (1 - prog.y));
            future.y = prog.y + (YperX * (1 - prog.x));
            
            if (_distance.y == 0) {
                XperY = 1;
                YperX = 0;
                future.x = prog.x + 1;
                future.y = prog.y;
            }
            else if (_distance.x == 0) {
                XperY = 0;
                YperX = 1;
                future.x = prog.x;
                future.y = prog.y + 1;
            }

            NextStepEnum _next = NextStepEnum.Diag;
            if (future.x >= 1 && future.y < 1)
                _next = NextStepEnum.X;
            else if (future.x < 1 && future.y >= 1)
                _next = NextStepEnum.Y;

            switch (_next){
                case NextStepEnum.Diag:
                    if (roundedCurrent.x == roundedEnd.x) { 
                        // if x already is goal x, throw error.
                        ErrorDump(_start, _end, step);
                        abort = true;
                        return null;
                    }
                    if (roundedCurrent.y == roundedEnd.y) {
                        // if y already is goal y, throw error.
                        ErrorDump(_start, _end, step);
                        abort = true;
                        return null;
                    }

                    // if exiting this tile through a corner (approximate, because rays shot at vertices always seem to miss :/ )
                    // add next tile and both diagonal neighbours (since we're passing right between them)
                    overlaps.Add(new OverlapSimple(
                        roundedCurrent + step,
                        new Vector2[] {
                            new Vector2(roundedCurrent.x + step.x, roundedCurrent.y),
                            new Vector2(roundedCurrent.x, roundedCurrent.y + step.y)
                        })
                    );
                    roundedCurrent += step;

                    // x/y might not be exactly 1 (since we're approximating future) so we can't zero neither x nor y like we do below
                    prog = Vector2.one - future;

                    // check if the extra neighbours we added was the end
                    if (roundedPrevious.x + step.x == roundedEnd.x && roundedPrevious.y == roundedEnd.y)
                        break;
                    if (roundedPrevious.x == roundedEnd.x && roundedPrevious.y + step.y == roundedEnd.y)
                        break;
                break;
                case NextStepEnum.X:
                    if (roundedCurrent.x == roundedEnd.x) {
                        // if x already is goal x, throw error.
                        ErrorDump(_start, _end, step);
                        abort = true;
                        return null;
                    }

                    prog.x = 0;
                    prog.y = future.y;

                    roundedCurrent.x += step.x;
                    overlaps.Add(new OverlapSimple(roundedCurrent));
                break;
                case NextStepEnum.Y:
                    if (roundedCurrent.y == roundedEnd.y) {
                        // if y already is goal y, throw error.
                        ErrorDump(_start, _end, step);
                        abort = true;
                        return null;
                    }

                    prog.x = future.x;
                    prog.y = 0;

                    roundedCurrent.y += step.y;
                    overlaps.Add(new OverlapSimple(roundedCurrent));
                break;
            }

            total.x = step.x > 0 ? total.x + (Mathf.Clamp01(future.x) - prog.x) : total.x - (Mathf.Clamp01(future.x) - prog.x);
            total.y = step.y > 0 ? total.y + (Mathf.Clamp01(future.y) - prog.y) : total.y - (Mathf.Clamp01(future.y) - prog.y);
        }

        lastCastStart = _start;
        lastCastEnd = _end;
        return overlaps;
    }
    private static void ErrorDump(Vector2 _start, Vector2 _end, Vector2 _step){
        Debug.LogErrorFormat(("Gridcast failed! \n"+
        "Start: {0}, {1} \n"+
        "End: {2}, {3}\n"+
        "Future X, Y: {4}, {5}\n"+
        "Progress: {6}, {7}\n"+
        "XPerY: {8}\n"+
        "YPerX: {9}\n"+
        "WorldPos: {10}, {11}\n"+
        "Step: {12}, {13}\n"+
        "RoundedCurrent: {14}, {15}\n"+
        "RoundedEnd: {16}, {17}").Color(Color.black), 
        _start.x.ToString(), _start.y.ToString(), 
        _end.x.ToString(), _end.y.ToString(), 
        future.x.ToString(), future.y.ToString(), 
        prog.x.ToString(), prog.y.ToString(), 
        XperY.ToString(), YperX.ToString(), 
        total.x.ToString(), total.y.ToString(),
        _step.x.ToString(), _step.y.ToString(),
        roundedCurrent.x.ToString(), roundedCurrent.y.ToString(),
        roundedEnd.x.ToString(), roundedEnd.y.ToString());

        Debug.DrawLine(_start, _end, Color.red, Mathf.Infinity);

        Debug.DrawLine(roundedCurrent + new Vector2(-0.5f, 0.5f), roundedCurrent + new Vector2(0.5f, 0.5f), Color.magenta, Mathf.Infinity);
        Debug.DrawLine(roundedCurrent + new Vector2(0.5f, 0.5f), roundedCurrent + new Vector2(0.5f, -0.5f), Color.magenta, Mathf.Infinity);
        Debug.DrawLine(roundedCurrent + new Vector2(0.5f, -0.5f), roundedCurrent + new Vector2(-0.5f, -0.5f), Color.magenta, Mathf.Infinity);
        Debug.DrawLine(roundedCurrent + new Vector2(-0.5f, -0.5f), roundedCurrent + new Vector2(-0.5f, 0.5f), Color.magenta, Mathf.Infinity);
    }

    private static List<OverlapWithTiles> overlapsWithTiles = new List<OverlapWithTiles>();
    public static List<OverlapWithTiles> Gridcast(Vector2 _start, Vector2 _end) {
        overlapsWithTiles.Clear();

        GridcastSimple(_start, _end);
        for (int i = 0; i < overlaps.Count; i++)
            overlapsWithTiles.Add(new OverlapWithTiles(overlaps[i].Pos, overlaps[i].ExtraPositions));    

        return overlapsWithTiles;
    }

    public static List<BresenhamsLine.OverlapWithTiles> ReplayGridcast() {
        Debug.Log("Replaying: (" + lastCastStart + ") -> (" + lastCastEnd + ")");
        return Gridcast(lastCastStart, lastCastEnd);
    }
}