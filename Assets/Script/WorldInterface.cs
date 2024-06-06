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
    public SpriteManager sprite_manager = new SpriteManager();

    // Start is called before the first frame update
    GameObject prefab_basic_explosion;
    GameObject prefab_basic_blade;
    GameObject prefab_basic_laser;

    void InitializePrefabs()
    {
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
                Resources.Load<Material>("Material/Unlit_BladeFlipBook");

        prefab_basic_laser = Instantiate(Resources.Load<GameObject>("Prefabs/BasicEntity"));
        prefab_basic_laser.SetActive(false);
        prefab_basic_laser.AddComponent<LaserControl>();
        prefab_basic_laser.AddComponent<FlipbookRender>();
        prefab_basic_laser.GetComponent<Renderer>().material =
                Resources.Load<Material>("Material/Unlit_LaserFlipBook");
    }

    public GameObject game_data;
    GameObject game_control;
    public GameObject music_control;
    void Start()
    {
        Application.targetFrameRate = 120;
        sprite_manager.Initialize();

        game_data = new GameObject();
        game_data.name = "GameData";
        game_data.AddComponent<GameData>();

        var cc = GameObject.Find("MainCamera").GetComponent<CameraController>();

        game_control = new GameObject();
        game_control.AddComponent<GameControl>().Initialize(cc, this);
        game_control.name = "GameControl";

        music_control = new GameObject();
        music_control.AddComponent<MusicControl>();
        music_control.name = "MusicControl";

        InitializePrefabs();
    }

    List<GameObject> explosions = new List<GameObject>();
    public void ExplosionAt(Vector3 l, float scale = 1)
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
                        l,
                        scale
                    );
        explosion.GetComponent<ExplosionControl>().Initialize(Tick.tick, GameInstance.GetInstance().GlobalSpeedCoef());
        explosion.transform.localScale = Vector3.zero;
        explosion.name = "Explosion";
    }

    public int delay_tick = 12;
    public bool stop = true;

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
                        e1,
                        3
                    );
        blade.GetComponent<BladeControl>().Initialize(Tick.tick, e1, e2,
            1.0f * GameInstance.GetInstance().GlobalSpeedCoef());
        blade.name = "Blade";
    }
    

    GameObject laser = null;
    public void LaserStop()
    {
        if (laser != null)
            laser.SetActive(false);
    }
    public void LaserAt(Vector3 e1)
    {
        var ne2 = game_data.GetComponent<GameData>().FindClosestMonster(e1);
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

    public void OnPauseGame()
    {
        if (hud == null)
        {
            hud = GameObject.Find("Canvas");
            if (hud)
                hud.GetComponent<InGameMenuControl>().Initialize();
        }
        if (!hud)
            return;

        var mc = hud.GetComponent<InGameMenuControl>();
        if (mc.IsMenuActive() && mc.game_state == "Pause")
        {
            mc.ShowMenu(false);
            game_control.GetComponent<GameControl>().FMOD_PauseBGM(false);
            return;
        }

        if (!mc.IsMenuActive())
        {
            mc.ShowMenu(true);
            mc.SetGameState("Pause");
            game_control.GetComponent<GameControl>().FMOD_PauseBGM(true);
            return;
        }
    }

    Int64 first_tick = -1;
    GameObject hud = null;
    // Update is called once per frame
    void Update()
    {
        if (first_tick < 0)
            first_tick = Tick.tick;

        bool is_bgm_playing = game_control.GetComponent<GameControl>().FMOD_IsBGMPlaying();
        var secret = game_data.GetComponent<GameData>().secret;
        bool secret_destoryed = secret != null && !secret.GetComponent<RegularEnemy>().IsAlive();

        if (hud == null)
        {
            hud = GameObject.Find("InGameHUD");
            if (hud)
                hud.GetComponent<InGameMenuControl>().Initialize();
        }
        if (hud)
        {
            var mc = hud.GetComponent<InGameMenuControl>();

            if (secret_destoryed)
            {
                game_control.GetComponent<GameControl>().FMOD_StopBGM();
                mc.SetGameState("Game Over");
                mc.ShowMenu(true);
                return;
            }

            if (!is_bgm_playing && first_tick + 2 < Tick.tick)
            {
                game_control.GetComponent<GameControl>().FMOD_StopBGM();
                mc.SetGameState("You win!");
                mc.ShowMenu(true);
                return;
            }

            if (mc.IsMenuActive())
                return;
        }
        

        Tick.TickUpdate();

        var cc = GameObject.Find("MainCamera").GetComponent<CameraController>();
        game_data.GetComponent<GameData>().GenerateEnemies(cc.GetCameraBounds(), this);
        game_data.GetComponent<GameData>().Refresh();

        foreach (var e in explosions)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);
        foreach (var e in blades)
            e.GetComponent<FlipbookRender>().Refresh(Tick.tick);
        if (laser != null)
            laser.GetComponent<FlipbookRender>().Refresh(Tick.tick);
    }
}
