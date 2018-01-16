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

    public override void Setup() {
        base.Setup();
		ChangeColor (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
    }

	public override void ChangeAsset(GridLayerEnum _layer, DoubleInt _assetCoordinates, bool _temporary){
		if (_layer == GridLayerEnum.Top){
			if (MyTopUVC == null) { 
				MyTopUVC = ObjectPooler.Instance.GetPooledObject<UVControllerBasic>(CachedAssets.WallSet.Purpose.UVControllerBasic);
				MyTopUVC.transform.parent = transform;
				MyTopUVC.transform.position = transform.position;
				MyTopUVC.SortAboveActors = true;
				MyTopUVC.Sort(Grid.Instance.GetTileFromWorldPoint(transform.position).GridCoord.Y);
			}
			if (_assetCoordinates == CachedAssets.WallSet.Null){
				MyTopUVC.GetComponent<PoolerObject>().ReturnToPool();
				MyTopUVC = null;
			}
			else{
				MyTopUVC.ChangeAsset(_layer, _assetCoordinates, _temporary);
			}
		}
		else
			base.ChangeAsset(_layer, _assetCoordinates, _temporary);
	}

	public void ChangeColor(byte _colorIndex, bool _temporary) {
        if (!hasStarted)
            Setup();

        ChangeColor(_colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _temporary);
    }
    private static Vector4 sUV01234 = new Vector4();
    private static Vector4 sColorIndices = new Vector4();
    private static List<Vector2> sAllUVs = new List<Vector2>();
    private static List<Vector4> sAllColorIndices = new List<Vector4>();
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

        sAllUVs.Clear();
        sAllColorIndices.Clear();

		AssignBytesToVectorChannel(ref sColorIndices.x, colorIndex_0, colorIndex_1, colorIndex_2, colorIndex_3);
		AssignBytesToVectorChannel(ref sColorIndices.y, colorIndex_4, colorIndex_5, colorIndex_6, colorIndex_7);
		AssignBytesToVectorChannel(ref sColorIndices.z, colorIndex_8, colorIndex_9, 0, 0);

		for (int i = 0; i < myMeshFilter.mesh.uv.Length; i++)
            sAllColorIndices.Add(sColorIndices);

        myMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, sAllColorIndices);
    }
	void AssignBytesToVectorChannel(ref float _channel, byte _index0, byte _index1, byte _index2, byte _index3) {
		_channel = _index0 | (_index1 << 8) | (_index2 << 16) | (_index3 << 24);
	}
	public void ResetColor() {
        ChangeColor(setColorIndex_0, setColorIndex_1, setColorIndex_2, setColorIndex_3, setColorIndex_4, setColorIndex_5, setColorIndex_6, setColorIndex_7, setColorIndex_8, setColorIndex_9, false);
    }

    private static List<Vector4> sUVDoubleDots = new List<Vector4>();
	public void SetUVDoubleDot(int _specificUV, float _doubleDot0, float _doubleDot1, float _doubleDot2, float _doubleDot3){
		// only get UVs if this has run once
		if(sUVDoubleDots.Count > 0)
	        myMeshFilter.mesh.GetUVs(UVCHANNEL_DOUBLEDOT, sUVDoubleDots);
		
		// setup list for caching UVs
		if (sUVDoubleDots.Count != myMeshFilter.mesh.vertexCount) {
			sUVDoubleDots = new List<Vector4>(myMeshFilter.mesh.vertexCount);
			for (int i = 0; i < myMeshFilter.mesh.vertexCount; i++)
				sUVDoubleDots.Add(new Vector4());
		}

		// apply UVs
        Vector4 _doubleDot = sUVDoubleDots[_specificUV];
        _doubleDot.x = _doubleDot0;
        _doubleDot.y = _doubleDot1;
        _doubleDot.z = _doubleDot2;
        _doubleDot.w = _doubleDot3;
		sUVDoubleDots[_specificUV] = _doubleDot;
        myMeshFilter.mesh.SetUVs(UVCHANNEL_DOUBLEDOT, sUVDoubleDots);
    }

	private static List<Color32> sVertexColors = new List<Color32>();
	public void SetVertexColor(int _specificVertex, Color32 _color){
		// only get colors if this has run once
		if (sVertexColors.Count > 0)
			myMeshFilter.mesh.GetColors(sVertexColors);

		// setup list for caching colors
		if (sVertexColors.Count != myMeshFilter.mesh.vertexCount){
			sVertexColors = new List<Color32>(myMeshFilter.mesh.vertexCount);
			for (int i = 0; i < myMeshFilter.mesh.vertexCount; i++)
				sVertexColors.Add(new Color32());
		}

		// apply colors
		sVertexColors[_specificVertex] = _color;
		myMeshFilter.mesh.SetColors(sVertexColors);
	}
}
