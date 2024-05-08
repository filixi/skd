using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionControl : MonoBehaviour, FlipbookRenderData
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    long spawn_tick = -100000;
    // Update is called once per frame
    public void Initialize(long tick)
    {
        spawn_tick = tick;
        collidedObjects.Clear();
    }

    private float ComputeTotalProgress(long tick)
    {
        return (tick - spawn_tick) / (60.0f * 0.6f);
    }
    private float ComputeExpansionProgress(long tick)
    {
        var total_progress = ComputeTotalProgress(tick);
        var expand_progress = FlipbookRender.SmoothStep(0.0f, 0.3f, total_progress);
        return expand_progress;
    }

    private float ComputeFadeOutProgress(long tick)
    {
        var total_progress = ComputeTotalProgress(tick);
        var fade_progress = FlipbookRender.SmoothStep(0.2f, 1.0f, total_progress);
        return fade_progress;
    }

    public bool IsFinished(long tick)
    {
        return ComputeTotalProgress(tick) >= 1;
    }

    public bool IsDamageDealingFinished(long tick)
    {
        return ComputeExpansionProgress(tick) >= 1;
    }

    public void UpdateAnimationMatrix(ref Matrix4x4 m, long tick, float scale)
    {
        var expand_progress = ComputeExpansionProgress(tick);
        if (expand_progress < 1)
            gameObject.transform.localScale = Vector3.one * scale * expand_progress * 0.8f;

        var fade_progress = ComputeFadeOutProgress(tick);
        m[2, 0] = Mathf.Clamp((fade_progress - 0.5f) * 2, 0, 1);
    }

    HashSet<GameObject> collidedObjects = new HashSet<GameObject>();
    void Update()
    {
        if (IsDamageDealingFinished(Tick.tick))
            return;

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, gameObject.transform.localScale.x * 5);
        foreach (Collider collider in colliders)
        {
            if (collidedObjects.Contains(collider.gameObject))
                continue;
            collidedObjects.Add(collider.gameObject);

            var receiver = collider.GetComponent<RegularEnemy>();
            if (receiver != null)
                receiver.TakeDamage(DamageType.Explosive, 100, Tick.tick);
        }
    }
}
