using UnityEngine;
using Utilities;
using System.Collections.Generic;
public static class Extensions {
    public static string Color(this string s, Color c) {
        Color32 c32 = c;
        string cHex = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2") + c32.a.ToString("X2");
        return "<color=" + cHex + ">" + s + "</color>";
    }

	public static bool Any(this Color c) {
		return c.r > 0 || c.g > 0 || c.b > 0 || c.a > 0;
	}
	public static bool Any(this Color32 c) {
		return c.r > 0 || c.g > 0 || c.b > 0 || c.a > 0;
	}

	public static T GetLast<T>(this T[] _array){
		return _array[_array.Length - 1];
	}
	public static T GetLast<T>(this T[,] _array){
		return _array[_array.GetLength(0) - 1, _array.GetLength(1) - 1];
	}
    public static void Insert<T>(this T[] _array, int _index, T _obj){
        for (int i = _array.Length - 1; i > _index; i--){
            _array[i] = _array[i - 1];
        }
        _array[_index] = _obj;
    }
	// public static int PseudoRemove<T>(this T[] _array, T _obj, T _emptyValue){ // replace value with _emptyValue (eg -1) and move following items forward a step
	// 	int _index = -1;
	// 	for (int i = 0; i < _array.Length; i++){
	// 		if (EqualityComparer<T>.Default.Equals(_array[i], _obj)) {
	// 			_index = i;
	// 		}
	// 	}

	// 	return PseudoRemoveAt(_array, _index, _emptyValue);
	// }
	// public static int PseudoRemoveAt<T>(this T[] _array, int _index, T _emptyValue){ // replace value with _emptyValue (eg -1) and move following items forward a step
	// 	int _newIndex = -1;
	// 	_array[_index] = _emptyValue;
	// 	for (int i = _index; i < _array.Length - 1; i++){
	// 		T _nextIndexContent = _array[i + 1];
	// 		if (EqualityComparer<T>.Default.Equals(_nextIndexContent, _emptyValue)) {
	// 			_newIndex = i;
	// 			break;
	// 		}

	// 		T _thisIndexContent = _array[i];
	// 		_array[i] = _nextIndexContent;
	// 		_array[i + 1] = _thisIndexContent;
	// 	}
	// 	return _newIndex;
	// }

	public static float GetXOrY(this Vector3 _vector, XYEnum _axis){
		return _axis == XYEnum.X ? _vector.x : _vector.y;
	}
	public static int GetXOrY(this Vector2i _vector, XYEnum _axis){
		return _axis == XYEnum.X ? _vector.x : _vector.y;
	}
}

public static class MathfExtensions { 
    public static int Digits(int val) {
        return Mathf.Max(Mathf.FloorToInt(Mathf.Log10(val) + 1), 1);
    }
    public static int Digits(ulong val) {
        return Mathf.Max(Mathf.FloorToInt(Mathf.Log10(val) + 1), 1);
    }
    public static int ConcatToInt(int val1, int val2) {
        return int.Parse(val1.ToString() + val2.ToString());
    }
    public static ulong ConcatToUlong(int val1, int val2) {
        return ulong.Parse(val1.ToString() + val2.ToString());
    }
    public static ulong ConcatToUlong(ulong val1, ulong val2) { 
        return ulong.Parse(val1.ToString() + val2.ToString());
    }
}
