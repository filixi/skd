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

    private List<EventInstance> fxEventInstance = new List<EventInstance>();

    void Start()
    {
        for (int i = 0; i < 10; ++i)
        {
            single_click_musics.Add(AddAudioSource("SingleClick" + i, "SoundEffect/SoundFX/Click/" + (i + 1)));
            long_click_musics.Add(AddAudioSource("LongClick" + i, "SoundEffect/SoundFX/Hold/" + (i + 1)));
            swip_musics.Add(AddAudioSource("Swip" + i, "SoundEffect/SoundFX/Flick/" + (i + 1)));
        }

        for (int i = 0; i < 100; ++i)
            fxEventInstance.Add(RuntimeManager.CreateInstance("event:/FX"));

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

        var index = (adj_l.x / x_length) * 5 + (adj_l.z / z_length);

        var adj_index = Mathf.Clamp((int)index, 0, index_shuffle.Count - 1);

        Debug.Log("MusicIndex: " + adj_index);
        return index_shuffle[adj_index];
    }

    int indexx = 0;
    public void SingleClickAt(Vector3 l, float delay)
    {
        var local_index = indexx % fxEventInstance.Count();
        var eve = fxEventInstance[local_index];

        eve.setParameterByName("InputRegion", 6);
        // eve.setProperty(FMOD.Studio.EVENT_PROPERTY.SCHEDULE_DELAY, delay);

        eve.start();

        ++indexx;
        //single_click_musics[GetIndex(l)].Play();
    }

    public void SwipClickAt(Vector3 l)
    {

        // swip_musics[GetIndex(l)].Play();
    }

    public void HoldAt(Vector3 l)
    {
        var audio_source = long_click_musics[GetIndex(l)];
        if (!audio_source.isPlaying)
            audio_source.Play();
    }

    public void HoldStopAt(Vector3 l)
    {
        var audio_source = long_click_musics[GetIndex(l)];
        audio_source.Stop();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
