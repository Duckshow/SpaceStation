using UnityEngine;
public static class BitCompressor {

	public static int FourBytesToInt32(byte _b0, byte _b1, byte _b2, byte _b3) {
		return _b0 |(_b1 << 8) |(_b2 << 16) |(_b3 << 24);
	}

	//* Can't use more than three bytes in Float due to floating point precision
	public static float ThreeBytesToFloat32(byte _b0, byte _b1, byte _b2) {
		return _b0 |(_b1 << 8) |(_b2 << 16);
	}

	public static void Int32ToFourBytes(int _i, out byte _b0, out byte _b1, out byte _b2, out byte _b3) {
		_b0 =(byte)(_i & 0xFF);
		_b1 =(byte)(_i >> 8 & 0xFF);
		_b2 =(byte)(_i >> 16 & 0xFF);
		_b3 =(byte)(_i >> 24 & 0xFF);
	}

	public static int TwoIntsToInt32(int _i0, int _i1) {
		if(_i0 > 0xFFFF)
			Debug.LogError("Int2ToInt()'s first parameter is " + _i0 + ", but the limit is " + 0xFFFF + "!");
		if(_i1 > 0xFFFF)
			Debug.LogError("Int2ToInt()'s second parameter is " + _i0 + ", but the limit is " + 0xFFFF + "!");

		return _i0 |(_i1 << 16);
	}

	public static int ThreeIntsToInt32(int _i0, int _i1, int _i2) {
		if(_i0 > 1023)
			Debug.LogError("Int3ToInt()'s first parameter is " + _i0 + ", but the limit is 1023!");
		if(_i1 > 1023)
			Debug.LogError("Int3ToInt()'s second parameter is " + _i1 + ", but the limit is 1023!");
		if(_i2 > 1023)
			Debug.LogError("Int3ToInt()'s third parameter is " + _i2 + ", but the limit is 1023!");

		return _i0 |(_i1 << 10) |(_i2 << 20);
	}

	public static float TwoBytesToFloat16(int _b0, int _b1) {
		return _b0 |(_b1 << 8);
	}
	
	public static float FourHalfBytesToFloat16(int _b0, int _b1, int _b2, int _b3) {
		return _b0 |(_b1 << 4) | (_b2 << 8) | (_b3 << 12);
	}
}