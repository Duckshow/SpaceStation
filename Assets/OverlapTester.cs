using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapTester : MonoBehaviour {

    public Transform pointTransform;
    private Vector2 point;
    public Transform[] vectorTransforms;
    private Vector2[] vertices;

    private bool intersect = false;
    private int j;
    [EasyButtons.Button]
    public void OverlapPointOrAlmost() {
        point = pointTransform.position;

        vertices = new Vector2[vectorTransforms.Length];
        for (int i = 0; i < vectorTransforms.Length; i++) {
            vertices[i] = vectorTransforms[i].position;
        }

        intersect = false;

        j = vertices.Length - 1;

        for (int v = 0; v < vertices.Length; j = v++) {
            // stolen from the internets D:
            if ((((vertices[v].y <= point.y && point.y < vertices[j].y) || (vertices[j].y <= point.y && point.y < vertices[v].y)) &&
                (point.x < (vertices[j].x - vertices[v].x) * (point.y - vertices[v].y) / (vertices[j].y - vertices[v].y) + vertices[v].x)) ||
                (((Vector2)transform.position + vertices[v]) - point).magnitude <= Tile.PIXEL_RADIUS) // if not overlapping, check if the distance is less or equal a pixel's radius.
                intersect = !intersect;
        }

        if (intersect)
            Debug.Log("True!");
        else
           Debug.Log("False.");
    }
}
