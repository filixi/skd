using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum DamageType
{
    Explosive,
    Laser,
    Blade,
    Standard,
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

    public long last_attack_tick = 0;
    public int attack_interval = 0;
    public int dph = 0;

    public float attack_range;

    public string name;
}

public class AnimationInfo
{
    public int attack_frame_count;
    public int moving_frame_count;
    public int moving_first_frame;
}

public class RegularEnemy : MonoBehaviour, FlipbookRenderData
{
    EnemyPackage package;
    public int hp = 0;
    public float current_speed = 1.0f;
    public float accumulated_t = 0.0f;

    public EnemyPackage GetEnemyPackage()
    {
        return package;
    }


    AnimationInfo animation_info;
    public void Initialize(EnemyPackage package, AnimationInfo animation_info)
    {
        this.package = package;
        hp = package.hp_cap;
        current_speed = package.speed;
        accumulated_t = 0;

        this.animation_info = animation_info;
    }

    long last_damage_taken = -10000;
    public void TakeDamage(DamageType damage_type, int damage, long tick)
    {
        if (!IsAlive())
            return;

        if (name == "Secret" && damage_type != DamageType.Standard)
            return;
        if (damage_type == DamageType.Laser)
            current_speed = package.speed * 0.2f;

        hp -= damage;
        last_damage_taken = tick;
    }

    public bool IsAlive()
    {
        return hp > 0;
    }

    public bool JustHit(long tick)
    {
        var tick_since_hit = (tick - last_damage_taken);
        return tick_since_hit == 1;
    }

    public void Refresh(long tick)
    {
        if (!IsAlive())
            return;

        if (Vector3.Distance(transform.position, package.end) < 1)
            return;

        var start = package.start;
        var end = package.end;

        current_speed += package.speed * 0.005f;
        current_speed = Mathf.Clamp(current_speed, 0, package.speed);

        var total_t = Vector3.Distance(start, end) / package.speed;

        accumulated_t += current_speed / package.speed;

        var total_progress = accumulated_t * 1.0f / total_t;
        transform.position = Vector3.Lerp(start, end, total_progress);
    }

    public bool IsMoving(long tick)
    {
        return (transform.position - package.end).magnitude > 0.02;
    }

    public void UpdateAnimationMatrix(ref Matrix4x4 m, long tick, float scale)
    {
        m[1, 3] = gameObject.transform.position.x > package.end.x ? 1.0f : 0.0f;

        var damage_progress = (tick - last_damage_taken) / (60.0f * 0.5f);
        if (damage_progress > 0 && damage_progress < 1)
            m[1, 1] = damage_progress;

        if (!IsAlive())
        {
            var tick_since_dead = (tick - last_damage_taken);
            var slice_progress = tick_since_dead / (60.0f * 1);
            m[1, 0] = slice_progress;
            return;
        }

        if (animation_info.moving_frame_count <= 0)
            return;

        if (IsMoving(tick))
        {
            Vector4 render_data = new Vector4(1, 1, 0,
                tick / 10 % animation_info.moving_frame_count + animation_info.moving_first_frame);
            gameObject.GetComponent<Renderer>().material.SetVector("_Data", render_data);
        }
        else
        {
            Vector4 render_data = new Vector4(1, 1, 0,
                tick / 10 % animation_info.attack_frame_count);
            gameObject.GetComponent<Renderer>().material.SetVector("_Data", render_data);
        }
    }
}
