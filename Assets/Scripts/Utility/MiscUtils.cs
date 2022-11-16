
using System.IO;
using UnityEngine;

public static class MiscUtils 
{
    public static Transform FindGrandChildByName(Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                return child;
            var childTransform = FindGrandChildByName(child, name);
            if (childTransform != null)
                return childTransform;
        }

        return null;
    }

    public static Transform FindGrandChildByNamePart(Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains(name))
                return child;
            var childTransform = FindGrandChildByNamePart(child, name);
            if (childTransform != null)
                return childTransform;
        }

        return null;
    }

    public static void AddScore(int addScore)
    {
        var score = PlayerPrefs.HasKey(Constants.SCORE_KEY) ? PlayerPrefs.GetInt(Constants.SCORE_KEY) : 0;
        score += addScore;
        PlayerPrefs.SetInt(Constants.SCORE_KEY, score);
    }

    public static void WriteStringToFile(string path, string content, bool append)
    {
        using var writer = new StreamWriter(path, append);
        writer.WriteLine(content);
        writer.Close();
    }

    public static string ReadStringFromFile(string path)
    {
        using var reader = new StreamReader(path);
        var content = reader.ReadToEnd();
        reader.Close();
        return content;
    }
    
#if UNITY_EDITOR
    // Sphere with radius of 1
    private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);

    // Square with edge of length 1
    private static readonly Vector4[] s_UnitSquare =
    {
            new Vector4(-0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, -0.5f, 0, 1),
            new Vector4(-0.5f, -0.5f, 0, 1),
    };

    private static Vector4[] MakeUnitSphere(int len)
    {
        Debug.Assert(len > 2);
        var v = new Vector4[len * 3];
        for (int i = 0; i < len; i++)
        {
            var f = i / (float)len;
            float c = Mathf.Cos(f * (float)(Mathf.PI * 2.0));
            float s = Mathf.Sin(f * (float)(Mathf.PI * 2.0));
            v[0 * len + i] = new Vector4(c, s, 0, 1);
            v[1 * len + i] = new Vector4(0, c, s, 1);
            v[2 * len + i] = new Vector4(s, 0, c, 1);
        }
        return v;
    }
    public static void DrawSphere(Vector4 pos, float radius, Color color, float duration)
    {
        Vector4[] v = s_UnitSphere;
        int len = s_UnitSphere.Length / 3;
        for (int i = 0; i < len; i++)
        {
            var sX = pos + radius * v[0 * len + i];
            var eX = pos + radius * v[0 * len + (i + 1) % len];
            var sY = pos + radius * v[1 * len + i];
            var eY = pos + radius * v[1 * len + (i + 1) % len];
            var sZ = pos + radius * v[2 * len + i];
            var eZ = pos + radius * v[2 * len + (i + 1) % len];
            Debug.DrawLine(sX, eX, color, duration);
            Debug.DrawLine(sY, eY, color, duration);
            Debug.DrawLine(sZ, eZ, color, duration);
        }
    }
#endif
}
