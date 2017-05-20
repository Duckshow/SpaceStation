/****************************************************************************
 Copyright (c) 2014 Martin Ysa

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 ****************************************************************************/

using UnityEngine;

public class SinCosTable{

	static bool hasInstanced = false;

	// -- almaceno seno y cose pos cuestiones de performance --// (translation: Stored sin and cos positions due to performance issues (?))
	public static float[] sSinArray;
	public static float[] sCosArray;


	public static void InitSinCos(){
		if(hasInstanced == false){
			sSinArray = new float[360];
			sCosArray = new float[360];
			
			for(int i = 0; i < 360; i++){
				sSinArray[i] = Mathf.Sin(i * Mathf.Deg2Rad);
				sCosArray[i] = Mathf.Cos(i * Mathf.Deg2Rad);
			}

			hasInstanced = true;
		}
	}
}
