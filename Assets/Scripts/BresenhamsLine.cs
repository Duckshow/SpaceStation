using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

public class BresenhamsLine : MonoBehaviour // : IEnumerable
{

    //private static List<Vector2> points = new List<Vector2>();
    //public static List<Vector2> GridRay(Vector2 _start, Vector2 _end){
    //    points.Clear();

    //    float DX = _end.x - _start.x;
    //    float DY = _end.y - _start.y;

    //    float stepX = Mathf.Sign(DX);
    //    float stepY = Mathf.Sign(DY);

    //    float progressPerTile_X = Mathf.Abs(1 / DX); // 1 should be (Tile.Radius * 2), but hey, this is faster!
    //    float progressPerTile_Y = Mathf.Abs(1 / DY);

    //    float progress_X = progressPerTile_X * (float)Frac(_start.x); // _start.x should divide by Tile.Radius * 2 but since it's 1 currently, skip!
    //    float progress_Y = progressPerTile_Y * (float)Frac(_start.y); 

    //    Vector2 pos = _start;
    //    int i = 0;
    //    points.Add(pos);
    //    while(pos != _end && i <= (_end - _start).magnitude){
    //        i++;
    //        if(progress_X < progress_Y){
    //            progress_X += progressPerTile_X;
    //            pos.x += stepX;
    //        }
    //        else {
    //            progress_Y += progressPerTile_Y;
    //            pos.y += stepY;
    //        }

    //        points.Add(pos);
    //    }
    //    // if(i >= 500)
    //    // throw new System.Exception("GridRay appears to have missed its target!");

    //    return points;
    //}
    //public static float Frac(float _val){ 
    //    return Mathf.Abs(_val - (float)System.Math.Truncate(_val)); 
    //}

    private static List<Vector2> debugPositions = new List<Vector2>();
    private static List<Vector2> debugValues1 = new List<Vector2>();
    private static List<Vector2> debugValues2 = new List<Vector2>();
    // void OnDrawGizmos(){
    //     for (int i = 0; i < debugPositions.Count; i++) { 
    //         Handles.Label(debugPositions[i], "x");
    //         Handles.Label(debugPositions[i] + new Vector2(0.1f, 0), debugPositions[i] + " == " + debugValues1[i] + ", " + debugValues2[i].x + " == " + debugValues2[i].y);
    //     }
    // }

    // private static float debugX;
    // private static float debugY;

    public static Vector2 lastCastStart;
    public static Vector2 lastCastEnd;

