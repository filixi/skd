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
    RegularState,
    BossFightState
}

interface IControlState
{
    ControlState GetControlState();
    void UpdateState(CameraController cc, WorldInterface wi, GameControl gc);
}

class RegularControlState : IControlState
{
    public ControlState GetControlState()
    {
        return ControlState.RegularState;
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
                gc.SingleClickAt(up);
            }

            // Debug.Log("Start" + sample.First() + "Tick:" + sample.Count.ToString() + " v:" + speed.ToString());
            sample.Clear();
        }
    }
}

public class GameControl : MonoBehaviour
{
    IControlState control_state = new RegularControlState();

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
    FMOD.Studio.EventInstance _musicEventInstance;


    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT FMODEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, System.IntPtr param, System.IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
            Debug.LogFormat("Marker: {0}", (string)parameter.name);
        }

        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            Debug.LogFormat("Marker: {0}", parameter.tempo.ToString());
        }

        return FMOD.RESULT.OK;
    }

    private EventInstance eventInstance;
    void FMOD_PlayBGM()
    {
        // FMOD.Studio.EventDescription desc = FMODUnity.RuntimeManager.GetEventDescription("event:/Parent");
        // desc.createInstance(out _musicEventInstance);

        _musicFmodCallback = new FMOD.Studio.EVENT_CALLBACK(FMODEventCallback);

        eventInstance = RuntimeManager.CreateInstance("event:/BGM");

        eventInstance.setCallback(_musicFmodCallback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        eventInstance.start();
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
    Dictionary<int, Queue<Action<float>>> scheduled_user_input = new Dictionary<int, Queue<Action<float>>>();

    public void SingleClickAt(Vector3 l)
    {
        pending_user_input.Enqueue((float now) =>
        {
            var next = (int)(now + 125) / 125 * 125;

            wi.ExplosionAt(l);
            wi.music_control.GetComponent<MusicControl>().SingleClickAt(l, -1 /*wi.delay_dsp_clock * 46.3f*/);
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
    void Update()
    {
        control_state.UpdateState(cc, wi, this);

        ++ttick;
        if (!wi.stop && UnityEngine.Random.Range(0, 1) < 0.05)
        {
            // if (ttick % 250 < 125)
            //if (scheduled_user_input.Count <= 0)
            //    SingleClickAt(new Vector3(0, 0, 0));

            //if (pending_user_input.Count <= 0)
            //    SingleClickAt(new Vector3(0, 0, 0));
        }

        int position = 0;
        // position = (int)(audio_source.time * 1000));
        eventInstance.getTimelinePosition(out position);

        var tick = Mathf.RoundToInt(position / 1000.0f * 60);

        // 120 bpm
        // 2 1/4 per second
        // 4 1/8 per second
        // 8 1/16 per second

        if (tick % 15 == wi.delay_tick)
        {
            foreach (var e in pending_user_input)
                e.Invoke(position * 1.0f);
            pending_user_input.Clear();
        }

        //var next_play = wi.delay_tick - tick % 15 + ttick;
        //var adj_next_play = next_play > ttick ? next_play  : next_play;
        //while (pending_user_input.Count > 0)
        //{
        //    if (!scheduled_user_input.ContainsKey(adj_next_play))
        //        scheduled_user_input.Add(adj_next_play, new Queue<Action<float>>());
        //    scheduled_user_input[adj_next_play].Enqueue(pending_user_input.Dequeue());
        //}

        //scheduled_user_input.TryGetValue(ttick, out var scheduled);
        //if (scheduled != null)
        //{
        //    foreach (var a in scheduled)
        //        a.Invoke(position / 1.0f);
        //}
        //var out_dated = scheduled_user_input.Keys.Where(v => v <= ttick).ToList();
        //foreach (var v in out_dated)
        //    scheduled_user_input.Remove(v);

        if (pending_long_click.Count <= 0 && tick % 15 == wi.delay_tick)
        {
            if (laser != null)
                wi.music_control.GetComponent<MusicControl>().HoldStopAt(laser.Value);
            wi.LaserStop();
        }
        while (pending_long_click.Count > 0)
        {
            var l1 = pending_long_click.Dequeue();

            if (pending_long_click.Count == 0)
            {
                if (laser != null && laser.Value != l1)
                    wi.music_control.GetComponent<MusicControl>().HoldStopAt(laser.Value);

                wi.LaserAt(l1);
                wi.music_control.GetComponent<MusicControl>().HoldAt(l1);
                laser = l1;
            }
        }
    }
}
