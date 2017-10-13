using UnityEngine;

[ExecuteInEditMode]
public class Gradient : MonoBehaviour {

    public Color[] Colors;

    private Texture2D tex;
    private Material mat;

    void Start () {
        tex = new Texture2D(1, Colors.Length, TextureFormat.ARGB32, false);
        for (int i = 0; i < Colors.Length; ++i)
            tex.SetPixel(0, Colors.Length - i - 1, Colors[i]); // Reverse Y

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = tex;

        GetComponent<MeshRenderer>().material = mat;
    }

    void Update () {
    }
}
