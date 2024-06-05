using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class BeatControl
{
    BeatControl(long bpm, long fps)
    {
        this.bpm = bpm;
        this.fps = fps;
    }

    public void Tick()
    {
        ++tick_since_start;
    }
    public void Reset()
    {
        tick_since_start = 0;
    }

    public long GetCurrentTick()
    {
        return tick_since_start;
    }
    public long GetCurrentNote(long note_length)
    {
        // tick_since_start / (GetBeatLength() / upper * 4 / note_length);
        return tick_since_start * upper * note_length / (4 * GetBeatLength());
    }
    public long GetBeatLength()
    {
        return 60 * fps / bpm;
    }
    public long GetMeasureLength()
    {
        return GetBeatLength() * lower;
    }

    // how many lower beat are there in a measure
    long upper = 4;
    long lower = 4;

    long bpm = 120;
    long fps = 60;
    long tick_since_start = 0;
}

public enum ControlState
{
    TouchRegularControlState,
    KeyboardRegularControlState,
    BossFightState
}

interface IControlState
{
    ControlState GetControlState();
    void UpdateState(CameraController cc, WorldInterface wi, GameControl gc);
    bool IsNeighbor(KeyCode a, KeyCode b);
}

class TouchRegularControlState : IControlState
{
    public ControlState GetControlState()
    {
        return ControlState.TouchRegularControlState;
    }

    public bool IsNeighbor(KeyCode a, KeyCode b)
    {
        return false;
    }

    List<Vector3> sample = new List<Vector3>();
    public void UpdateState(CameraController cc, WorldInterface wi, GameControl gc)
    {
        float speed = 0;
        if (sample.Count > 0)
            speed = Vector3.Distance(sample[sample.Count - 1], sample.First()) / sample.Count() * 60;

        if (Input.GetMouseButton(0))
        {
            var at = cc.GetMouseLocation();
            sample.Add(at);

            // if speed is too slow
        }

        if (Input.GetMouseButton(0) && sample.Count > 20)
        {
            if (sample.Count > 40)
                sample = sample.Skip(20).ToList();
            gc.LongClickAt(sample.Last());
        }

        if (Input.GetMouseButtonUp(0) && sample.Count > 0)
        {
            var up = cc.GetMouseLocation();

            gc.LongClickStop();

            if (sample.Count < 20 && speed > 15)
            {
                var intersection = cc.ComputeLineScreenIntersection(sample.First(), sample.Last());
                if (intersection != null)
                    gc.SwipAt(intersection.Item1, intersection.Item2);
            } else if (sample.Count < 20) {
                gc.SingleClickAt(up, KeyCode.None);
            }

            // Debug.Log("Start" + sample.First() + "Tick:" + sample.Count.ToString() + " v:" + speed.ToString());
            sample.Clear();
        }
    }
}

class KeyboardRegularControlState : IControlState
{
    public ControlState GetControlState()
    {
        return ControlState.KeyboardRegularControlState;
    }

    Dictionary<KeyCode, KeyCode> remap = new Dictionary<KeyCode, KeyCode>()
    {
        { KeyCode.Alpha1, KeyCode.Q },
        { KeyCode.Alpha2, KeyCode.W },
        { KeyCode.Alpha3, KeyCode.E },
        { KeyCode.Alpha4, KeyCode.R },
        { KeyCode.Alpha5, KeyCode.T },
        { KeyCode.Alpha6, KeyCode.Y },
        { KeyCode.Alpha7, KeyCode.U },
        { KeyCode.Alpha8, KeyCode.I },
        { KeyCode.Alpha9, KeyCode.O },
        { KeyCode.Alpha0, KeyCode.P },
    };

    List<List<KeyCode>> attack_key = new List<List<KeyCode>> {
            // new List<KeyCode>{ KeyCode.Alpha1, KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4,KeyCode.Alpha5,KeyCode.Alpha6,KeyCode.Alpha7,KeyCode.Alpha8,KeyCode.Alpha9,KeyCode.Alpha0 },
            new List<KeyCode>{ KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket },
            new List<KeyCode>{ KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote },
            new List<KeyCode>{ KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, KeyCode.Slash},
        };

    public class KeyInfo
    {
        public List<KeyCode> keys = new List<KeyCode>();

        public Bounds bound = new Bounds();
        public Vector3 center;
        public List<KeyCode> neighbors = new List<KeyCode>();
    }
    Dictionary<KeyCode, KeyInfo> key_info;

