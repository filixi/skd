using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public enum EnemyType
    {
        Dog,
        Standard,
        Ranged,
        Shield,
        Special,
        Secret
    }

    Dictionary<EnemyType, EnemyPackage> enemy_data;
    GameObject prefab_basic_enemy;
    void LoadEnemyData(WorldInterface wi)
    {
        enemy_data = new Dictionary<EnemyType, EnemyPackage>();

        { // dog
            var dog = new EnemyPackage();
            dog.hp_cap = 500;
            dog.end = new Vector3(0, 0, 0);
            dog.speed = 0.005f;
            dog.attack_interval = 60;
            dog.attack_range = 3.0f;
            dog.dph = 30;
            dog.name = "Dog";

            enemy_data.Add(EnemyType.Dog, dog);
        }

        { // close combat
            var close_combat = new EnemyPackage();
            close_combat.hp_cap = 500;
            close_combat.end = new Vector3(0, 0, 0);
            close_combat.speed = 0.005f;
            close_combat.attack_interval = 60;
            close_combat.attack_range = 3.0f;
            close_combat.dph = 30;
            close_combat.name = "CloseCombat";

            enemy_data.Add(EnemyType.Standard, close_combat);
        }

        { // ranged combat
            var ranged_combat = new EnemyPackage();
            ranged_combat.hp_cap = 500;
            ranged_combat.end = new Vector3(0, 0, 0);
            ranged_combat.speed = 0.005f;
            ranged_combat.attack_interval = 60;
            ranged_combat.attack_range = 3.0f;
            ranged_combat.dph = 30;
            ranged_combat.name = "Ranged";

            enemy_data.Add(EnemyType.Ranged, ranged_combat);
        }

        { // shield combat
            var shield_combat = new EnemyPackage();
            shield_combat.hp_cap = 500;
            shield_combat.end = new Vector3(0, 0, 0);
            shield_combat.speed = 0.005f;
            shield_combat.attack_interval = 60;
            shield_combat.attack_range = 3.0f;
            shield_combat.dph = 30;
            shield_combat.name = "Shield";

            enemy_data.Add(EnemyType.Shield, shield_combat);
        }

        { // special force
            var special_force = new EnemyPackage();
            special_force.hp_cap = 500;
            special_force.end = new Vector3(0, 0, 0);
            special_force.speed = 0.005f;
            special_force.attack_interval = 60;
            special_force.attack_range = 3.0f;
            special_force.dph = 30;
            special_force.name = "Special";

            enemy_data.Add(EnemyType.Special, special_force);
        }

        { // secret
            var secret = new EnemyPackage();
            secret.hp_cap = 2000;
            secret.end = new Vector3(0, 0, 0);
            secret.speed = 0.0f;
            secret.attack_interval = 0;
            secret.attack_range = 0;
            secret.dph = 0;
            secret.name = "Secret";
            secret.end = new Vector3(0, 0, 0);

            enemy_data.Add(EnemyType.Secret, secret);
        }

        prefab_basic_enemy = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_enemy.SetActive(false);
        prefab_basic_enemy.AddComponent<RegularEnemy>();
        prefab_basic_enemy.AddComponent<FlipbookRender>();
        prefab_basic_enemy.GetComponent<Renderer>().material =
            Instantiate(wi.sprite_manager.entity_material_entry.array_material);
    }

    public GameObject SpawnEnemy(EnemyType type, Vector3 location, WorldInterface wi)
    {
        if (enemy_data == null)
            LoadEnemyData(wi);

        var enemy = Instantiate(prefab_basic_enemy);
        ResetEnemy(enemy, type, location, wi);

        return enemy;
    }
    void ResetEnemy(GameObject enemy, EnemyType type, Vector3 location, WorldInterface wi)
    {
        enemy_data.TryGetValue(type, out var package);

        var enemy_package = new EnemyPackage();
        enemy_package.hp_cap = package.hp_cap;
        enemy_package.start = location;
        enemy_package.end = package.end;
        enemy_package.speed = package.speed;
        enemy_package.spawn_tick = Tick.tick;

        enemy_package.attack_range = package.attack_range;
        enemy_package.dph = package.dph;
        enemy_package.attack_interval = package.attack_interval;

        enemy.GetComponent<RegularEnemy>().Initialize(enemy_package);

        enemy.GetComponent<FlipbookRender>().Initialize(
                wi.sprite_manager.entity_material_entry.id_mapping[package.name].render_data,
                location
            );
        enemy.name = package.name;
        enemy.SetActive(true);
    }

    // enemy generator
    public GameObject secret = null;
    public List<GameObject> current_enemies = new List<GameObject>();
    Dictionary<EnemyType, float> distribution = new Dictionary<EnemyType, float> {
            {EnemyType.Dog, 1.0f },
            {EnemyType.Special, 0.2f },
            {EnemyType.Ranged, 0.8f },
            {EnemyType.Shield, 0.5f },
            {EnemyType.Standard, 1.5f }
        };
    public void GenerateEnemies(Bounds bound, int total, float rate_per_tick, WorldInterface wi)
    {
        if (secret == null)
        {
            secret = SpawnEnemy(EnemyType.Secret, new Vector3(0, 0, 0), wi);
            secret.AddComponent<Secret>();
        }

        var to_generate = UnityEngine.Random.Range(0.0f, 1.0f);
        if (to_generate > rate_per_tick)
            return;
        
        var sum = distribution.Values.Sum();
        var random = UnityEngine.Random.Range(0, sum);
        EnemyType? type_to_generate = null;
        foreach (var kv in distribution)
        {
            type_to_generate = kv.Key;
            random -= kv.Value;
            if (random <= 0)
                break;
        }
        
        if (type_to_generate == null)
            return;

        var l = new Vector3(0, 0, 0);

        int count_down = 50;
        while (Vector3.Distance(l, Vector3.zero) < 10.0)
        {
            if (count_down-- <= 0)
                return;
            l = new Vector3(
                UnityEngine.Random.Range(bound.min.x, bound.max.x),
                0,
                UnityEngine.Random.Range(bound.min.z, bound.max.z));
        }

        foreach (var go in current_enemies)
        {
            if (!go.GetComponent<RegularEnemy>().IsAlive())
            {
                ResetEnemy(go, type_to_generate.Value, l, wi);
                return;
            }
        }

        if (current_enemies.Count > total)
            return;
        current_enemies.Add(SpawnEnemy(type_to_generate.Value, l, wi));
    }

    public Vector3? FindClosestMonster(Vector3 e)
    {
        if (current_enemies.Count <= 0)
            return null;

        GameObject min = null;
        float distance = 0;
        foreach (var v in current_enemies)
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

    // Update is called once per frame
    void Update()
    {
        foreach (var e in current_enemies)
            e.GetComponent<RegularEnemy>().Refresh(Tick.tick);
        foreach (var e in current_enemies)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);

        secret.GetComponent<RegularEnemy>().Refresh(Tick.tick);
        secret.GetComponent<FlipbookRender>().Refresh(Tick.tick);
    }

}
