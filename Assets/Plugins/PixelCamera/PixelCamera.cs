using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class PixelCamera : MonoBehaviour {

	//upgrade logic
	[SerializeField][HideInInspector]
	private Camera TargetCamera;
	
	public Camera[] TargetCameras;
	
	public enum ScaleModes {
		BasedOnWidth,
		BasedOnHeight,
		BasedOnBoth,
	}
	
	public enum ColorDepths {
		Default,
		_12bits,
		_16bits,
		_24bits,
	}
	
	public ScaleModes ScaleMode = ScaleModes.BasedOnHeight;
	//public ColorDepths ColorDepth = ColorDepths.Default;
	private const ColorDepths ColorDepth = ColorDepths.Default;
	public int PixelWidth = 320;
	public int PixelHeight = 240;
	public Material CustomMaterial;
	
	private int m_lastWidth = -1;
	private int m_lastHeight = -1;
	private ColorDepths m_lastColorDepth;
	private Material m_lastCustomMaterial;
	
	private RenderTexture m_texture;
	private Renderer m_quad;
	private Material m_material;
	
	private void Update () {
		PixelWidth = Mathf.Max(1, PixelWidth);
		PixelHeight = Mathf.Max(1, PixelHeight);
		
		var camera = GetComponent<Camera>();
		if(camera != null) {
			float actualAspect = camera.aspect * camera.rect.height / camera.rect.width;

			switch(ScaleMode) {
			case ScaleModes.BasedOnHeight:
				PixelWidth = Mathf.RoundToInt(PixelHeight * actualAspect);
				break;
			case ScaleModes.BasedOnWidth:
				PixelHeight = Mathf.RoundToInt(PixelWidth / actualAspect);
				break;
			}
		}
		
		if(!Application.isPlaying) {
			UpdateTexture(false);
		}
		
		if(m_lastWidth != PixelWidth || m_lastHeight != PixelHeight || 
		   m_lastColorDepth != ColorDepth || m_lastCustomMaterial != CustomMaterial) {
			UpdateTexture();
			
			m_lastWidth = PixelWidth;
			m_lastHeight = PixelHeight;
			m_lastColorDepth = ColorDepth;
			m_lastCustomMaterial = CustomMaterial;
		}
		
		if(TargetCameras != null) {
			foreach(var targetCamera in TargetCameras) {
				if(targetCamera == null) continue;

				if(targetCamera.targetTexture != m_texture || m_texture == null) {
					UpdateTexture();
				}
			}
		}
		
		transform.localScale = Vector3.one;
		transform.position = 99999 * Vector3.down;
	}
	
	private void OnEnable() {
		//upgrade logic
		if(TargetCamera != null) {
			TargetCameras = new Camera[1];
			TargetCameras[0] = TargetCamera;
			TargetCamera = null;
		}

		UpdateTexture();
	}
	
	private void OnDisable() {
		ClearTargetTextures();
	}

	private void ClearTargetTextures() {
		if(TargetCameras != null) {
			foreach(var camera in TargetCameras) {
				if(camera == null) continue;

				camera.targetTexture = null;
			}
		}
	}
	
	private void UpdateTexture(bool p_forceRefresh = true) {
		if(TargetCameras == null || TargetCameras.Length == 0) {
			return;
		}
		
		if(p_forceRefresh || m_texture == null) {
			if(m_texture != null) {
				ClearTargetTextures();
				m_texture.Release();
				DestroyImmediate(m_texture);
			}

			var format = RenderTextureFormat.Default;
			/*switch(ColorDepth) {
			case ColorDepths._12bits:
				format = RenderTextureFormat.ARGB4444;
				break;
			case ColorDepths._16bits:
				format = RenderTextureFormat.RGB565;
				break;
			case ColorDepths._24bits:
				format = RenderTextureFormat.ARGB32;
				break;
			}*/

			m_texture = new RenderTexture(PixelWidth, PixelHeight, 24, format);
			m_texture.name = "PixelCamera RTT";
			m_texture.Create();
			m_texture.filterMode = FilterMode.Point;
		}

		Camera firstCamera = null;
		foreach(var targetCamera in TargetCameras) {
			if(targetCamera == null) continue;

			if(firstCamera == null) {
				firstCamera = targetCamera;
			}

			targetCamera.targetTexture = m_texture;
		}

		if(firstCamera == null) {
			return;
		}
		
		var camera = GetComponent<Camera>();
		if(camera == null) {
			camera = gameObject.AddComponent<Camera>();
		}
		camera.orthographic = true;
		camera.orthographicSize = 0.5f;
		camera.farClipPlane = 2.0f;
		camera.depth = firstCamera.depth;
		camera.rect = firstCamera.rect;
		
		if(m_quad == null) {
			var oldChildren = transform.GetComponentsInChildren<Transform>();
			foreach(var child in oldChildren) {
				if(child != transform) {
					DestroyImmediate(child.gameObject);
				}
			}
			
			m_quad = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshRenderer>();
			DestroyImmediate(m_quad.GetComponent<Collider>());
		}
		m_quad.transform.parent = transform;
		m_quad.transform.localPosition = 1.0f * Vector3.forward;
		if(camera.pixelHeight > 0) {
			m_quad.transform.localScale = new Vector3(
				(float)camera.pixelWidth / camera.pixelHeight,
				1f, 1f);
		} else {
			m_quad.transform.localScale = Vector3.one;
		}

		if(CustomMaterial == null) {
			if(m_material == null) {
				m_material = new Material(Shader.Find("Unlit/Texture"));
			}
		} else {
			m_material = CustomMaterial;
		}
		m_material.mainTexture = m_texture;
		m_quad.material = m_material;
	}
}
