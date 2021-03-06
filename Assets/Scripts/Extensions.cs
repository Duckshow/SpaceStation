﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public interface ICloneable<T> {
	T Clone();
}

public static class Extensions {
	
	public static T[,] CloneArrayAndContents<T>(this T[,] _array) where T : ICloneable<T> {
		int _lengthX = _array.GetLength(0);
		int _lengthY = _array.GetLength(1);
		T[,] _clone = new T[_lengthX, _lengthY];

		for (int x = 0; x < _lengthX; x++) {
			for (int y = 0; y < _lengthY; y++) {
				_clone[x, y] = _array[x, y].Clone();
			}
		}

		return _clone;
	}

	public static bool IsGreaterOrEqual(this float _f, params float[] _fs) {
		for (int i = 0; i < _fs.Length; i++) {
			if (_fs[i] > _f) {
				return false;
			}
		}

		return true;
	}

	public static string Color(this string s, Color c) {
		Color32 c32 = c;
		string cHex = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2") + c32.a.ToString("X2");
		return "<color=" + cHex + ">" + s + "</color>";
	}

	public static bool Equals(this Color32 c0, Color c1) {
		return c0.r == c1.r && c0.g == c1.g && c0.b == c1.b && c0.a == c1.a;
	}

	// public static Direction GetDirection(Int2 _from, Int2 _to) {
	// 	Int2 _diff = _to - _from;

	// 	if (_diff.x > 0) { 
	// 		if (_diff.y > 0){
	// 			return Direction.TR;
	// 		}
	// 		else if (_diff.y == 0){
	// 			return Direction.R;
	// 		}
	// 		else if (_diff.y < 0){
	// 			return Direction.BR;
	// 		}
	// 	}
	// 	else if (_diff.x == 0) { 
	// 		if (_diff.y > 0){
	// 			return Direction.T;
	// 		}
	// 		else if (_diff.y == 0){
	// 			return Direction.None;
	// 		}
	// 		else if (_diff.y < 0){
	// 			return Direction.B;
	// 		}
	// 	}
	// 	else if (_diff.x < 0) { 
	// 		if (_diff.y > 0){
	// 			return Direction.TL;
	// 		}
	// 		else if (_diff.y == 0){
	// 			return Direction.L;
	// 		}
	// 		else if (_diff.y < 0){
	// 			return Direction.BL;
	// 		}
	// 	}

	// 	return Direction.None;
	// }
}

public enum Sorting { None, Back, Front }
public enum Rotation { None, Up, Right, Down, Left }
public enum Direction { None, All, TL, T, TR, R, BR, B, BL, L }

[System.Serializable] public struct Float2 {
	public float x;
	public float y;

	public Float2(float x, float y) {
		this.x = x;
		this.y = y;
	}

	public override string ToString() {
		return string.Format("({0}, {1})", x.ToString(), y.ToString());
	}

	public static Float2 operator +(Float2 value0, Float2 value1) {
		return new Float2(value0.x + value1.x, value0.y + value1.y);
	}

	public static Float2 operator -(Float2 value0, Float2 value1) {
		return new Float2(value0.x - value1.x, value0.y - value1.y);
	}

	public static Float2 operator *(Float2 value0, int m) {
		return new Float2(value0.x * m, value0.y * m);
	}
}

[CustomPropertyDrawer(typeof(Float2))]
public class Float2Drawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		EditorGUI.BeginProperty(position, label, property);

		float width = position.width * 0.5f;
		Rect rectX = new Rect(position.x, position.y, width, position.height);
		Rect rectY = new Rect(width + position.x, position.y, width, position.height);
		SerializedProperty propX = property.FindPropertyRelative("x");
		SerializedProperty propY = property.FindPropertyRelative("y");
		propX.floatValue = EditorGUI.FloatField(rectX, "", propX.floatValue);
		propY.floatValue = EditorGUI.FloatField(rectY, "", propY.floatValue);

		EditorGUI.EndProperty();
	}
}