    public void ResetKeyInfo(CameraController cc)
    {
        var bound = cc.GetCameraBounds();

        // left to right
        var x_span = bound.max.x - bound.min.x;
        var z_span = bound.max.z - bound.min.z;

        key_info = new Dictionary<KeyCode, KeyInfo>();
        for (int i = 0; i < attack_key.Count; ++i)
        {
            for (int j = 0; j < attack_key[i].Count; ++j)
            {
                var has_info = key_info.TryGetValue(attack_key[i][j], out var value);
                if (value == null)
                    value = new KeyInfo();

                value.keys.Add(attack_key[i][j]);
                
                value.bound.min = new Vector3(
                        x_span / attack_key[i].Count * j + bound.min.x,
                        0,
                        -z_span / attack_key.Count * i - bound.min.z
                    );
                value.bound.max = new Vector3(
                        x_span / attack_key[i].Count * (j+1) + bound.min.x,
                        0,
                        -z_span / attack_key.Count * (i+1) - bound.min.z
                    );
                value.center = (value.bound.min + value.bound.max) / 2.0f;

                if (!has_info)
                    key_info.Add(attack_key[i][j], value);
            }
        }

        foreach (var kv in remap)
        {
            key_info.TryGetValue(kv.Value, out var value);
            if (value != null)
                key_info.Add(kv.Key, value);
        }
    }
    public KeyInfo GetKeyBox(KeyCode key, CameraController cc)
    {
        if (key_info == null)
            ResetKeyInfo(cc);

        key_info.TryGetValue(key, out var value);
        return value;
    }

    Dictionary<KeyCode, HashSet<KeyCode>> neighbors;

    public bool IsNeighbor(KeyCode a, KeyCode b)
    {
        Action<int, int, int, int> add_neighbor = (int k1, int k2, int k3, int k4) =>
        {
            if (k3 >= 0 && k3 < attack_key.Count)
            {
                if (k4 >= 0 && k4 < attack_key[k3].Count)
                    neighbors[attack_key[k1][k2]].Add(attack_key[k3][k4]);
            }
        };

        if (neighbors == null)
        {
            neighbors = new Dictionary<KeyCode, HashSet<KeyCode>>();
            for (int i = 0; i < attack_key.Count; ++i)
            {
                for (int j = 0; j < attack_key[i].Count; ++j)
                {
                    neighbors.Add(attack_key[i][j], new HashSet<KeyCode>());
                    add_neighbor(i, j, i - 1, j - 1);
                    add_neighbor(i, j, i - 1, j);
                    add_neighbor(i, j, i - 1, j + 1);
                    add_neighbor(i, j, i, j - 1);
                    add_neighbor(i, j, i, j);
                    add_neighbor(i, j, i, j + 1);
                    add_neighbor(i, j, i + 1, j - 1);
                    add_neighbor(i, j, i + 1, j);
                    add_neighbor(i, j, i + 1, j + 1);
                }
            }
        }

        neighbors.TryGetValue(a, out var value);
        if (value == null)
            return false;
        return value.Contains(b);
    }


    // when to resolve key up
    // after time pressed
    // the same key clicked
    // neighbor key pressed
    //  check for the time passed
    //    if short, resolve as super bomb
    //    if long, resolve as swipe

    Dictionary<KeyCode, long> down_count = new Dictionary<KeyCode, long>();
    Dictionary<KeyCode, long> up_tick = new Dictionary<KeyCode, long>();
    public void UpdateState(CameraController cc, WorldInterface wi, GameControl gc)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            wi.OnPauseGame();

        List<KeyCode> keys = new List<KeyCode>();
        for (int i = 0; i < attack_key.Count; ++i)
        {
            for (int j = 0; j < attack_key[i].Count; ++j)
                keys.Add(attack_key[i][j]);
        }

        foreach (var re in remap.Keys)
        {
            keys.Add(re);
        }

        var key_down = keys.Where(key => Input.GetKey(key));
        var key_up = keys.Where(key => Input.GetKeyUp(key));

