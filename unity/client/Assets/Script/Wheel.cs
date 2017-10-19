using System.Collections.Generic;
using UnityEngine;

// Masking: Clamp icon rect to screen bounds. No extra component needed.
// Icons. Mid, below *, above * y.
// Create rnd range. Start pos at end of range.
// Move pos to start of range.
// SetPos: Map range in view to icons and set translate Y for partial.

public class Wheel : MonoBehaviour
{
    float SpinSpeed = 12.0f;

    public GameObject iconPrefab;
    public Sprite[] iconSprites;

    private int iconCount;
    private int errorIconIdx;

    private List<SpriteRenderer> row0Renderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> row1Renderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> row2Renderers = new List<SpriteRenderer>();
    private List<int> row0Icons = new List<int>();
    private List<int> row1Icons = new List<int>();
    private List<int> row2Icons = new List<int>();

    // Position: Position in list. Clamps at length - 3.
    float position0 = 0.0f;
    float position1 = 0.0f;
    float position2 = 0.0f;

    float targetPosition0 = 0.0f;
    float targetPosition1 = 0.0f;
    float targetPosition2 = 0.0f;

    float offsetX = 0.30f;
    float offsetY = 0.33f;
    float scale = 0.26f;

    [System.NonSerialized]
    public bool IsSpinning = false;

