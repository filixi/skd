using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class LaserControl : MonoBehaviour, FlipbookRenderData
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(long tick, Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;

        Collider[] colliders = Physics.OverlapSphere(end, 0.001f);
        foreach (Collider collider in colliders)
        {
            var receiver = collider.GetComponent<RegularEnemy>();
            if (receiver != null)
                receiver.TakeDamage(DamageType.Explosive, 3, Tick.tick);
        }
    }

    Vector3 start = Vector3.zero;
    Vector3 end = Vector3.zero;

    Vector3? default_scale;
    public void UpdateAnimationMatrix(ref Matrix4x4 m, long tick)
    {
        if (default_scale == null)
            default_scale = gameObject.transform.localScale;
        start.y = end.y = 0.4f;

        var center = (start + end) * 0.5f;
        var length = (end - start).magnitude;

        gameObject.transform.position = center;
        gameObject.transform.rotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(end - start);
        gameObject.transform.localScale = new Vector3(length / 10, default_scale.Value.y, default_scale.Value.z);
    }
}
