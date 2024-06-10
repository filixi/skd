using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameData;

public class GameData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public class EnemyRefreshFrag
    {
        public int total_count = 60;

        public float dog = 1;
        public float standard = 1;
        public float ranged = 1;
        public float shield = 1;
        public float special = 1;
    }
    List<EnemyRefreshFrag> enemy_refresh_sequence = new List<EnemyRefreshFrag>
    {
        new EnemyRefreshFrag{ total_count = 5, dog = 0.5f, standard = 0.4f, ranged = 0, shield = 0, special = 0 }, // 1
        new EnemyRefreshFrag{ total_count = 5, dog = 0.5f, standard = 0.4f, ranged = 0.4f, shield = 0, special = 0 }, // 2
        new EnemyRefreshFrag{ total_count = 5, dog = 0.4f, standard = 0.3f, ranged = 0.3f, shield = 0.3f, special = 0 }, // 3
        new EnemyRefreshFrag{ total_count = 10, dog = 1, standard = 1, ranged = 1, shield = 0.5f, special = 0 }, // 4
        new EnemyRefreshFrag{ total_count = 15, dog = 1, standard = 2, ranged = 1, shield = 0.5f, special = 0.5f }, // 5
        new EnemyRefreshFrag{ total_count = 15, dog = 1, standard = 1, ranged = 1, shield = 1, special = 1 }, // 6
        new EnemyRefreshFrag{ total_count = 20, dog = 2, standard = 2, ranged = 1, shield = 0.5f, special = 0.5f }, // 7
        new EnemyRefreshFrag{ total_count = 25, dog = 2, standard = 2, ranged = 2, shield = 1, special = 1 }, // 8
        new EnemyRefreshFrag{ total_count = 30, dog = 2, standard = 3, ranged = 2, shield = 1, special = 1 }, // 9
        new EnemyRefreshFrag{ total_count = 0, dog = 0, standard = 0, ranged = 0, shield = 0, special = 0 }, // 10
        new EnemyRefreshFrag{ total_count = 3, dog = 0.5f, standard = 0.4f, ranged = 0, shield = 0, special = 0 }, // 11
        new EnemyRefreshFrag{ total_count = 3, dog = 0.4f, standard = 0.5f, ranged = 0.5f, shield = 0, special = 0 }, // 12
        new EnemyRefreshFrag{ total_count = 4, dog = 0.7f, standard = 0.3f, ranged = 0.5f, shield = 0, special = 0 }, // 13
        new EnemyRefreshFrag{ total_count = 4, dog = 0.3f, standard = 0.5f, ranged = 0.3f, shield = 0.2f, special = 0 }, // 14
        new EnemyRefreshFrag{ total_count = 5, dog = 0.4f, standard = 0.3f, ranged = 0.3f, shield = 0.3f, special = 0 }, // 15
        new EnemyRefreshFrag{ total_count = 5, dog = 0.6f, standard = 0.8f, ranged = 0, shield = 0, special = 0 }, // 16
        new EnemyRefreshFrag{ total_count = 5, dog = 0.7f, standard = 0.6f, ranged = 0.4f, shield = 0, special = 0 }, // 17
        new EnemyRefreshFrag{ total_count = 8, dog = 0.3f, standard = 0.4f, ranged = 0.3f, shield = 0.2f, special = 0.1f }, // 18
    };
    public int current_frag_index = 0;
    EnemyRefreshFrag GetCurrentRefreshFrag()
    {
        current_frag_index = Mathf.Clamp(current_frag_index, 0, enemy_refresh_sequence.Count);
        if (current_frag_index >= enemy_refresh_sequence.Count)
            return new EnemyRefreshFrag();
        return enemy_refresh_sequence[current_frag_index];
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
        enemy_package.speed = package.speed * GameInstance.GetInstance().GlobalSpeedCoef();
        enemy_package.spawn_tick = Tick.tick;

        enemy_package.attack_range = package.attack_range;
        enemy_package.dph = package.dph;
        enemy_package.attack_interval = package.attack_interval;

        AnimationInfo a_info = new AnimationInfo();
        if (type == EnemyType.Dog)
        {
            a_info.moving_frame_count = 4;
            a_info.moving_first_frame = 13;
            a_info.attack_frame_count = 13;
            enemy.GetComponent<Renderer>().material =
                    Instantiate(Resources.Load<Material>("Material/Unlit_Dog"));
        }
        if (type == EnemyType.Special)
        {
            a_info.moving_frame_count = 8;
            a_info.moving_first_frame = 8;
            a_info.attack_frame_count = 5;
            enemy.GetComponent<Renderer>().material =
                    Instantiate(Resources.Load<Material>("Material/Unlit_Ex"));
        }
        if (type == EnemyType.Standard)
        {
            a_info.moving_frame_count = 4;
            a_info.moving_first_frame = 4;
            a_info.attack_frame_count = 4;
            enemy.GetComponent<Renderer>().material =
                    Instantiate(Resources.Load<Material>("Material/Unlit_Standard"));
        }
        if (type == EnemyType.Ranged)
        {
            a_info.moving_frame_count = 6;
            a_info.moving_first_frame = 13;
            a_info.attack_frame_count = 13;
            enemy.GetComponent<Renderer>().material =
                    Instantiate(Resources.Load<Material>("Material/Unlit_Gun"));
        }
        if (type == EnemyType.Shield)
        {

            a_info.moving_frame_count = 20;
            a_info.moving_first_frame = 20;
            a_info.attack_frame_count = 16;
            enemy.GetComponent<Renderer>().material =
                    Instantiate(Resources.Load<Material>("Material/Unlit_Shield"));
        }

        enemy.GetComponent<RegularEnemy>().Initialize(enemy_package, a_info);

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
    void GenerateEnemyOfType(Bounds bound, int count, EnemyType enemy_type, WorldInterface wi)
    {
        if (enemy_type == EnemyType.Secret)
            return;

        for (int i = 0; i < count; ++i)
        {
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
                    ResetEnemy(go, enemy_type, l, wi);
                    return;
                }
            }

            current_enemies.Add(SpawnEnemy(enemy_type, l, wi));
        }
    }
    public long score = 10;
    public void GenerateEnemies(Bounds bound, WorldInterface wi)
    {
        if (secret == null)
        {
            secret = SpawnEnemy(EnemyType.Secret, new Vector3(0, 0, 0), wi);
            secret.AddComponent<Secret>();
        }

        if (Tick.tick % 7 == 0)
            score = (long)(score / 1.15);
        if (score <= 0)
            score = 10;
        foreach (var go in current_enemies)
        {
            if (go.GetComponent<RegularEnemy>().JustHit(Tick.tick))
            {
                wi.score += score;
                score = score + 5;
            }
        }
        
        var setup = GetCurrentRefreshFrag();
        int current_count = current_enemies.Count(v => v.GetComponent<RegularEnemy>().IsAlive());
        float game_difficuty_coef = GameInstance.GetInstance().hard_mode ? 1 : 1;
        if (current_count >= setup.total_count * game_difficuty_coef)
            return;

        if (setup.dog > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < setup.dog * game_difficuty_coef / 60)
            GenerateEnemyOfType(bound, 1, EnemyType.Dog, wi);
        if (setup.standard > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < setup.standard * game_difficuty_coef / 60)
            GenerateEnemyOfType(bound, 1, EnemyType.Standard, wi);
        if (setup.ranged > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < setup.ranged * game_difficuty_coef / 60)
            GenerateEnemyOfType(bound, 1, EnemyType.Ranged, wi);
        if (setup.shield > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < setup.shield * game_difficuty_coef / 60)
            GenerateEnemyOfType(bound, 1, EnemyType.Shield, wi);
        if (setup.special > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < setup.special * game_difficuty_coef / 60)
            GenerateEnemyOfType(bound, 1, EnemyType.Special, wi);
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

    public void Refresh()
    {
        foreach (var e in current_enemies)
            e.GetComponent<RegularEnemy>().Refresh(Tick.tick);
        foreach (var e in current_enemies)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);

        secret.GetComponent<RegularEnemy>().Refresh(Tick.tick);
        secret.GetComponent<FlipbookRender>().Refresh(Tick.tick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