[System.Serializable] public struct Int2 {
	public int x;
	public int y;

	public Int2(int x, int y) {
		this.x = x;
		this.y = y;
	}
	public Int2(float x, float y) {
		this.x =(int)x;
		this.y =(int)y;
	}

	public void Set(int _x, int _y) {
		this.x = _x;
		this.y = _y;
	}

	public override string ToString() {
		return string.Format("({0}, {1})", x.ToString(), y.ToString());
	}

	public static bool operator ==(Int2 i0, Int2 i1) {
		if(System.Object.ReferenceEquals(i0, i1)) {
			return true;
		}
		if(System.Object.ReferenceEquals(null, i0)) {
			return false;
		}

		return(i0.Equals(i1));
	}

	public static bool operator !=(Int2 i0, Int2 i1) {
		return !(i0 == i1);
	}

	public override bool Equals(object value) {
		Int2 otherInt2 =(Int2)value;

		return !System.Object.ReferenceEquals(null, otherInt2) &&
			String.Equals(x, otherInt2.x) &&
			String.Equals(y, otherInt2.y);
	}

	public override int GetHashCode() {
		unchecked {
			// Choose large primes to avoid hashing collisions
			const int HashingBase =(int)2166136261;
			const int HashingMultiplier = 16777619;

			int hash = HashingBase;
			hash =(hash * HashingMultiplier) ^(!System.Object.ReferenceEquals(null, x) ? x.GetHashCode() : 0);
			hash =(hash * HashingMultiplier) ^(!System.Object.ReferenceEquals(null, y) ? y.GetHashCode() : 0);
			return hash;
		}
	}

	public static Int2 operator +(Int2 value0, Int2 value1) {
		return new Int2(value0.x + value1.x, value0.y + value1.y);
	}

	public static Int2 operator -(Int2 value0, Int2 value1) {
		return new Int2(value0.x - value1.x, value0.y - value1.y);
	}

	public static Int2 operator *(Int2 value0, int m) {
		return new Int2(value0.x * m, value0.y * m);
	}

	public static Int2 operator /(Int2 value0, int d) {
		return new Int2(value0.x / d, value0.y / d);
	}

	public static Int2 Zero { get { return new Int2(0, 0); } }
	public static Int2 One { get { return new Int2(1, 1); } }
	public static Int2 MinusOne { get { return new Int2(-1, -1); } }

	public static Int2 Up { get { return new Int2(0, 1); } }
	public static Int2 Down { get { return new Int2(0, -1); } }
	public static Int2 Left { get { return new Int2(-1, 0); } }
	public static Int2 Right { get { return new Int2(1, 0); } }

	public static Int2 UpLeft { get { return new Int2(-1, 1); } }
	public static Int2 UpRight { get { return new Int2(1, 1); } }
	public static Int2 DownLeft { get { return new Int2(-1, -1); } }
	public static Int2 DownRight { get { return new Int2(1, -1); } }

	public static Int2 GetDirection(Direction _dir) {
		switch(_dir) {
			case Direction.None:
				return Int2.Zero;
			case Direction.All:
				return Int2.One;
			case Direction.TL:
				return Int2.UpLeft;
			case Direction.T:
				return Int2.Up;
			case Direction.TR:
				return Int2.UpRight;
			case Direction.R:
				return Int2.Right;
			case Direction.BR:
				return Int2.DownRight;
			case Direction.B:
				return Int2.Down;
			case Direction.BL:
				return Int2.DownLeft;
			case Direction.L:
				return Int2.Left;
			default:
				throw new System.NotImplementedException(_dir + " hasn't been properly implemented yet!");
		}
	}
}

[CustomPropertyDrawer(typeof(Int2))]
public class Int2Drawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		EditorGUI.BeginProperty(position, label, property);

		float width = position.width * 0.5f;
		Rect rectX = new Rect(position.x, position.y, width, position.height);
		Rect rectY = new Rect(width + position.x, position.y, width, position.height);
		SerializedProperty propX = property.FindPropertyRelative("x");
		SerializedProperty propY = property.FindPropertyRelative("y");
		propX.intValue = EditorGUI.IntField(rectX, "", propX.intValue);
		propY.intValue = EditorGUI.IntField(rectY, "", propY.intValue);

		EditorGUI.EndProperty();
	}
}