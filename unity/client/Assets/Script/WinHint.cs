using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Script;

public class WinHint : MonoBehaviour
{
    public char CurrentChar;

    public void UpdateHint(Sprite sprite, char winChar, int level)
    {
        foreach (var img in GetComponentsInChildren<Image>())
        {
            img.sprite = sprite;
        }

        var labels = GetComponentsInChildren<TextMesh>();
        labels[0].text = Win.GetWinText(winChar);
        labels[1].text = Win.GetRequirementText(winChar, level);
    }

    void Start()
    {
    }

    void Update()
    {
    }
}
