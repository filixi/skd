using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BladeControl : MonoBehaviour, FlipbookRenderData
{
    // Start is called before the first frame update
    void Start()
    {

    }

    long spawn_tick = -100000;
    Vector3 start;
    Vector3 end;

    float speed = 1;
    // Update is called once per frame
    public void Initialize(long tick, Vector3 start, Vector3 target, float speed)
    {
        spawn_tick = tick;
        this.start = start;
        this.end = target;
        this.speed = speed;

        gameObject.transform.position = start;
        collidedObjects.Clear();
    }

    public bool IsFinished()
    {
        return Vector3.Distance(end, gameObject.transform.position) < 0.05;
    }

    public void UpdateAnimationMatrix(ref Matrix4x4 m, long tick, float scale)
    {
        gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.right, end - start);

        var total_t = Vector3.Distance(start, end) / speed;
        var total_progress = (tick - spawn_tick) * 1.0f / total_t;
        transform.position = Vector3.Lerp(start, end, total_progress);
    }

    HashSet<GameObject> collidedObjects = new HashSet<GameObject>();
    void Update()
    {
        if (IsFinished())
        {
            gameObject.SetActive(false);
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, gameObject.transform.localScale.x * 5);
        foreach (Collider collider in colliders)
        {
            if (collidedObjects.Contains(collider.gameObject))
                continue;
            collidedObjects.Add(collider.gameObject);

            var receiver = collider.GetComponent<RegularEnemy>();
            if (receiver != null)
                receiver.TakeDamage(DamageType.Blade, 100, Tick.tick);
        }
    }
}
