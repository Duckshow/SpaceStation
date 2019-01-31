using UnityEngine;
public static class BitCompressor {

	// compress four bytes into one int (32 bit)
	public static int Byte4ToInt(byte _b0, byte _b1, byte _b2, byte _b3){
		return _b0 | (_b1 << 8) | (_b2 << 16) | (_b3 << 24);
	}

	public static void IntToByte4(int _i, out byte _b0, out byte _b1, out byte _b2, out byte _b3) {
		_b0 = (byte)(_i & 0xFF);
		_b1 = (byte)(_i >> 8 & 0xFF);
		_b2 = (byte)(_i >> 16 & 0xFF);
		_b3 = (byte)(_i >> 24 & 0xFF);
	}

	public static int Int2ToInt(int _i0, int _i1){
		if (_i0 > 0xFFFF)
			Debug.LogError("Int2ToInt()'s first parameter is " + _i0 + ", but the limit is " + 0xFFFF + "!");
		if (_i1 > 0xFFFF)
			Debug.LogError("Int2ToInt()'s second parameter is " + _i0 + ", but the limit is " + 0xFFFF + "!");

		return _i0 | (_i1 << 16);
	}

	// compress four ints into one int (32 bit) (MAX 1023!)
	// public static int Int3ToInt(int _i0, int _i1, int _i2){
	// 	if(_i0 > 1023)
	// 		Debug.LogError("Int3ToInt()'s first parameter is " + _i0 + ", but the limit is 1023!");
	// 	if (_i1 > 1023)
	// 	Debug.LogError("Int3ToInt()'s second parameter is " + _i1 + ", but the limit is 1023!");
	// 	if (_i2 > 1023)
	// 		Debug.LogError("Int3ToInt()'s third parameter is " + _i2 + ", but the limit is 1023!");

	// 	return _i0 | (_i1 << 10) | (_i2 << 20);
	// }
}