    public List<SpriteRenderer> GetRenderers(int[] indices)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = 0; i < indices.Length; ++i)
        {
            int idx = indices[i];
            int row = idx % 3;
            if (row == 0)
                result.Add(row0Renderers[idx / 3]);
            else if (row == 1)
                result.Add(row1Renderers[idx / 3]);
            else if (row == 2)
                result.Add(row2Renderers[idx / 3]);
        }
        return result;
    }

    public List<int> GetFinal9()
    {
        // Round instead of clamp to be 100% to hit the correct icon
        int top0 = Mathf.RoundToInt(position0);
        int top1 = Mathf.RoundToInt(position1);
        int top2 = Mathf.RoundToInt(position2);

        return new List<int> {
            row0Icons[top0 + 0], row1Icons[top1 + 0], row2Icons[top2 + 0],
            row0Icons[top0 + 1], row1Icons[top1 + 1], row2Icons[top2 + 1],
            row0Icons[top0 + 2], row1Icons[top1 + 2], row2Icons[top2 + 2],
        };
    }

    public Sprite GetSpriteForIcon(int idx)
    {
        Sprite sprite = iconSprites[idx];
        return sprite;
    }

    void Start()
    {
        iconCount = iconSprites.Length - 1; // Last one is used for out of bounds
        errorIconIdx = iconSprites.Length - 1;

        // Setup x-positions and initial icons
        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 3; ++x)
            {
                int xx = x - 1;
                int yy = y - 2;
                var pos = new Vector3(xx * offsetX, yy * offsetY, -0.5f);
                var icon = (GameObject)Instantiate(iconPrefab);
                icon.transform.SetParent(this.gameObject.transform);
                icon.transform.localScale = new Vector2(scale, scale);
                icon.transform.localPosition = pos;
                var renderer = icon.GetComponent<SpriteRenderer>();
                int iconIdx = (x + y * 3) % iconCount;
                if (x == 0)
                {
                    row0Renderers.Add(renderer);
                    row0Icons.Add(iconIdx);
                }
                else if (x == 1)
                {
                    row1Renderers.Add(renderer);
                    row1Icons.Add(iconIdx);
                }
                else if (x == 2)
                {
                    row2Renderers.Add(renderer);
                    row2Icons.Add(iconIdx);
                }
            }
        }

        // Set screenspace crop on shared material
        var cropTopObj = GameObject.Find("CropPosTop");
        var cropBottomObj = GameObject.Find("CropPosBottom");
        var cropTop = Camera.main.WorldToViewportPoint(cropTopObj.transform.position);
        var cropBottom = Camera.main.WorldToViewportPoint(cropBottomObj.transform.position);

        var anyRenderer = row0Renderers[0];
        anyRenderer.sharedMaterial.SetFloat("_MaxY", cropTop.y);
        anyRenderer.sharedMaterial.SetFloat("_MinY", cropBottom.y);

        UpdateRows();
    }

    private void AddNewIcons(List<int> oldRow, List<int> newRow, ref float position)
    {
        // Save the 3 currently visible
        int truncPos = (int)position;
        int v0, v1, v2;
        v0 = oldRow[truncPos + 0];
        v1 = oldRow[truncPos + 1];
        v2 = oldRow[truncPos + 2];

        // Assign the new list
        oldRow.Clear();
        oldRow.AddRange(newRow);

        // Append the 3 currently visible
        oldRow.Add(v0);
        oldRow.Add(v1);
        oldRow.Add(v2);

        // Finally adjust position to still show the same 3
        // Previously this was at position 0, but now it is the last 3.
        position = oldRow.Count - 3;
    }

    void SetSpinTarget(bool fullSpin, bool holdFlag, float currentPosition, ref float targetPosition)
    {
        if (holdFlag)
        {
            // Hold, do not move
            targetPosition = currentPosition;
            return;
        }
        else if (fullSpin)
        {
            // Spin to top of list (0)
            targetPosition = 0;
            return;
        }
        else
        {
            // 1-down
            targetPosition = currentPosition - 1;
        }
    }

    void InsertForOneDown(List<int> row, bool holdFlag, ref float position)
    {
        if (holdFlag)
            return;

        // Insert duplicate of bottom visible row at top
        const int BottomVisibleIdx = 2;
        row.Insert(0, row[BottomVisibleIdx]);
        position += 1;
    }

    public void DoOneDown(bool[] holdFlags)
    {
        InsertForOneDown(row0Icons, holdFlags[0], ref position0);
        InsertForOneDown(row1Icons, holdFlags[1], ref position1);
        InsertForOneDown(row2Icons, holdFlags[2], ref position2);

        const bool FullSpin = false;
        SetSpinTarget(FullSpin, holdFlags[0], position0, ref targetPosition0);
        SetSpinTarget(FullSpin, holdFlags[1], position1, ref targetPosition1);
        SetSpinTarget(FullSpin, holdFlags[2], position2, ref targetPosition2);

        IsSpinning = true;
    }

    public void DoFullSpin(bool[] holdFlags)
    {
        const bool FullSpin = true;
        SetSpinTarget(FullSpin, holdFlags[0], position0, ref targetPosition0);
        SetSpinTarget(FullSpin, holdFlags[1], position1, ref targetPosition1);
        SetSpinTarget(FullSpin, holdFlags[2], position2, ref targetPosition2);

        IsSpinning = true;
    }

    public void SetNextSpin(List<int> newRow0, List<int> newRow1, List<int> newRow2)
    {
        AddNewIcons(row0Icons, newRow0, ref position0);
        AddNewIcons(row1Icons, newRow1, ref position1);
        AddNewIcons(row2Icons, newRow2, ref position2);
    }

    void UpdateRow(float position, List<SpriteRenderer> renderers, List<int> icons)
    {
        // Show 4 icons from this clamped position
        int iconIdx = (int)position;
        float frac = position - iconIdx;

        float yOffset = offsetY;

        // Top to bottom of row
        foreach(var renderer in renderers)
        {
            int spriteIdx;
            bool isOutOfBounds = iconIdx < 0 || iconIdx >= icons.Count;
            if (isOutOfBounds)
            {
                spriteIdx = errorIconIdx;
            }
            else
            {
                spriteIdx = icons[iconIdx];
            }
            iconIdx++;

            Sprite sprite = iconSprites[spriteIdx];
            if (renderer.sprite != sprite)
                renderer.sprite = sprite;

            var pos = renderer.transform.localPosition;
            pos.y = yOffset + frac * offsetY;
            renderer.transform.localPosition = pos;
            yOffset -= offsetY;
        }
    }

    void UpdateRows()
    {
        UpdateRow(position0, row0Renderers, row0Icons);
        UpdateRow(position1, row1Renderers, row1Icons);
        UpdateRow(position2, row2Renderers, row2Icons);
    }

    bool MoveToTarget(float target, ref float position)
    {
        position -= Time.deltaTime * SpinSpeed;
        if (position < target)
        {
            position = target;
            return false;
        }
        return true;
    }

    void Update()
    {
        if (IsSpinning)
        {
            bool didMove = false;
            didMove |= MoveToTarget(targetPosition0, ref position0);
            didMove |= MoveToTarget(targetPosition1, ref position1);
            didMove |= MoveToTarget(targetPosition2, ref position2);

            UpdateRows();

            if (!didMove)
            {
                IsSpinning = false;
            }
        }
    }
}
