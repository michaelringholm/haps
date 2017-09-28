using UnityEngine;
using System.Collections;

public class WinLine : MonoBehaviour
{
    private int[] Line = new int[] { 3, 4, 5 }; // Default: middle line

    private LineRenderer line_;
    private Transform fruitGrid_;
    private Material lineMat_;

    private Vector3 target0_;
    private Vector3 target1_;
    private Vector3 current0_;
    private Vector3 current1_;

    private float z_;
    private float speed_ = 1.0f;

    public Color ColorFlash = Color.green;
    public Color Color = Color.white;
    public float Width = 0.1f;
    public float WidthFlash = 0.4f;
    private float flashT_ = 0.0f;

    public float LineSizeHorizontal = 1.65f;
    public float LineSizeDiagonal = 1.6f;

    void Start()
    {
        z_ = transform.position.z;
        float y = transform.position.y;
        current0_ = new Vector3(0, y, z_);
        current1_ = new Vector3(0, y, z_);
        line_ = GetComponent<LineRenderer>();
        lineMat_ = line_.material;
        fruitGrid_ = GameObject.Find("FruitGrid").transform;
        UpdateTarget(Line, 100);
    }

    public void SetDefault()
    {
        Line = new int[] { 3, 4, 5 }; // Default: middle line
        UpdateTarget(Line, 100);
    }

    public void UpdateTarget(int[] cells, float speed)
    {
        speed_ = speed;

        float w = fruitGrid_.transform.localScale.x / 3;
        float h = fruitGrid_.transform.localScale.y / 3;
        float y = fruitGrid_.transform.position.y;

        if (cells[2] != cells[0] + 2) // ex: 012, 345, 678
        {
            // Diagonal
            if (cells[0] == 0)
            {
                // \
                target0_ = new Vector3(-w * LineSizeDiagonal, (((cells[2] / 3) - 1) * h * LineSizeDiagonal) + y, z_);
                target1_ = new Vector3(w * LineSizeDiagonal, (((cells[0] / 3) - 1) * h * LineSizeDiagonal) + y, z_);
            }
            else
            {
                // /
                target0_ = new Vector3(-w * LineSizeDiagonal, (((cells[2] / 3) - 1) * h * LineSizeDiagonal) + y, z_);
                target1_ = new Vector3(w * LineSizeDiagonal, (((cells[0] / 3) - 1) * h * LineSizeDiagonal) + y, z_);
            }
        }
        else
        {
            // Horizontal
            target0_ = new Vector3(-w * LineSizeHorizontal, (((cells[0] / 3) - 1) * -h) + y, z_);
            target1_ = new Vector3(w * LineSizeHorizontal, (((cells[2] / 3) - 1) * -h) + y, z_);
        }

        this.Line = cells;
        flashT_ = 1.0f;
    }

    void Update()
    {
        // Exploiting the fact that the flash is always complete AFTER the endpoint movement.
        if (flashT_ <= 0.0f)
            return;

        float step = Time.deltaTime * speed_;
        current0_ = Vector3.MoveTowards(current0_, target0_, step);
        current1_ = Vector3.MoveTowards(current1_, target1_, step);
        line_.SetPosition(0, current0_);
        line_.SetPosition(1, current1_);
        float width = Mathf.Lerp(Width, WidthFlash, (flashT_ - 0.75f) * 4);
        line_.SetWidth(width, width);

        lineMat_.color = Color.Lerp(Color, ColorFlash, flashT_);
        float colorSpeed = 0.02f * speed_;
        flashT_ -= Time.deltaTime * colorSpeed;
    }
}
