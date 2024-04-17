using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Start is called before the first frame update
    void Start()
    {
        audio_source = gameObject.AddComponent<AudioSource>();
        AudioClip audioClip = Resources.Load<AudioClip>("SoundEffect/Level1");
        audio_source.clip = audioClip;
        audio_source.Play();
        audio_source.volume = 0.0f;
    }

    CameraController cc;
    WorldInterface wi;
    public void Initialize(CameraController cc, WorldInterface wi)
    {
        this.cc = cc;
        this.wi = wi;
    }

    Queue<Vector3> pending_single_click = new Queue<Vector3>();
    public void SingleClickAt(Vector3 l)
    {
        pending_single_click.Enqueue(l);
    }
    Queue<Tuple<Vector3, Vector3>> pending_swip = new Queue<Tuple<Vector3, Vector3>>();
    public void SwipAt(Vector3 l1, Vector3 l2)
    {
        pending_swip.Enqueue(Tuple.Create(l1, l2));
    }

    Queue<Vector3> pending_long_click = new Queue<Vector3>();
    public void LongClickAt(Vector3 l1)
    {
        pending_long_click.Enqueue(l1);
    }
    public void LongClickStop()
    {
        pending_long_click.Clear();
    }

    // Update is called once per frame

    Vector3? laser;
    void Update()
    {
        control_state.UpdateState(cc, wi, this);

        var tick = Mathf.RoundToInt(audio_source.time * 120);

        if (tick % 1 == 0 || (tick + 1) % 15 == 0 || (tick - 1) % 15 == 0)
        {
            while (pending_single_click.Count > 0)
            {
                var l = pending_single_click.Dequeue();
                wi.ExplosionAt(l);
                wi.music_control.GetComponent<MusicControl>().SingleClickAt(l);
            }
            while (pending_swip.Count > 0)
            {
                var (l1, l2) = pending_swip.Dequeue();
                wi.BladeAt(l1, l2);
                wi.music_control.GetComponent<MusicControl>().SwipClickAt(l1);
            }

            if (pending_long_click.Count <= 0)
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
}
