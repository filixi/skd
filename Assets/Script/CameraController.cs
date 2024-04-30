using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    Camera camera_c;
    void Start()
    {
        camera_c = GetComponent<Camera>();
    }

    public Bounds GetCameraBounds()
    {
        if (!camera_c)
            camera_c = GetComponent<Camera>();

        var upperRightScreen = new Vector3(Screen.width, Screen.height, 0);
        var lowerLeftScreen = new Vector3(0, 0, 0);

        var lower = camera_c.ScreenToWorldPoint(lowerLeftScreen);
        var upper = camera_c.ScreenToWorldPoint(upperRightScreen);

        return new Bounds
        {
            min = lower,
            max = upper
        };
    }

    public Vector3 GetMouseLocation()
    {
        return camera_c.ScreenToWorldPoint(Input.mousePosition);
    }

    public Vector2Int GetMouseVertexLocation()
    {
        var location = GetMouseLocation();
        return new Vector2Int(Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.z));
    }

    // Update is called once per frame
    void Update()
    {
        return;
        //Vector3 delta = new Vector3(0, 0, 0);
        //if (Input.GetKey("w"))
        //    delta += new Vector3(0, 0, 1);
        //if (Input.GetKey("s"))
        //    delta += new Vector3(0, 0, -1);
        //if (Input.GetKey("a"))
        //    delta += new Vector3(-1, 0, 0);
        //if (Input.GetKey("d"))
        //    delta += new Vector3(1, 0, 0);

        //camera_c.orthographicSize -= Input.mouseScrollDelta.y;
        //camera_c.orthographicSize = UnityEngine.Mathf.Clamp(camera_c.orthographicSize, 3, 20);

        //delta.Normalize();
        //transform.position += delta * 3 * camera_c.orthographicSize * Time.deltaTime;
    }

    Vector2? CalculateIntersection(Vector3 p31, Vector3 p32, Vector3 p33, Vector3 p34)
    {
        Func<Vector3, Vector2> cast = (a) => new Vector2(a.x, a.z);
        var p1 = cast(p31);
        var p2 = cast(p32);
        var p3 = cast(p33);
        var p4 = cast(p34);

        float denominator = ((p2.x - p1.x) * (p4.y - p3.y)) - ((p2.y - p1.y) * (p4.x - p3.x));

        if (denominator == 0)
            return null;

        float numerator1 = ((p1.y - p3.y) * (p4.x - p3.x)) - ((p1.x - p3.x) * (p4.y - p3.y));
        float numerator2 = ((p1.y - p3.y) * (p2.x - p1.x)) - ((p1.x - p3.x) * (p2.y - p1.y));

        float r = numerator1 / denominator;
        float s = numerator2 / denominator;

        if (r >= 0 && r <= 1 && s >= 0 && s <= 1)
        {
            float x = p1.x + (r * (p2.x - p1.x));
            float y = p1.y + (r * (p2.y - p1.y));
            return new Vector2(x, y);
        }
        return null;
    }

    public Tuple<Vector3, Vector3> ComputeLineScreenIntersection(Vector3 e1, Vector3 e2)
    {
        Vector3 a = camera_c.ViewportToWorldPoint(new Vector3(0, 0, camera_c.nearClipPlane));
        Vector3 b = camera_c.ViewportToWorldPoint(new Vector3(0, 1, camera_c.nearClipPlane));
        Vector3 c = camera_c.ViewportToWorldPoint(new Vector3(1, 1, camera_c.nearClipPlane));
        Vector3 d = camera_c.ViewportToWorldPoint(new Vector3(1, 0, camera_c.nearClipPlane));

        Vector3 direction = e2 - e1;
        Vector3 p = e1;
        e1 = p + direction * 100f;
        e2 = p - direction * 100f;

        var a1 = CalculateIntersection(e1, e2, a, b);
        var a2 = CalculateIntersection(e1, e2, b, c);
        var a3 = CalculateIntersection(e1, e2, c, d);
        var a4 = CalculateIntersection(e1, e2, d, a);

        var points = (new List<Vector2?>{ a1, a2, a3, a4})
            .Where(v => v != null)
            .Select(v => new Vector3(v.Value.x, 0, v.Value.y))
            .ToList();

        if (points.Count() < 2)
            return null;

        var d1 = points[1] - points[0];
        if (Vector3.Dot(d1, direction) > 0.95)
            return Tuple.Create(points[0], points[1]);
        return Tuple.Create(points[1], points[0]);
    }
}
