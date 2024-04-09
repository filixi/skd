using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        for (int i = 0; i < 10; ++i)
        {
            single_click_musics.Add(AddAudioSource("SingleClick" + i, "SoundEffect/SoundFX/Click/" + (i + 1)));
            long_click_musics.Add(AddAudioSource("LongClick" + i, "SoundEffect/SoundFX/Hold/" + (i + 1)));
            swip_musics.Add(AddAudioSource("Swip" + i, "SoundEffect/SoundFX/Flick/" + (i + 1)));
        }

        foreach (var x in long_click_musics)
            x.loop = true;
    }

    private int GetIndex(Vector3 l)
    {
        var index = Mathf.Clamp((int)Mathf.Floor(l.x / 4) + 3, 0, 4) + (l.z < 0 ? 5 : 0);
        Debug.Log("MusicIndex: " + index);
        return index;
    }

    public void SingleClickAt(Vector3 l)
    {
        single_click_musics[GetIndex(l)].Play();
    }

    public void SwipClickAt(Vector3 l)
    {
        swip_musics[GetIndex(l)].Play();
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
