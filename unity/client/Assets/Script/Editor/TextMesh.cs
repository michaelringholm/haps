using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TextMesh
{
    [MenuItem("Examples/TMP")]
    static void EditorPlaying()
    {
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            var name = g.GetType().Name;
            Debug.Log(name);
        }
    }
}