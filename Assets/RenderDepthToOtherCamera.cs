using UnityEngine;

public class RenderDepthToOtherCamera : MonoBehaviour {
    [SerializeField] private RenderDepthFromOtherCamera Partner;
    private Camera thisCamera;
    private RenderTexture rt1;
    private RenderTexture rt2;
    void Awake() {
        thisCamera = GetComponent<Camera>();
    }

    void OnPreRender() {
        rt1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        if (rt2 == null)
            rt2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        Graphics.SetRenderTarget(rt1.colorBuffer, rt2.depthBuffer);
        Partner.Texture = rt2;
    }
    void OnPostRender() {
        rt1.Release();
    }
}
