using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class BresenhamsLine // : IEnumerable
{

    private static List<Vector2> points = new List<Vector2>();
    public static List<Vector2> GridRay(Vector2 _start, Vector2 _end){
        points.Clear();

        float DX = _end.x - _start.x;
        float DY = _end.y - _start.y;

        float stepX = Mathf.Sign(DX);
        float stepY = Mathf.Sign(DY);

        float progressPerTile_X = Mathf.Abs(1 / DX); // 1 should be (Tile.Radius * 2), but hey, this is faster!
        float progressPerTile_Y = Mathf.Abs(1 / DY);

        float progress_X = progressPerTile_X * (float)Frac(_start.x); // _start.x should divide by Tile.Radius * 2 but since it's 1 currently, skip!
        float progress_Y = progressPerTile_Y * (float)Frac(_start.y); 

        Vector2 pos = _start;
        int i = 0;
        points.Add(pos);
        while(pos != _end && i < 500){
            i++;
            if(progress_X < progress_Y){
                progress_X += progressPerTile_X;
                pos.x += stepX;
            }
            else {
                progress_Y += progressPerTile_Y;
                pos.y += stepY;
            }

            points.Add(pos);
        }
        // if(i >= 500)
        // throw new System.Exception("GridRay appears to have missed its target!");

        return points;
    }
    public static float Frac(float _val){ 
        return Mathf.Abs(_val - (float)System.Math.Truncate(_val)); 
    }

    // public static List<Vector2> DDASuperCover(Vector2 _start, Vector2 _end) {
    //     float diffX = _end.x - _start.x;
    //     float diffY = _end.y - _start.y; // get the differences
    //     float dx = Mathf.Sqrt(1 + Mathf.Pow((diffY / diffX), 2)); // length of vector < 1, slope >
    //     float dy = Mathf.Sqrt(1 + Mathf.Pow((diffX / diffY), 2)); // length of vector < 1 / slope, 1 >

    //     float flooredStartX = Mathf.Floor(_start.x);
    //     float flooredStartY = Mathf.Floor(_start.y); // initialize starting positions

    //     float stepX; // sx is the increment direction
    //     float ex; // ex is the distance from _start.x to ix
    //     if (diffX < 0) {
    //         stepX = -1;
    //         ex = (_start.x - flooredStartX) * dx;
    //     }
    //     else {
    //         stepX = 1;
    //         ex = (flooredStartX + 1 - _start.x) * dx; // subtract from 1 instead of 0 to make up for flooring ix
    //     }

    //     float stepY;
    //     float ey;
    //     if (diffY < 0) {
    //         stepY = -1;
    //         ey = (_start.y - flooredStartY) * dy;
    //     }
    //     else {
    //         stepY = 1;
    //         ey = (flooredStartY + (1 - _start.y)) * dy;
    //     }

    //     float length = Mathf.Sqrt(Mathf.Pow(diffX, 2) + Mathf.Pow(diffY, 2));
    //     List<Vector2> _points = new List<Vector2>();
    //     while (Mathf.Min(ex, ey) <= length) {
    //         _points.Add(new Vector2(flooredStartX, flooredStartY));

    //         if (ex < ey) {
    //             ex += dx;
    //             flooredStartX += stepX;
    //         }
    //         else {
    //             ey += dy;
    //             flooredStartY += stepY;
    //         }
    //     }
    //     _points.Add(new Vector2(flooredStartX, flooredStartY));
    //     return _points;
    // }


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