using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

// Free component (maybe pro version too):
// Schemes, active scheme.
// btn*.Image.Color = ButtonColor
// btn*.Button.HighlightedColor = ButtonHighlightedColor
// Smart suggestions while typing.
// List all Color properties in scene?

[System.Serializable]
public class ColorScheme
{
    public Color ButtonColor = Color.white;
    public Color TextColor = Color.black;
}

//[ExecuteInEditMode]
public class ColorSchemeManager : MonoBehaviour {

    public ColorScheme[] Schemes = new ColorScheme[1];
    public int ActiveScheme = 0;

    private ColorScheme current_;

    void OnValidate()
    {
        current_ = Schemes[ActiveScheme];
        UpdateColors();
    }

    // Use this for initialization
    void Start () {
        UpdateColors();
	}

    IEnumerable<GameObject> GetGameObjectStartingWith(string pre)
    {
        return GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.StartsWith(pre));
    }

    void UpdateMatches(string pre, string compType, string propName, Color color)
    {
        foreach (var go in GetGameObjectStartingWith(pre))
        {
            var comp = go.GetComponent(compType);
            if (comp == null)
            {
                Debug.Log("Component not found: " + compType);
                continue;
            }

            var t = comp.GetType();
            var prop = t.GetProperty(propName);
            if (prop == null)
            {
                var field = t.GetField(propName);
                if (field == null)
                {
                    Debug.Log("Member not found: " + propName);
                    continue;
                }
                field.SetValue(comp, color);
                continue;
            }

            prop.SetValue(comp, color, null);
        }
    }

    void UpdateColors()
    {
        //UpdateMatches("btn", "Image", "color", current_.ButtonColor);
        //UpdateMatches("label1", "TextMeshProUGUI", "color", current_.TextColor);
    }
}
