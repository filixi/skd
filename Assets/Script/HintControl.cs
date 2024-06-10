using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintControl : MonoBehaviour
{
    public AnimationCurve curve;

    // Start is called before the first frame update
    GameObject hint_a;
    GameObject hint_b;

    void Start()
    {
        hint_a = GameObject.Find("HintTxtA");
        hint_b = GameObject.Find("HintTxtB");
    }

    long start_tick = -100000;
    long duration = 1000;
    public void StartAnimation()
    {
        start_tick = tick_now = 0;
    }

    long tick_now = 0;
    void Update()
    {
        float progress = (tick_now - start_tick) * 1.0f / duration;
        if (progress > 1 || progress < 0)
        {
            hint_a.SetActive(false);
            hint_b.SetActive(false);
            return;
        }

        ++tick_now;

        hint_a.SetActive(true);
        hint_b.SetActive(true);

        RectTransform objectRectTransform = GameObject.Find("InGameHUD").GetComponent<RectTransform>();
        float width = objectRectTransform.rect.width;

        var left = width;
        var right = -width;

        var p1 = Mathf.Clamp(progress * 2, 0, 1);
        var p2 = Mathf.Clamp(progress * 2 - 0.8f, 0, 1);

        var shift1 = curve.Evaluate(p1);
        var shift2 = curve.Evaluate(p2);

        var x1 = Mathf.Lerp(left, right, shift1);
        var x2 = Mathf.Lerp(right, left, shift2);

        hint_a.GetComponent<RectTransform>().anchoredPosition = new Vector3(x1-20, 0, 0);
        hint_b.GetComponent<RectTransform>().anchoredPosition = new Vector3(x2 - 300, -80, 0);
    }
}
