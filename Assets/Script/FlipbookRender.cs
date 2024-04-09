using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public interface FlipbookRenderData
{
    void UpdateAnimationMatrix(ref Matrix4x4 m, long tick);
}

public class FlipbookRender : MonoBehaviour
{
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    Vector4 render_data;
    public void Initialize(Vector4 render_data, Vector3 location)
    {
        this.render_data = render_data;
        gameObject.GetComponent<Renderer>().material.SetVector("_Data", render_data);

        gameObject.transform.localScale = new Vector3(render_data[0] / 10, 1, render_data[1] / 10);
        gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
        gameObject.transform.position = location;

        Matrix4x4 animation_data = new Matrix4x4();
        foreach (var c in GetComponents<FlipbookRenderData>())
            c.UpdateAnimationMatrix(ref animation_data, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Refresh(long tick)
    {
        Matrix4x4 animation_data = new Matrix4x4();
        foreach (var c in GetComponents<FlipbookRenderData>())
            c.UpdateAnimationMatrix(ref animation_data, tick);

        gameObject.GetComponent<Renderer>().material.SetMatrix("_AnimationData_0", animation_data);
    }
}