    public class Overlap {
        public Vector2 TilePos;
        public Vector2[] ExtraTilePositions;
        public Tile Tile;
        public Tile[] ExtraTiles;
        public Overlap(Vector2 _tile, Vector2[] _extras = null) {
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

    private static bool roundedDownStartX;
    private static bool roundedDownStartY;
    private static bool roundedDownEndX;
    private static bool roundedDownEndY;
    private static float diffX;
    private static float diffY;
    private static float YperX;
    private static float XperY;
    private static float roundedStartX;
    private static float roundedStartY;
    private static float roundedEndX;
    private static float roundedEndY;
    private static float stepX;
    private static float stepY;
    private static float x;
    private static float y;
    private static List<Vector2> _points = new List<Vector2>();
    public static List<Overlap> Gridcast(Vector2 _start, Vector2 _end) {

        debugPositions.Clear();
        debugValues1.Clear();
        debugValues2.Clear();

        // length of ray
        diffX = _end.x - _start.x;
        diffY = _end.y - _start.y;

        // x- and y-ratios
        XperY = Mathf.Sqrt(Mathf.Pow((diffX / diffY), 2));
        YperX = Mathf.Sqrt(Mathf.Pow((diffY / diffX), 2));
        if(diffY == 0)
            XperY = 1;
        if (diffX == 0)
            YperX = 1;

        // rounded start and end
        roundedStartX = Mathf.Floor(Mathf.Abs(_start.x)) * Mathf.Sign(_start.x); // special flooring so negative values go up (-10.5 becomes -10, not -11)
        roundedStartY = Mathf.Floor(Mathf.Abs(_start.y)) * Mathf.Sign(_start.y);
        roundedEndX = Mathf.Floor(Mathf.Abs(_end.x)) * Mathf.Sign(_end.x);
        roundedEndY = Mathf.Floor(Mathf.Abs(_end.y)) * Mathf.Sign(_end.y);
        

        // increment direction
        stepX = diffX == 0 ? 0 : Mathf.Sign(diffX);
        stepY = diffY == 0 ? 0 : Mathf.Sign(diffY);

        // pos ranging 0-1 to indicate distance to next grid-x or grid-y
        // x = ((_start.x - roundedStartX) * stepX) + 0.5f;
        // y = ((_start.y - roundedStartY) * stepY) + 0.5f;

        x = Mathf.Abs(_start.x - roundedStartX);
        if(stepX != 0 && stepX != Mathf.Sign(_start.x))
            x = 1 - x;
        y = Mathf.Abs(_start.y - roundedStartY);
        if(stepY != 0 && stepY != Mathf.Sign(_start.y))
            y = 1 - y;

        //x = Mathf.Max(0.001f, x);
        //y = Mathf.Max(0.001f, y);

        //Debug.Log((x + ", " + y).ToString().Color(Color.green));
       // Debug.Log((_start.x + " (" + stepX + ")" + " (" + x + "), " + _start.y + " (" + stepY + ")" + " (" + y + ")").ToString().Color(Color.cyan));
        //Debug.Log(_start.x + ", " + roundedStartX);

        // roundedDownStartX = roundedStartX < _start.x;
        // roundedDownStartY = roundedStartY < _start.y;
        // roundedDownEndX = roundedEndX < _end.x;
        // roundedDownEndY = roundedEndY < _end.y;
        // roundedStartX += roundedDownStartX ? 0.5f : -0.5f;
        // roundedStartY += roundedDownStartY ? 0.5f : -0.5f;
        // roundedEndX += roundedDownEndX ? 0.5f : -0.5f;
        // roundedEndY += roundedDownEndY ? 0.5f : -0.5f;

        roundedStartX += Mathf.Sign(_start.x) * 0.5f;
        roundedStartY += Mathf.Sign(_start.y) * 0.5f;
        roundedEndX += Mathf.Sign(_end.x) * 0.5f;
        roundedEndY += Mathf.Sign(_end.y) * 0.5f;


        // Debug.Log(x.ToString().Color(Color.cyan));
        // Debug.Log(y.ToString().Color(Color.cyan));

        // debugX = _start.x;
        // debugY = _start.y;

        // tiles found
        List<Overlap> tiles = new List<Overlap>();
        tiles.Add(new Overlap(new Vector2(roundedStartX, roundedStartY)));

        int douche = 0;
        // used for making sure that the last tile also does diagonal stuff
        float currentTileX = roundedStartX;
        float currentTileY = roundedStartY;
        float futureX = 0;
        float futureY = 0;
        float totalX = _start.x; //roundedStartX + (-0.5f * stepX);
        float totalY = _start.y;// roundedStartY + (-0.5f * stepY);
        bool forceAnotherIteration = false;
        while (((roundedStartX != roundedEndX || roundedStartY != roundedEndY) && douche < 10000)/* || forceAnotherIteration*/)
        {
            if (roundedStartX != roundedStartY && (stepX > 0 && roundedStartX > roundedEndX || stepX < 0 && roundedStartX < roundedEndX)) { 
                Debug.LogErrorFormat("Gridcast failed! ({0} -> {1}, {2}/{3} | {4} ({5}), {6} ({7})".Color(Color.red), _start.ToString(), _end.ToString(), futureX.ToString(), futureY.ToString(), x.ToString(), XperY.ToString(), y.ToString(), YperX.ToString());
                break;
            }
            if (roundedStartY != roundedEndY && (stepY > 0 && roundedStartY > roundedEndY || stepY < 0 && roundedStartY < roundedEndY)) { 
                Debug.LogErrorFormat("Gridcast failed! ({0} -> {1}, {2}/{3} | {4} ({5}), {6} ({7})".Color(Color.red), _start.ToString(), _end.ToString(), futureX.ToString(), futureY.ToString(), x.ToString(), XperY.ToString(), y.ToString(), YperX.ToString());
                break;
            }
            
            forceAnotherIteration = false;
            //Debug.Log("(" + currentTileX + ", " + currentTileY + ") - (" + roundedEndX + ", " + roundedEndY + ")");
            douche++;
            currentTileX = roundedStartX;
            currentTileY = roundedStartY;

            // truncate predicted x/y to three decimal points (tried two, but appears to cause noticeable inaccuracy)
            // futureX = Mathf.Round((x + (XperY * (1 - y))) * 1000) / 1000f;
            // futureY = Mathf.Round((y + (YperX * (1 - x))) * 1000) / 1000f;
            futureX = x + (XperY * (1 - y));
            futureY = y + (YperX * (1 - x));
            if(float.IsNaN(futureX) || float.IsInfinity(futureX))
                futureX = 0;
            if (float.IsNaN(futureY) || float.IsInfinity(futureY))
                futureY = 0;

            totalX = stepX > 0 ? totalX + (Mathf.Clamp01(futureX) - x) : totalX - (Mathf.Clamp01(futureX) - x);
            totalY = stepY > 0 ? totalY + (Mathf.Clamp01(futureY) - y) : totalY - (Mathf.Clamp01(futureY) - y);

            if (futureX >= 0.999f && futureY >= 0.999f){ // exiting tile diagonally (approximate, because rays shot at vertices always seem to miss :/ )

                tiles.Add(new Overlap(
                    new Vector2(roundedStartX + stepX, roundedStartY + stepY),
                    new Vector2[] {
                        new Vector2(roundedStartX + stepX, roundedStartY),
                        new Vector2(roundedStartX, roundedStartY + stepY)
                    })
                );

                debugPositions.Add(new Vector2(totalX, totalY));
                debugValues1.Add(new Vector2(roundedEndX + Mathf.Clamp01(futureX), roundedEndY + Mathf.Clamp01(futureY)));
                debugValues2.Add(new Vector2(1, 1));

                roundedStartX += stepX;
                roundedStartY += stepY;
                forceAnotherIteration = true;

                // can't be zeroed like the others, since x/y might not be exactly 1, as we're approximating diagonal overlapping
                x = 1 - futureX;
                y = 1 - futureY;

                // check if the extra neighbours we added was the end
                if (currentTileX + stepX == roundedEndX && currentTileY == roundedEndY)
                    break;
                if (currentTileX == roundedEndX && currentTileY + stepY == roundedEndY)
                    break;
            }
            else if (futureX >= 1)
            { // if the x we'll get from the remaining y is greater than the y we'll get from the remaining x...

                // debugX += 1 - x;
                // debugY += YperX * (1 - x);
                debugPositions.Add(new Vector2(totalX, totalY));
                debugValues1.Add(new Vector2(roundedEndX + Mathf.Clamp01(futureX), roundedEndY + Mathf.Clamp01(futureY)));
                debugValues2.Add(new Vector2(Mathf.Clamp01(futureX), Mathf.Clamp01(futureY)));

                // move y to where it should be at end of x and zero x because new tile
                x = 0;
                y = Mathf.Clamp01(futureY);


                roundedStartX += stepX;
                tiles.Add(new Overlap(new Vector2(roundedStartX, roundedStartY)));
            }
            else
            { // y + (YperX * (1 - x)) > 1

                // debugX += -(XperY * (1 - y));
                // debugY += 1 - y;
                debugPositions.Add(new Vector2(totalX, totalY));
                debugValues1.Add(new Vector2(roundedEndX + Mathf.Clamp01(futureX), roundedEndY + Mathf.Clamp01(futureY)));
                debugValues2.Add(new Vector2(Mathf.Clamp01(futureX), Mathf.Clamp01(futureY)));

                x = Mathf.Clamp01(futureX);
                y = 0;


                roundedStartY += stepY;
                tiles.Add(new Overlap(new Vector2(roundedStartX, roundedStartY)));
            }
        }
        if (douche >= 10000)
            Debug.LogErrorFormat("Hmm, still douching.");
        //else
            //Debug.LogFormat("{0}/{1} | {2} ({3}), {4} ({5})".Color(Color.cyan), futureX.ToString(), futureY.ToString(), x.ToString(), XperY.ToString(), y.ToString(), YperX.ToString());

        //futureX = Mathf.Round((x + (XperY * (1 - y))) * 1000) / 1000f;


        // Vector2 v;
        // for (int i = 0; i < tiles.Count; i++){
        //     v = tiles[i];
        //     v.x += roundedDownStartX ? 0.5f : -0.5f;
        //     v.y += roundedDownStartY ? 0.5f : -0.5f;
        //     tiles[i] = v;
        // }

        lastCastStart = _start;
        lastCastEnd = _end;
        return tiles;
    }
    public static List<BresenhamsLine.Overlap> ReplayGridcast() {
        Debug.Log("Replaying: (" + lastCastStart + ") -> (" + lastCastEnd + ")");
        return Gridcast(lastCastStart, lastCastEnd);
    }


    //    function line(x0, y0, x1, y1)
    //  local vx, vy = x1 - x0, y1-y0           -- get the differences
    //     local dx = math.sqrt(1 + (vy/vx)^2)  -- length of vector<1, slope>
    //     local dy = math.sqrt(1 + (vx/vy)^2)  -- length of vector<1/slope, 1>

    //     local ix,iy = math.floor(x0), math.floor(y0) -- initialize starting positions
    //     local sx,ex -- sx is the increment direction
    //              -- ex is the distance from x0 to ix
    //  if vx< 0 then
    //    sx = -1
    //    ex = (x0-ix) * dx
    //  else
    //    sx = 1
    //    ex = (ix + 1-x0) * dx -- subtract from 1 instead of 0
    //                          -- to make up for flooring ix
    //  end

    //  local sy,ey
    //  if vy< 0 then
    //    sy = -1
    //    ey = (y0-iy) * dy
    //  else
    //    sy = 1
    //    ey = (iy + 1-y0) * dy
    //  end

    //  local done = false
    //  local len = math.sqrt(vx ^ 2 + vy ^ 2)
    //  return function()
    //    if math.min(ex,ey) <= len then
    //      local rx, ry = ix, iy
    //      if ex<ey then
    //        ex = ex + dx
    //        ix = ix + sx
    //      else
    //        ey = ey + dy
    //        iy = iy + sy
    //      end
    //      return rx, ry
    //    elseif not done then -- return the final two coordinates
    //      done = true
    //      return ix, iy
    //    end
    //  end
    //end

    //private static int resolution;

    //// use Bresenham-like algorithm to print a line from (y1,x1) to (y2,x2) 
    //// The difference with Bresenham is that ALL the points of the line are 
    //// printed, not only one per x coordinate. 
    //// Principles of the Bresenham's algorithm (heavily modified) were taken from: 
    //// http://www.intranet.ca/~sshah/waste/art7.html 
    //private static List<Vector2> superCover = new List<Vector2>();
    //public static List<Vector2> GetAllCasesCovered(Vector2 _start, Vector2 _end, int _resolution = 1) {
    //    resolution = _resolution;
    //    superCover.Clear();

    //    int x1 = (int)(_start.x * resolution);
    //    int y1 = (int)(_start.y * resolution);
    //    int x2 = (int)(_end.x * resolution);
    //    int y2 = (int)(_end.y * resolution);
    //    int i;               // loop counter 
    //    int ystep, xstep;    // the step on y and x axis 
    //    int error;           // the error accumulated during the increment 
    //    int errorprev;       // *vision the previous value of the error variable 
    //    int y = y1, x = x1;  // the line points 
    //    int ddy, ddx;        // compulsory variables: the double values of dy and dx 
    //    int dx = x2 - x1;
    //    int dy = y2 - y1;

    //    AddPoint(x1, y1); // first point 
    //                      // NB the last point can't be here, because of its previous point (which has to be verified) 
    //    if (dy < 0) {
    //        ystep = -1;
    //        dy = -dy;
    //    }
    //    else
    //        ystep = 1;
    //    if (dx < 0) {
    //        xstep = -1;
    //        dx = -dx;
    //    }
    //    else
    //        xstep = 1;
    //    ddy = 2 * dy;  // work with double values for full precision 
    //    ddx = 2 * dx;
    //    if (ddx >= ddy) {  // first octant (0 <= slope <= 1) 
    //                       // compulsory initialization (even for errorprev, needed when dx==dy) 
    //        errorprev = error = dx;  // start in the middle of the square 
    //        for (i = 0; i < dx; i++) {  // do not use the first point (already done) 
    //            x += xstep;
    //            error += ddy;
    //            if (error > ddx) {  // increment y if AFTER the middle ( > ) 
    //                y += ystep;
    //                error -= ddx;
    //                // three cases (octant == right->right-top for directions below): 
    //                if (error + errorprev < ddx)  // bottom square also 
    //                    AddPoint(x, y - ystep);
    //                else if (error + errorprev > ddx)  // left square also 
    //                    AddPoint(x - xstep, y);
    //                else {  // corner: bottom and left squares also 
    //                    AddPoint(x, y - ystep);
    //                    AddPoint(x - xstep, y);
    //                }
    //            }
    //            AddPoint(x, y);
    //            errorprev = error;
    //        }
    //    }
    //    else {  // the same as above 
    //        errorprev = error = dy;
    //        for (i = 0; i < dy; i++) {
    //            y += ystep;
    //            error += ddx;
    //            if (error > ddy) {
    //                x += xstep;
    //                error -= ddy;
    //                if (error + errorprev < ddy)
    //                    AddPoint(x - xstep, y);
    //                else if (error + errorprev > ddy)
    //                    AddPoint(x, y - ystep);
    //                else {
    //                    AddPoint(x - xstep, y);
    //                    AddPoint(x, y - ystep);
    //                }
    //            }
    //            AddPoint(x, y);
    //            errorprev = error;
    //        }
    //    }
    //    // assert ((y == y2) && (x == x2));  // the last point (y2,x2) has to be the same with the last point of the algorithm 
    //    return superCover;
    //}

    //private static Vector2 newPoint = new Vector2();
    //private static void AddPoint(int _x, int _y) {
    //    newPoint.x = _x * (1 / (float)resolution);
    //    newPoint.y = _y * (1 / (float)resolution);
    //    superCover.Add(newPoint);
    //}

    //   Vector2 start;
    //   Vector2 end;
    //   float resolution;

    //   public BresenhamsLine( Vector2 _start, Vector2 _end, float _resolution )
    //   {
    //       resolution = _resolution;
    //       start = _start * resolution;
    //       end = _end * resolution;
    //   }


    //Vector2 result;
    //   int xd, yd;
    //   int x, y;
    //   int ax, ay;
    //   int sx, sy;
    //   int dx, dy;
    //   public IEnumerator GetEnumerator()
    //   {
    //       dx = (int)(end.x - start.x);
    //       dy = (int)(end.y - start.y);

    //       ax = Mathf.Abs( dx ) << 1;
    //       ay = Mathf.Abs( dy ) << 1;

    //       sx = (int)Mathf.Sign( (float) dx );
    //       sy = (int)Mathf.Sign( (float) dy );

    //       x = (int)start.x;
    //       y = (int)start.y;

    //       if( ax >= ay ) // x dominant
    //       {
    //           yd = ay - ( ax >> 1 );
    //           for( ; ; )
    //           {
    //               result.x = (int)( x );
    //               result.y = (int)( y );
    //               result.x *= (1 / resolution);
    //               result.y *= (1 / resolution);
    //               yield return result;

    //               if( Mathf.Abs(x) >= Mathf.CeilToInt(Mathf.Abs(end.x)))
    //                   yield break;

    //               if( yd >= 0 )
    //               {
    //                   y += sy;
    //                   yd -= ax;
    //               }

    //               x += sx;
    //               yd += ay;
    //           }
    //       }
    //       else if( ay >= ax ) // y dominant
    //       {
    //           xd = ax - ( ay >> 1 );
    //           for( ; ; )
    //           {
    //               result.x = (int)( x );
    //               result.y = (int)( y );
    //               result.x *= (1 / resolution);
    //               result.y *= (1 / resolution);
    //               yield return result;

    //               if (Mathf.Abs(y) >= Mathf.CeilToInt(Mathf.Abs(end.y)))
    //                   yield break;

    //               if( xd >= 0 )
    //               {
    //                   x += sx;
    //                   xd -= ay;
    //               }

    //               y += sy;
    //               xd += ax;
    //           }
    //       }
    //   }
}