using UnityEngine;
using System.Collections;

public class TouchTrail : MonoBehaviour
{
    private Vector2 startSwipe_;
    private Vector2 endSwipe_;
    private TrailRenderer trail_;
    private Transform fruitGrid_;
    private bool isSwiping_;
    private Main main_;

    void Start()
    {
        main_ = GameObject.Find("Main").GetComponent<Main>();
        fruitGrid_ = GameObject.Find("FruitGrid").transform;

        trail_ = GetComponent<TrailRenderer>();
        trail_.sortingLayerName = "Background";
        trail_.sortingOrder = 2;
    }
	
    Vector2 GetMouseWorldPos()
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return worldPoint;
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    void TrySetWinLine(Vector2 a, Vector2 b)
    {
        float h = fruitGrid_.transform.localScale.y / 3;
        if (a.x > b.x)
        {
            // Swap a and b so a swipe is always from left to right
            Vector2 tmp = b;
            b = a;
            a = tmp;
        }

        int y0 = Mathf.RoundToInt(a.y / h) + 1;
        int y1 = Mathf.RoundToInt(b.y / h) + 1;
        if (y0 == y1)
        {
            // Horizontal
            switch (y0)
            {
                case 0: main_.SetWinLine(new int[] { 6, 7, 8 }); break;
                case 1: main_.SetWinLine(new int[] { 3, 4, 5 }); break;
                case 2: main_.SetWinLine(new int[] { 0, 1, 2 }); break;
            }
        }
        else if (y1 < y0)
        {
            // Diagonal, left -> right
            main_.SetWinLine(new int[] { 0, 4, 8 });
        }
        else
        {
            // Diagonal, right -> left
            main_.SetWinLine(new int[] { 6, 4, 2 });
       }
    }

    void Update()
    {
        if (Main.State == Main.GlobalState.Other || main_.GameState != Main.GameStateEnum.AwaitingUserDecisions)
            return;

        if (Input.GetMouseButton(0) && isSwiping_)
        {
            this.transform.position = GetMouseWorldPos();
        }

        if (Input.GetMouseButtonDown(0))
        {
            trail_.time = 0.5f;
            trail_.Clear();

            var pos = GetMouseWorldPos();
            var p = fruitGrid_.position;
            var s = fruitGrid_.transform.localScale * 0.5f;
            bool withinFruitGrid = pos.y > p.y - s.y && pos.y < p.y + s.y;
            if (withinFruitGrid)
            {
                transform.position = pos;
                startSwipe_ = transform.position - fruitGrid_.transform.position;
                trail_.Clear();
                isSwiping_ = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isSwiping_)
        {
            endSwipe_ = transform.position - fruitGrid_.transform.position;
            trail_.time = 0.2f;
            TrySetWinLine(startSwipe_, endSwipe_);
            isSwiping_ = false;
        }
    }
}
