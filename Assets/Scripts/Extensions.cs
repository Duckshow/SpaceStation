using UnityEngine;
public static class Extensions {
    public static string Color(this string s, Color c) {
        Color32 c32 = c;
        string cHex = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2") + c32.a.ToString("X2");
        return "<color=" + cHex + ">" + s + "</color>";
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
