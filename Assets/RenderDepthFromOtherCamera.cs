using UnityEngine;

public class RenderDepthFromOtherCamera : MonoBehaviour {
    [System.NonSerialized] public RenderTexture Texture;
    private Camera thisCamera;

    void Awake() {
        thisCamera = GetComponent<Camera>();
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (Texture != null) {
            Debug.Log(thisCamera);
            Debug.Log(source);
            thisCamera.SetTargetBuffers(source.colorBuffer, Texture.depthBuffer);
        }
        Graphics.Blit(source, destination);
    }
}