        foreach (var key in key_up)
        {
            var clicked_before = up_tick.ContainsKey(key);
            if (clicked_before)
            {
                up_tick.Remove(key);
                gc.SingleClickAt(GetKeyBox(key, cc).center, key);
            }

            bool consumed = false;
            foreach (var (kk, tick) in up_tick.ToList())
            {
                var is_neighbor = IsNeighbor(kk, key);
                if (is_neighbor && Tick.tick - tick > 2)
                {
                    var from = GetKeyBox(key, cc).center;
                    var to = GetKeyBox(kk, cc).center;
                    var (afrom, ato) = cc.ComputeLineScreenIntersection(from, to);
                    gc.SwipAt(ato, afrom);
                    up_tick.Remove(kk);
                    consumed = true;
                    break;
                }
                else
                {
                    gc.SingleClickAt(GetKeyBox(key, cc).center, key);
                    gc.SingleClickAt(GetKeyBox(kk, cc).center, kk);
                    up_tick.Remove(kk);
                    consumed = true;
                    break;
                }
            }

            if (!consumed)
                up_tick.Add(key, Tick.tick);
        }

        foreach (var (kk, tick) in up_tick.ToList())
        {
            if (Tick.tick - tick > 7)
            {
                gc.SingleClickAt(GetKeyBox(kk, cc).center, kk);
                up_tick.Remove(kk);
            }
        }

        HashSet<KeyCode> down_keys = new HashSet<KeyCode>();
        foreach (var key in key_down)
        {
            if (!down_keys.Contains(key))
                down_keys.Add(key);
        }
        foreach (var key in key_down)
        {
            if (!down_count.ContainsKey(key))
                down_count.Add(key, 1);
        }
        foreach (var key in down_count.Keys.ToList())
        {
            down_count.TryGetValue(key, out var val);
            down_count.Remove(key);
            if (down_keys.Contains(key))
                down_count.Add(key, val + 1);
        }
        foreach (var kv in down_count)
        {
            if (kv.Value > 30)
                gc.LongClickAt(GetKeyBox(kv.Key, cc).center);
        }
    }
}

public class GameControl : MonoBehaviour
{
    IControlState control_state = new KeyboardRegularControlState();

    AudioSource audio_source;
    void AudioSource_PlayBGM()
    {
        audio_source = gameObject.AddComponent<AudioSource>();
        AudioClip audioClip = Resources.Load<AudioClip>("SoundEffect/Level1");
        audio_source.clip = audioClip;
        audio_source.Play();
        audio_source.volume = 0.0f;
    }

    FMOD.Studio.EVENT_CALLBACK _musicFmodCallback;

