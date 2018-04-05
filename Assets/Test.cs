using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

public class Test : MonoBehaviour{


    void Start(){
    }

	void Update(){
	}

	[EasyButtons.Button]
	public void TestBitshift(){
		int x = 255;
		int y = 255;
		int z = 255;
		int w = 255;

		int bits = x | (y << 8) | (z << 16) | (w << 24);
		float deBittedX = (bits & 0xFF);
		float deBittedY = (bits >> 8 & 0xFF);
		float deBittedZ = (bits >> 16 & 0xFF);
		float debittedW = (bits >> 24 & 0xFF);

		Debug.Log((GetBinaryString(x) + " +\n" + GetBinaryString(y) + " +\n" + GetBinaryString(z) + " +\n" + GetBinaryString(w)).ToString().Color(Color.cyan));
		Debug.Log((GetBinaryString(bits) + " (" + GetBinaryString(bits).ToString().Length + ")").ToString().Color(Color.cyan));
		Debug.Log((deBittedX + ", " + deBittedY + ", " + deBittedZ + ", " + debittedW).ToString().Color(Color.cyan));
	}
	string GetBinaryString(int n) {
		char[] b = new char[32];
		int pos = 31;
		int i = 0;

		while (i < 32){
			if ((n & (1 << i)) != 0){
				b[pos] = '1';
			}
			else{
				b[pos] = '0';
			}
			pos--;
			i++;
		}
		return new string(b);
	}

    public static void ClearLogConsole(){
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }
}
