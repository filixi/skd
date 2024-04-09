using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Tick
{
    public static void TickUpdate()
    {
        ++tick;
    }

    public static long tick
    {
        get;
        private set;
    }
}


public class WorldInterface : MonoBehaviour
{
    SpriteManager sprite_manager = new SpriteManager();

    // Start is called before the first frame update
    List<GameObject> test_enemies;
    GameObject prefab_basic_enemy;
    GameObject prefab_basic_explosion;
    GameObject prefab_basic_blade;
    GameObject prefab_basic_laser;

    void InitializePrefabs()
    {
        prefab_basic_enemy = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_enemy.SetActive(false);
        prefab_basic_enemy.AddComponent<RegularEnemy>();
        prefab_basic_enemy.AddComponent<FlipbookRender>();
        prefab_basic_enemy.GetComponent<Renderer>().material =
            Instantiate(sprite_manager.entity_material_entry.array_material);

        prefab_basic_explosion = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_explosion.SetActive(false);
        prefab_basic_explosion.AddComponent<ExplosionControl>();
        prefab_basic_explosion.AddComponent<FlipbookRender>();
        prefab_basic_explosion.GetComponent<Renderer>().material =
            Instantiate(sprite_manager.entity_material_entry.array_material);

        prefab_basic_blade = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_blade.SetActive(false);
        prefab_basic_blade.AddComponent<BladeControl>();
        prefab_basic_blade.AddComponent<FlipbookRender>();
        prefab_basic_blade.GetComponent<Renderer>().material =
            Instantiate(sprite_manager.entity_material_entry.array_material);

        prefab_basic_laser = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_laser.SetActive(false);
        prefab_basic_laser.AddComponent<LaserControl>();
        prefab_basic_laser.AddComponent<FlipbookRender>();
        prefab_basic_laser.GetComponent<Renderer>().material =
            Instantiate(sprite_manager.entity_material_entry.array_material);
    }

    GameObject game_control;
    public GameObject music_control;
    void Start()
    {
        Application.targetFrameRate = 60;
        sprite_manager.Initialize();

        var cc = GameObject.Find("MainCamera").GetComponent<CameraController>();

        game_control = new GameObject();
        game_control.AddComponent<GameControl>().Initialize(cc, this);
        game_control.name = "GameControl";

        music_control = new GameObject();
        music_control.AddComponent<MusicControl>();
        music_control.name = "MusicControl";

        InitializePrefabs();

        test_enemies = new List<GameObject>();
        int enemy_count = 10;
        for (int i = 0; i < enemy_count; ++i)
        {
            var l = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), 0, UnityEngine.Random.Range(-10.0f, 10.0f));
            
            var enemy = Instantiate(prefab_basic_enemy);
            enemy.SetActive(true);

            var enemy_package = new EnemyPackage();
            enemy_package.hp_cap = 500;
            enemy_package.start = l;
            enemy_package.end = new Vector3(0, 0, 0);
            enemy_package.speed = 0.005f;
            enemy.GetComponent<RegularEnemy>().Initialize(enemy_package);

            enemy.GetComponent<FlipbookRender>().Initialize(
                    sprite_manager.entity_material_entry.id_mapping["Dog"].render_data,
                    l
                );
            enemy.name = "Dog";

            test_enemies.Add(enemy);
        }
    }

    List<GameObject> explosions = new List<GameObject>();
    public void ExplosionAt(Vector3 l)
    {
        l.y = 0.1f;

        GameObject explosion = null;
        foreach (var e in explosions)
        {
            if (e.GetComponent<ExplosionControl>().IsFinished(Tick.tick))
            {
                explosion = e;
                break;
            }
        }
        if (explosion == null)
        {
            explosion = Instantiate(prefab_basic_explosion);
            explosions.Add(explosion);
        }

        explosion.SetActive(true);
        explosion.GetComponent<FlipbookRender>().Initialize(
                        sprite_manager.entity_material_entry.id_mapping["Explosion"].render_data,
                        l
                    );
        explosion.GetComponent<ExplosionControl>().Initialize(Tick.tick);
        explosion.transform.localScale = Vector3.zero;
        explosion.name = "Explosion";
    }

    List<GameObject> blades = new List<GameObject>();
    public void BladeAt(Vector3 e1, Vector3 e2)
    {
        e2.y = e1.y = 0.2f;

        GameObject blade = null;
        foreach (var e in blades)
        {
            if (e.GetComponent<BladeControl>().IsFinished())
            {
                blade = e;
                break;
            }
        }
        if (blade == null)
        {
            blade = Instantiate(prefab_basic_blade);
            blades.Add(blade);
        }

        blade.SetActive(true);
        blade.GetComponent<FlipbookRender>().Initialize(
                        sprite_manager.entity_material_entry.id_mapping["Blade"].render_data,
                        e1
                    );
        blade.GetComponent<BladeControl>().Initialize(Tick.tick, e1, e2, 2.0f);
        blade.name = "Blade";
    }

    private Vector3? FindClosestMonster(Vector3 e)
    {
        if (test_enemies.Count <= 0)
            return null;

        GameObject min = null;
        float distance = 0;
        foreach (var v in test_enemies)
        {
            if (!v.GetComponent<RegularEnemy>().IsAlive())
                continue;

            var d = (v.transform.position - e).magnitude;
            if (min == null || distance > d)
            {
                distance = d;
                min = v;
            }
        }
        if (min == null)
            return null;
        return min.transform.position;
    }

    GameObject laser = null;
    public void LaserStop()
    {
        if (laser != null)
            laser.SetActive(false);
    }
    public void LaserAt(Vector3 e1)
    {
        var ne2 = FindClosestMonster(e1);
        if (ne2 == null)
            return;
        var e2 = ne2.Value;

        e2.y = e1.y = 0.2f;
        if (laser == null)
        {
            laser = Instantiate(prefab_basic_laser);
            laser.GetComponent<FlipbookRender>().Initialize(
                            sprite_manager.entity_material_entry.id_mapping["Laser"].render_data,
                            e1
                        );
            laser.name = "Laser";
        }

        laser.SetActive(true);
        laser.GetComponent<LaserControl>().Initialize(Tick.tick, e1, e2);
    }

    // Update is called once per frame
    void Update()
    {
        Tick.TickUpdate();

        foreach (var e in test_enemies)
            e.GetComponent<RegularEnemy>().Refresh(Tick.tick);

        foreach (var e in test_enemies)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);
        foreach (var e in explosions)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);
        foreach (var e in blades)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);
        if (laser != null)
            laser.GetComponent<FlipbookRender>().Refresh(Tick.tick);
    }
}
