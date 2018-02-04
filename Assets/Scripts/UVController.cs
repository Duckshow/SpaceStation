using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UVController : UVControllerBasic {

	private UVControllerBasic MyTopUVC;

	private byte colorIndex_0;
    private byte colorIndex_1;
    private byte colorIndex_2;
    private byte colorIndex_3;
    private byte colorIndex_4;
    private byte colorIndex_5;
    private byte colorIndex_6;
    private byte colorIndex_7;
    private byte colorIndex_8;
    private byte colorIndex_9;
    private byte setColorIndex_0;
	private byte setColorIndex_1;
	private byte setColorIndex_2;
	private byte setColorIndex_3;
	private byte setColorIndex_4;
	private byte setColorIndex_5;
    private byte setColorIndex_6;
    private byte setColorIndex_7;
    private byte setColorIndex_8;
    private byte setColorIndex_9;

	public Color[] VColors = new Color[9];


	public override void Setup() {
        base.Setup();
		ChangeColor (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
    }

	[System.Serializable]
	public class DebugData{
		public GridLayerEnum Layer;
		public Vector2i AssetCoordinates;
		public bool Temporary;
		public DebugData(GridLayerEnum _layer, Vector2i _coord, bool _temporary){
			Layer = _layer;
			AssetCoordinates = _coord;
			Temporary = _temporary;
		}
	}
	public List<DebugData> StackTrace = new List<DebugData>();

	public override void StopTempMode(){
		base.StopTempMode();
		if (MyTopUVC != null)
			MyTopUVC.StopTempMode();
	}
	public override void ChangeAsset(GridLayerEnum _layer, Vector2i _assetCoordinates, bool _temporary){
		
		StackTrace.Add(new DebugData(_layer, _assetCoordinates, _temporary));

		if (_layer == GridLayerEnum.Top){
			if (_assetCoordinates != CachedAssets.WallSet.Null){
				if (MyTopUVC == null){
					MyTopUVC = ObjectPooler.Instance.GetPooledObject<UVControllerBasic>(CachedAssets.WallSet.Purpose.UVControllerBasic);
					MyTopUVC.transform.parent = transform;
					MyTopUVC.transform.position = transform.position;
					MyTopUVC.SortAboveActors = true;
					MyTopUVC.Sort(Grid.Instance.GetTileFromWorldPoint(transform.position).GridCoord.y);
				}

				MyTopUVC.ChangeAsset(_layer, _assetCoordinates, _temporary);
				if(_temporary && GridLayers[(int)GridLayerEnum.TopCorners].TemporaryCoordinates != null)
					MyTopUVC.ChangeAsset(GridLayerEnum.TopCorners, GridLayers[(int)GridLayerEnum.TopCorners].TemporaryCoordinates, _temporary);
				else if(!_temporary && GridLayers[(int)GridLayerEnum.TopCorners].Coordinates != null)
					MyTopUVC.ChangeAsset(GridLayerEnum.TopCorners, GridLayers[(int)GridLayerEnum.TopCorners].Coordinates, _temporary);
			}
			else if (MyTopUVC != null){
				MyTopUVC.ChangeAsset(_layer, _assetCoordinates, _temporary);
				MyTopUVC.ChangeAsset(GridLayerEnum.TopCorners, _assetCoordinates, _temporary);
				MyTopUVC.TotallySetTheBool = false;

				MyTopUVC.GetComponent<PoolerObject>().ReturnToPool();
				MyTopUVC = null;
			}
		}
		
		if (_layer == GridLayerEnum.TopCorners && MyTopUVC != null){
			MyTopUVC.TotallySetTheBool = true;
			MyTopUVC.ChangeAsset(_layer, _assetCoordinates, _temporary);
		}

		base.ChangeAsset(_layer, _assetCoordinates, _temporary);
	}

	public void ChangeColor(byte _colorIndex, bool _temporary) {
        if (!hasStarted)
            Setup();

        ChangeColor(_colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _temporary);
    }
	public void ResetColor() {
        ChangeColor(setColorIndex_0, setColorIndex_1, setColorIndex_2, setColorIndex_3, setColorIndex_4, setColorIndex_5, setColorIndex_6, setColorIndex_7, setColorIndex_8, setColorIndex_9, false);
    }
    private Vector4 UV01234 = new Vector4();
    private Vector4 colorIndices = new Vector4();
    private List<Vector4> allColorIndices = new List<Vector4>();
    public void ChangeColor(byte _color0, byte _color1, byte _color2, byte _color3, byte _color4, byte _color5, byte _color6, byte _color7, byte _color8, byte _color9, bool _temporarily) {
        if (!_temporarily) {
			setColorIndex_0 = _color0;
			setColorIndex_1 = _color1;
			setColorIndex_2 = _color2;
			setColorIndex_3 = _color3;
			setColorIndex_4 = _color4;
			setColorIndex_5 = _color5;
            setColorIndex_6 = _color6;
            setColorIndex_7 = _color7;
            setColorIndex_8 = _color8;
            setColorIndex_9 = _color9;
        }

		// NOTE: these do nothing??
        colorIndex_0 = _color0;
        colorIndex_1 = _color1;
        colorIndex_2 = _color2;
        colorIndex_3 = _color3;
        colorIndex_4 = _color4;
        colorIndex_5 = _color5;
        colorIndex_6 = _color6;
        colorIndex_7 = _color7;
        colorIndex_8 = _color8;
        colorIndex_9 = _color9;

		colorIndices.x = BitCompressor.Byte4ToInt(colorIndex_0, colorIndex_1, colorIndex_2,colorIndex_3);
		colorIndices.y = BitCompressor.Byte4ToInt(colorIndex_4, colorIndex_5, colorIndex_6, colorIndex_7);
		colorIndices.z = BitCompressor.Byte4ToInt(colorIndex_8, colorIndex_9, 0, 0);

        allColorIndices.Clear();
		for (int i = 0; i < MyMeshFilter.mesh.uv.Length; i++)
            allColorIndices.Add(colorIndices);

		shouldApplyChanges = true;
    }

    private List<Vector4> doubleDots = new List<Vector4>();
	public void SetUVDots(int _specificUV, Vector2 _dot0, Vector2 _dot1, Vector2 _dot2, Vector2 _dot3){
		// setup list for caching UVs
		if (doubleDots.Count != MyMeshFilter.mesh.vertexCount) {
			doubleDots.Clear();
			for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++)
				doubleDots.Add(new Vector4());
		}

		// apply UVs
        Vector4 _doubleDot = doubleDots[_specificUV];
        _doubleDot.x = BitCompressor.Int2ToInt((int)(_dot0.x * DOT_PRECISION), (int)(_dot0.y * DOT_PRECISION));
        _doubleDot.y = BitCompressor.Int2ToInt((int)(_dot1.x * DOT_PRECISION), (int)(_dot1.y * DOT_PRECISION));
        _doubleDot.z = BitCompressor.Int2ToInt((int)(_dot2.x * DOT_PRECISION), (int)(_dot2.y * DOT_PRECISION));
        _doubleDot.w = BitCompressor.Int2ToInt((int)(_dot3.x * DOT_PRECISION), (int)(_dot3.y * DOT_PRECISION));
		doubleDots[_specificUV] = _doubleDot;
		shouldApplyChanges = true;
    }

	private List<Color32> vertexColors = new List<Color32>();
	public void SetVertexColor(int _specificVertex, Color32 _color){
		// setup list for caching colors
		if (vertexColors.Count != MyMeshFilter.mesh.vertexCount){
			vertexColors = new List<Color32>(MyMeshFilter.mesh.vertexCount);
			for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++)
				vertexColors.Add(new Color32());
		}

		VColors[_specificVertex] = _color;
		vertexColors[_specificVertex] = _color;
		shouldApplyChanges = true;
	}

	protected override void ApplyAssetChanges(){
		//Debug.Log(sAllColorIndices.Count + ", " + sUVDoubleDots.Count + ", " + sVertexColors.Count);
		MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices);
		MyMeshFilter.mesh.SetUVs(UVCHANNEL_DOUBLEDOT, doubleDots);
		MyMeshFilter.mesh.SetColors(vertexColors);

		if (MyTopUVC != null){
			MyTopUVC.MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices);
			MyTopUVC.MyMeshFilter.mesh.SetUVs(UVCHANNEL_DOUBLEDOT, doubleDots);
			MyTopUVC.MyMeshFilter.mesh.SetColors(vertexColors);
		}

		base.ApplyAssetChanges();
	}
}