    public static int index = 0;

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT FMODEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, System.IntPtr param, System.IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
            var name = (string)parameter.name;
            if (name.StartsWith("Stage"))
            {
                var number = name.Replace("Stage", "");
                GameControl.index = Int32.Parse(number) - 1;
            }
            UnityEngine.Debug.LogFormat("Marker: {0}", (string)parameter.name);
        }

        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            self.eventInstance.getTimelinePosition(out var position);

            // self.wi.ExplosionAt(new Vector3(0, 0, 0));
            // self.wi.music_control.GetComponent<MusicControl>().SingleClickAt(new Vector3(0, 0, 0), 0);

            // Debug.LogFormat("Tempo: {0}, {1}", parameter.tempo.ToString(), position);
        }

        return FMOD.RESULT.OK;
    }

    private EventInstance eventInstance;
    private static GameControl self;
    void FMOD_PlayBGM()
    {
        _musicFmodCallback = new FMOD.Studio.EVENT_CALLBACK(FMODEventCallback);

        self = this;
        var event_path = GameInstance.GetInstance().bgm_event;
        eventInstance = RuntimeManager.CreateInstance(event_path);

        eventInstance.setCallback(_musicFmodCallback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        eventInstance.start();
    }

    public void FMOD_StopBGM()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public bool FMOD_IsBGMPlaying()
    {
        FMOD.Studio.PLAYBACK_STATE state;
        eventInstance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    public void FMOD_PauseBGM(bool paused)
    {
        eventInstance.setPaused(paused);
    }

    // Start is called before the first frame update
    void Start()
    {
        FMOD_PlayBGM();
    }

    CameraController cc;
    WorldInterface wi;
    public void Initialize(CameraController cc, WorldInterface wi)
    {
        this.cc = cc;
        this.wi = wi;
    }

    Queue<Action<float>> pending_user_input = new Queue<Action<float>>();
    List<Tuple<Vector3, KeyCode>> singe_click_cluster = new List<Tuple<Vector3, KeyCode>>();
    public void SingleClickAt(Vector3 l, KeyCode kc)
    {
        pending_user_input.Enqueue((float now) =>
        {
            var next = (int)(now + 125) / 125 * 125;

            singe_click_cluster.Add(Tuple.Create(l, kc));
        });
    }
    public void SwipAt(Vector3 l1, Vector3 l2)
    {
        pending_user_input.Enqueue((float now) =>
        {
            wi.BladeAt(l1, l2);
            wi.music_control.GetComponent<MusicControl>().SwipClickAt(l1);
        });
    }

    Queue<Vector3> pending_long_click = new Queue<Vector3>();
    public void LongClickAt(Vector3 l1)
    {
        pending_user_input.Enqueue((float now) =>
        {
            pending_long_click.Enqueue(l1);
        });
    }
    public void LongClickStop()
    {
        pending_long_click.Clear();
    }

    // Update is called once per frame

    Vector3? laser;
    int ttick = 0;
    int expecting_tick = 0;
    int last_enter_tick = 0;
    void Update()
    {
        wi.game_data.GetComponent<GameData>().current_frag_index = index;

        control_state.UpdateState(cc, wi, this);

        ++ttick;
        var interval = 15;

        if (!wi.stop && UnityEngine.Random.Range(0, 1) < 0.05)
        {
            if (pending_user_input.Count <= 0)
                SingleClickAt(new Vector3(0, 0, 0), KeyCode.None);
        }

        int position = 0;
        // position = (int)(audio_source.time * 1000));
        eventInstance.getTimelinePosition(out position);

        var tick = Mathf.RoundToInt(position / 1000.0f * 60);

        // 120 bpm
        // 2 1/4 per second
        // 4 1/8 per second
        // 8 1/16 per second

        var delta_tick = tick;

        if (delta_tick % interval == wi.delay_tick)
            expecting_tick = ttick + interval;
        
        bool synchronized = false;
        if ((delta_tick % interval == wi.delay_tick && ttick - last_enter_tick > 10) || (expecting_tick - ttick) % interval == 0)
            synchronized = true;
        if (!synchronized)
            return;

        last_enter_tick = ttick;

        foreach (var e in pending_user_input)
            e.Invoke(position * 1.0f);
        pending_user_input.Clear();

        // Debug.LogFormat("DT {0} {1}", position, Time.time * 1000);

        var music_control = wi.music_control.GetComponent<MusicControl>();

        if (singe_click_cluster.Count > 0)
        {
            cc.GetCameraBounds();

            HashSet<KeyCode> used = new HashSet<KeyCode>();
            for (int i = 0; i < singe_click_cluster.Count; ++i)
            {
                for (int j = i+1; j < singe_click_cluster.Count; ++j)
                {
                    var neighbor = control_state.IsNeighbor(singe_click_cluster[i].Item2, singe_click_cluster[j].Item2);
                    if (neighbor)
                    {
                        if (!used.Contains(singe_click_cluster[i].Item2))
                            used.Add(singe_click_cluster[i].Item2);
                        if (!used.Contains(singe_click_cluster[j].Item2))
                            used.Add(singe_click_cluster[j].Item2);
                        wi.ExplosionAt(
                                (singe_click_cluster[i].Item1 + singe_click_cluster[j].Item1) / 2,
                                2.5f
                            );
                        wi.music_control.GetComponent<MusicControl>().SingleClickAt(singe_click_cluster[i].Item1);
                    }
                }
            }

            for (int i = 0; i < singe_click_cluster.Count; ++i)
            {
                if (!used.Contains(singe_click_cluster[i].Item2))
                    wi.ExplosionAt(singe_click_cluster[i].Item1, 2);
                wi.music_control.GetComponent<MusicControl>().SingleClickAt(singe_click_cluster[i].Item1);
            }

            singe_click_cluster.Clear();
        }
        
        if (pending_long_click.Count <= 0 && laser != null)
        {
            music_control.HoldStopAt(laser.Value);
            wi.LaserStop();
            laser = null;
        }
        while (pending_long_click.Count > 0)
        {
            var l1 = pending_long_click.Dequeue();

            if (pending_long_click.Count == 0)
            {
                if (laser != null && !music_control.IsSameRegion(laser.Value, l1))
                {
                    music_control.HoldStopAt(laser.Value);
                    wi.LaserStop();
                }

                wi.LaserAt(l1);
                music_control.HoldAt(l1);
                laser = l1;
            }
        }
    }
}
