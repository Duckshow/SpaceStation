using UnityEngine;
using System.Collections;

public class SetRenderQueue : MonoBehaviour{
    public int renderQueue = 100;

    void Start(){
        Material mat = GetComponent<Renderer>().material;
        mat.renderQueue = renderQueue;
    }
}