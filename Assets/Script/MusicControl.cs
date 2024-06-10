using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class EnumerableExt
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
    {
        return list.Select(p => new { p, rank = UnityEngine.Random.Range(.1f, 1.0f) })
            .OrderBy(x => x.rank)
            .Select(x => x.p);
    }
}

public class MusicControl : MonoBehaviour
{
    // Start is called before the first frame update

    List<AudioSource> single_click_musics = new List<AudioSource>();
    List<AudioSource> long_click_musics = new List<AudioSource>();
    List<AudioSource> swip_musics = new List<AudioSource>();

    AudioSource AddAudioSource(string name, string path)
    {
        GameObject pad = new GameObject();
        pad.transform.SetParent(gameObject.transform);
        pad.name = name;
        var audio_source = pad.AddComponent<AudioSource>();

        AudioClip audioClip = Resources.Load<AudioClip>(path);
        audio_source.clip = audioClip;
        audio_source.playOnAwake = false;
        audio_source.volume = 0.5f;

        return audio_source;
    }

    List<EventInstance> fx_single_click_musics = new List<EventInstance>();
    List<EventInstance> fx_long_click_musics = new List<EventInstance>();
    List<EventInstance> fx_swip_musics = new List<EventInstance>();
    List<EventInstance> fx_level4_music = new List<EventInstance>();

    void Start()
    {
        for (int i = 0; i < 10; ++i)
        {
            single_click_musics.Add(AddAudioSource("SingleClick" + i, "SoundEffect/SoundFX/Click/" + (i + 1)));
            long_click_musics.Add(AddAudioSource("LongClick" + i, "SoundEffect/SoundFX/Hold/" + (i + 1)));
            swip_musics.Add(AddAudioSource("Swip" + i, "SoundEffect/SoundFX/Flick/" + (i + 1)));
        }

        for (int i = 0; i < 40; ++i)
        {
            var event_path = GameInstance.GetInstance().fx_event;
            fx_single_click_musics.Add(RuntimeManager.CreateInstance(event_path));
            fx_long_click_musics.Add(RuntimeManager.CreateInstance(event_path));
            fx_swip_musics.Add(RuntimeManager.CreateInstance(event_path));
        }

        for (int i = 0; i < 8; ++i)
            fx_level4_music.Add(RuntimeManager.CreateInstance("event:/FX/FX_Level4"));

        foreach (var x in long_click_musics)
            x.loop = true;
    }


    CameraController cc;
    Bounds? bound = null;
    List<int> index_shuffle = new List<int>();
    private int GetIndex(Vector3 l)
    {
        if (bound == null)
        {
            bound = GameObject.Find("MainCamera").GetComponent<CameraController>().GetCameraBounds();
            index_shuffle = Enumerable.Range(0, 40).Select(i => i / 4).Shuffle().ToList();
        }

        var x_span = bound.Value.max.x - bound.Value.min.x;
        var z_span = bound.Value.max.z - bound.Value.min.z;

        var x_length = x_span / 8;
        var z_length = z_span / 5;

        var adj_l = l - bound.Value.min;

        var index = (int)(adj_l.x / x_length) * 5 + (int)(adj_l.z / z_length);

        var adj_index = Mathf.Clamp(index, 0, index_shuffle.Count - 1);

        Debug.LogFormat("MusicIndex: {0}, {1}", l, index_shuffle[adj_index]);
        return index_shuffle[adj_index];
    }

    public bool IsSameRegion(Vector3 l, Vector3 r)
    {
        return GetIndex(l) == GetIndex(r);
    }

    private int current_level4_index = 0;
    private List<float> level4_parameter;
    public bool Level4Click(Vector3 l)
    {
        if (GameInstance.GetInstance().level_name != "Level4")
            return false;
        if (level4_parameter == null)
        {
            level4_parameter = fx_level4_music.Select(v => 0.0f).ToList();
            foreach (var e in fx_level4_music)
                e.start();
        }

        for (int i = 0; i < 5; ++i)
        {
            var index = (current_level4_index - i + level4_parameter.Count*100) % level4_parameter.Count;
            if (i == 0)
                level4_parameter[index] = 2;
            else if (level4_parameter[index] > 0)
                level4_parameter[index] = 2;
        }
        ++current_level4_index;
        current_level4_index %= level4_parameter.Count;
        return true;
    }
    public void Level4Advance()
    {
        if (GameInstance.GetInstance().level_name != "Level4" || level4_parameter == null)
            return;

        float total_tick = 120 * 3.0f;
        for (int i = 0; i < level4_parameter.Count; ++i)
        {
            level4_parameter[i] -= 1 / total_tick;
            if (level4_parameter[i] < 0)
                level4_parameter[i] = 0;
        }

        for (int i = 0; i < level4_parameter.Count; ++i)
        {
            var eve = fx_level4_music[i];
            eve.setParameterByName(string.Format("OnOff{0}", i+1), level4_parameter[i]);
            eve.setVolume(level4_parameter[i] > 1 ? 1.0f : level4_parameter[i]);
        }
    }

    public void SingleClickAt(Vector3 l)
    {
        if (Level4Click(l))
            return;

        var eve = fx_single_click_musics[GetIndex(l)];

        eve.setParameterByName("InputType", 1);
        eve.setParameterByName("Inst", (GetIndex(l) + 1) % 4);
        eve.start();
    }

    public void SwipClickAt(Vector3 l)
    {
        if (Level4Click(l))
            return;
        var eve = fx_swip_musics[GetIndex(l)];

        eve.setParameterByName("InputType", 2);
        eve.setParameterByName("Inst", (GetIndex(l) + 1) % 4);
        eve.start();
    }

    public void HoldAt(Vector3 l)
    {
        if (Level4Click(l))
            return;
        var eve = fx_long_click_musics[GetIndex(l)];

        eve.getPlaybackState(out var state);
        if (state != PLAYBACK_STATE.PLAYING)
        {
            eve.setParameterByName("InputType", 3);
            eve.setParameterByName("Inst", (GetIndex(l) + 1) % 4);
            eve.start();
        }
    }

    public void HoldStopAt(Vector3 l)
    {
        var eve = fx_long_click_musics[GetIndex(l)];
        eve.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
