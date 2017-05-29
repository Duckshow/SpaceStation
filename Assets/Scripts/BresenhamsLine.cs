using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class BresenhamsLine : IEnumerable
{
    Vector2 start;
    Vector2 end;
    float resolution;

    public BresenhamsLine( Vector2 _start, Vector2 _end, float _resolution )
    {
        resolution = _resolution;
        start = _start * resolution;
        end = _end * resolution;
    }
 
	Vector2 result;
    int xd, yd;
    int x, y;
    int ax, ay;
    int sx, sy;
    int dx, dy;
    public IEnumerator GetEnumerator()
    {
        dx = (int)(end.x - start.x);
        dy = (int)(end.y - start.y);
 
        ax = Mathf.Abs( dx ) << 1;
        ay = Mathf.Abs( dy ) << 1;
 
        sx = (int)Mathf.Sign( (float) dx );
        sy = (int)Mathf.Sign( (float) dy );
 
        x = (int)start.x;
        y = (int)start.y;
 
        if( ax >= ay ) // x dominant
        {
            yd = ay - ( ax >> 1 );
            for( ; ; )
            {
                result.x = (int)( x );
                result.y = (int)( y );
                result.x *= (1 / resolution);
                result.y *= (1 / resolution);
                yield return result;
 
                if( x == (int)end.x )
                    yield break;
 
                if( yd >= 0 )
                {
                    y += sy;
                    yd -= ax;
                }
 
                x += sx;
                yd += ay;
            }
        }
        else if( ay >= ax ) // y dominant
        {
            xd = ax - ( ay >> 1 );
            for( ; ; )
            {
                result.x = (int)( x );
                result.y = (int)( y );
                result.x *= (1 / resolution);
                result.y *= (1 / resolution);
                yield return result;
 
                if( y == (int)end.y )
                    yield break;
 
                if( xd >= 0 )
                {
                    x += sx;
                    xd -= ay;
                }
 
                y += sy;
                xd += ax;
            }
        }
    }
}