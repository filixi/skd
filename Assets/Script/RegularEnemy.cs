using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum DamageType
{
    Explosive,
    Laser,
    Blade
}

public enum EnemyType
{
    Dog,
    CloseCombat,
    Shooter
}

public class EnemyPackage
{
    public int hp_cap;
    public float speed = 1.0f;
    public Vector3 start;
    public Vector3 end;
    public long spawn_tick;
}

public class RegularEnemy : MonoBehaviour, FlipbookRenderData
{
    EnemyPackage package;
    int hp = 0;

    public void Initialize(EnemyPackage package)
    {
        this.package = package;
        hp = package.hp_cap;
    }

    long last_damage_taken = -10000;
    public void TakeDamage(DamageType damage_type, int damage, long tick)
    {
        if (!IsAlive())
            return;

        hp -= damage;
        last_damage_taken = tick;
    }

    public bool IsAlive()
    {
        return hp > 0;
    }

    public void Refresh(long tick)
    {
        if (!IsAlive())
            return;

        if (Vector3.Distance(transform.position, package.end) < 1)
            return;

        var start = package.start;
        var end = package.end;

        var total_t = Vector3.Distance(start, end) / package.speed;
        var total_progress = (tick - package.spawn_tick) * 1.0f / total_t;
        transform.position = Vector3.Lerp(start, end, total_progress);
    }

    public void UpdateAnimationMatrix(ref Matrix4x4 m, long tick)
    {
        m[1, 3] = gameObject.transform.position.x > package.end.x ? 1.0f : 0.0f;

        var damage_progress = (tick - last_damage_taken) / (60.0f * 0.5f);
        if (damage_progress > 0 && damage_progress < 1)
            m[1, 1] = damage_progress;

        if (!IsAlive())
        {
            var slice_progress = (tick - last_damage_taken) / (60.0f * 1);
            m[1, 0] = slice_progress;
        }
    }
}
