using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class MainMenuControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    HashSet<GameObject> widgets = new HashSet<GameObject>();
    void AddWidget(GameObject go)
    {
        if (!go)
            return;
        widgets.Add(go);
    }

    EventInstance? bgm;

    void Update()
    {
        if (bgm == null)
            bgm = RuntimeManager.CreateInstance("event:/BGM/BGM_MainMenu");

        if (bgm != null)
        {
            bgm.Value.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.STOPPED)
                bgm.Value.start();
        }

        AddWidget(GameObject.Find("Level2"));
        AddWidget(GameObject.Find("Level3"));
        AddWidget(GameObject.Find("Level4"));
        AddWidget(GameObject.Find("HardMode"));

        foreach (var go in widgets)
            go.SetActive(GameInstance.GetInstance().Succeed());
    }

    public void StartLevel1()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level1";
        gi.fx_event = "event:/FX/FX_Level1";
        gi.level_name = "Level1";

        if (bgm != null)
            bgm.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Level1");
    }

    public void StartLevel2()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level2";
        gi.fx_event = "event:/FX/FX_Level2";
        gi.level_name = "Level2";

        if (bgm != null)
            bgm.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Level2");
    }

    public void StartLevel3()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level3";
        gi.fx_event = "event:/FX/FX_Level3";
        gi.level_name = "Level3";

        if (bgm != null)
            bgm.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Level3");
    }

    public void StartLevel4()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = true;
        gi.bgm_event = "event:/BGM/BGM_Level4";
        gi.fx_event = "event:/FX/FX_Level4";
        gi.level_name = "Level4";

        if (bgm != null)
            bgm.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Level4");
    }

    public bool IsHardMode()
    {
        var go = GameObject.Find("HardMode");
        if (go == null)
            return false;
        var toggle = go.GetComponent<Toggle>();
        if (toggle == null)
            return false;
        return toggle.isOn;
    }
}
